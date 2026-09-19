using CustomerDebtAPI.Data;
using CustomerDebtAPI.DTOs;
using CustomerDebtAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomerDebtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var inputUsername = login.Username.Trim();
            var inputPassword = login.Password.Trim();

            // Step 1: find the user by username only (case-insensitive, trimmed).
            // Pulling this into memory avoids any SQL-side collation/padding quirks.
            var users = await _context.Users.ToListAsync();

            var user = users.FirstOrDefault(u =>
                u.Username.Trim().Equals(inputUsername, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Step 2: check the password separately, as a plain trimmed string comparison.
            if (!user.Password.Trim().Equals(inputPassword, StringComparison.Ordinal))
            {
                return Unauthorized("Invalid username or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                username = user.Username,
                role = user.Role
            });
        }
    }
}