using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace UserApi.Services
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiresMinutes { get; set; } = 60;
    }


    public interface IJwtTokenService
    {
        string CreateToken(int userId, string email, string firstName, string lastName);
    }


    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _opts;
        public JwtTokenService(IOptions<JwtOptions> opts) => _opts = opts.Value;


        public string CreateToken(int userId, string email, string firstName, string lastName)
        {
            var claims = new List<Claim>
{
new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
new Claim(JwtRegisteredClaimNames.Email, email),
new Claim("given_name", firstName),
new Claim("family_name", lastName)
};


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
            issuer: _opts.Issuer,
            audience: _opts.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opts.ExpiresMinutes),
            signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}