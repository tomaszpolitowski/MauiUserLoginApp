using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Models;
using UserApi.Services;


namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenService _jwt;
        private readonly PasswordHasher<User> _hasher = new();


        public AuthController(AppDbContext db, IJwtTokenService jwt)
        {
            _db = db; _jwt = jwt;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            if (req.Password != req.ConfirmPassword)
                return BadRequest(new { message = "Hasła nie są zgodne." });


            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return Conflict(new { message = "Użytkownik z takim e‑mailem już istnieje." });


            var user = new User
            {
                Email = req.Email.Trim().ToLowerInvariant(),
                FirstName = req.FirstName.Trim(),
                LastName = req.LastName.Trim()
            };
            user.PasswordHash = _hasher.HashPassword(user, req.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();


            return Ok(new { message = "Rejestracja zakończona sukcesem." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest req)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return Unauthorized(new { message = "Nieprawidłowy e‑mail lub hasło." });


            var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Nieprawidłowy e‑mail lub hasło." });


            var token = _jwt.CreateToken(user.Id, user.Email, user.FirstName, user.LastName);
            return Ok(new { token });
        }
    }
}