using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using UserApi.Data;
using UserApi.Models;


namespace UserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;


        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> Me()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (sub is null) return Unauthorized();
            if (!int.TryParse(sub, out var userId)) return Unauthorized();


            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (u is null) return NotFound();


            return new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.CreatedAt.ToString("u"));
        }
    }
}