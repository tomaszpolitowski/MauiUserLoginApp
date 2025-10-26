using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace UserAppMaui.Services
{
   
    internal sealed class DebugLoggingHandler : DelegatingHandler
    {
        public DebugLoggingHandler(HttpMessageHandler inner) : base(inner) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Debug.WriteLine($"[HTTP] {request.Method} {request.RequestUri}");

            foreach (var h in request.Headers)
                Debug.WriteLine($"[HTTP] H {h.Key}: {MaskAuth(h.Key, h.Value)}");

            if (request.Content != null)
            {
                var outBody = await request.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(outBody))
                    Debug.WriteLine($"[HTTP] BODY OUT: {Truncate(outBody, 2000)}");
            }

            var res = await base.SendAsync(request, ct);

            string inBody = string.Empty;
            if (res.Content != null)
                inBody = await res.Content.ReadAsStringAsync(ct);

            Debug.WriteLine($"[HTTP {(int)res.StatusCode}] {res.ReasonPhrase} BODY IN: {Truncate(inBody, 2000)}");

            return res;
        }

        private static string MaskAuth(string key, IEnumerable<string> values)
            => key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                ? "Bearer …(masked)"
                : string.Join(",", values);

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…(truncated)";
    }

   
    internal sealed class TokenHandler : DelegatingHandler
    {
        public TokenHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await SecureStorage.GetAsync("jwt");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

   
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true 
        };

        private const string MePath = "api/Users/me";

        public ApiClient(string baseUrl)
        {
            var httpHandler = new HttpClientHandler();
            var chain = new TokenHandler(new DebugLoggingHandler(httpHandler));

            _http = new HttpClient(chain)
            {
                BaseAddress = new Uri(baseUrl, UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(30)
            };

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public record UserDto
        {
            public int Id { get; init; }
            public string Email { get; init; } = "";
            public string FirstName { get; init; } = "";
            public string LastName { get; init; } = "";

            [JsonPropertyName("createdAt")]
            public string CreatedAt { get; init; } = "";
        }

        public async Task<(bool ok, string? message)> Register(string email, string password, string confirm, string firstName, string lastName)
        {
            var body = new { email, password, confirmPassword = confirm, firstName, lastName };
            var res = await _http.PostAsync("api/Auth/register",
                new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json"));

            var txt = await SafeReadAsync(res);
            Debug.WriteLine($"[REGISTER STATUS {(int)res.StatusCode}] BODY: {txt}");

            if (res.IsSuccessStatusCode) return (true, null);
            return (false, ExtractMessage(txt) ?? $"Błąd {(int)res.StatusCode}");
        }

        public async Task<(bool ok, string? message)> Login(string email, string password)
        {
            var body = new { email, password };
            var res = await _http.PostAsync("api/Auth/login",
                new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json"));

            var txt = await SafeReadAsync(res);
            Debug.WriteLine($"[LOGIN STATUS {(int)res.StatusCode}] BODY: {txt}");

            if (!res.IsSuccessStatusCode)
                return (false, ExtractMessage(txt) ?? $"Błąd logowania: {(int)res.StatusCode}");

            using var doc = JsonDocument.Parse(txt);
            string? token = null;
            foreach (var key in new[] { "token", "accessToken", "jwt", "access_token", "idToken" })
            {
                if (doc.RootElement.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String)
                {
                    token = v.GetString();
                    if (!string.IsNullOrWhiteSpace(token)) break;
                }
            }

            if (string.IsNullOrWhiteSpace(token))
                return (false, "Nie znaleziono tokenu w odpowiedzi.");

            await SecureStorage.SetAsync("jwt", token);
            Debug.WriteLine($"[LOGIN] Zapisano token (len={token.Length})");

            return (true, null);
        }

        public async Task<(UserDto? user, string? message)> Me()
        {
            var res = await _http.GetAsync(MePath);
            var txt = await SafeReadAsync(res);
            Debug.WriteLine($"[ME STATUS {(int)res.StatusCode}] BODY: {txt}");

            if (!res.IsSuccessStatusCode)
            {
                var msg = ExtractMessage(txt) ?? $"HTTP {(int)res.StatusCode}: {txt}";
                return (null, msg);
            }

            try
            {
                var user = JsonSerializer.Deserialize<UserDto>(txt, _json);
                return (user, null);
            }
            catch (Exception ex)
            {
                return (null, $"Błąd deserializacji odpowiedzi: {ex.Message}");
            }
        }

        public Task Logout()
        {
            SecureStorage.Remove("jwt");
            return Task.CompletedTask;
        }

        private static async Task<string> SafeReadAsync(HttpResponseMessage res)
        {
            try { return await res.Content.ReadAsStringAsync(); }
            catch { return string.Empty; }
        }

        private static string? ExtractMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                        return m.GetString();

                    if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                        return d.GetString();

                    if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in errs.EnumerateObject())
                        {
                            var first = prop.Value.EnumerateArray().FirstOrDefault();
                            if (first.ValueKind == JsonValueKind.String)
                                return $"{prop.Name}: {first.GetString()}";
                        }
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
