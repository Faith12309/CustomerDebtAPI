using CustomerDebtAPI.Data;
using CustomerDebtAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerDebtAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new { user.Id, user.Username, user.Role });
        }

        // POST: api/User
        [HttpPost]
        public async Task<IActionResult> CreateUser(User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.Username) ||
                string.IsNullOrWhiteSpace(newUser.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var exists = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == newUser.Username.Trim().ToLower());

            if (exists)
            {
                return BadRequest("Username already exists.");
            }

            var user = new User
            {
                Username = newUser.Username.Trim(),
                Password = newUser.Password.Trim(),
                Role = string.IsNullOrWhiteSpace(newUser.Role) ? "Staff" : newUser.Role.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
             
            return Ok(new
            {
                message = "User created successfully.",
                id = user.Id,
                username = user.Username,
                role = user.Role
            });
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            user.Username = updatedUser.Username;
            user.Role = updatedUser.Role;

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                user.Password = updatedUser.Password;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully." });
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}