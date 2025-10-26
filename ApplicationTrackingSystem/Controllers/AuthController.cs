using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApplicationTrackingSystem.Models;

namespace ApplicationTrackingSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        // Static users for demo (replace with database later)
        private readonly List<User> _users = new List<User>
        {
            new User { Id = 1, Username = "applicant1", Password = "password1", Role = "Applicant" },
            new User { Id = 2, Username = "botmimic", Password = "password2", Role = "BotMimic" },
            new User { Id = 3, Username = "admin", Password = "password3", Role = "Admin" },
        };

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            // Validate user credentials
            var user = _users.SingleOrDefault(u =>
                u.Username == login.Username &&
                u.Password == login.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("b8#TjLs$Ue!93dXpRk7zMbVqQwHgSxNcLmDaJfPqZaBgFhKs"); // Use same key as in Program.cs

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok (new
            {
                Token = jwtToken,
                Username = user.Username,
                Role = user.Role
            });
        }
    }

    // Request model for login
  
}
