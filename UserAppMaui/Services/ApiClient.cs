using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserAppMaui.Services;

public class TokenHandler : DelegatingHandler
{
    public TokenHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await SecureStorage.GetAsync("jwt");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ApiClient()
    {
        var httpHandler = new HttpClientHandler();

#if ANDROID
        // Jeśli emulator krzyczy o certyfikacie DEV HTTPS, na czas DEV możesz odkomentować:
        // httpHandler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;
        var baseUrl = "https://10.0.2.2:7067/";
#else
        var baseUrl = "https://localhost:7067/";
#endif

        var handler = new TokenHandler(httpHandler);
        _http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<(bool ok, string? message)> Register(string email, string password, string confirm, string firstName, string lastName)
    {
        var body = new { email, password, confirmPassword = confirm, firstName, lastName };
        var res = await _http.PostAsync("api/Auth/register",
            new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json"));
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await ExtractMessage(res));
    }

    public async Task<(bool ok, string? message)> Login(string email, string password)
    {
        var body = new { email, password };
        var res = await _http.PostAsync("api/Auth/login",
            new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json"));
        var txt = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) return (false, ExtractFromJson(txt));

        using var doc = JsonDocument.Parse(txt);
        var token = doc.RootElement.GetProperty("token").GetString();
        if (!string.IsNullOrEmpty(token))
            await SecureStorage.SetAsync("jwt", token);
        return (true, null);
    }

    public record UserDto(int Id, string Email, string FirstName, string LastName, string CreatedAt);

    public async Task<(UserDto? user, string? message)> Me()
    {
        var res = await _http.GetAsync("api/Users/me");
        var txt = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) return (null, ExtractFromJson(txt));
        var user = JsonSerializer.Deserialize<UserDto>(txt, _json);
        return (user, null);
    }

    public Task Logout()
    {
        SecureStorage.Remove("jwt");
        return Task.CompletedTask;
    }

    private static async Task<string?> ExtractMessage(HttpResponseMessage res)
        => ExtractFromJson(await res.Content.ReadAsStringAsync());

    private static string? ExtractFromJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var m)) return m.GetString();
        }
        catch { }
        return null;
    }
}
