using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Westmarchestool.API.DTOs;
using Westmarchestool.API.Services;
using Westmarchestool.Core.Entities;
using Westmarchestool.Infrastructure.Data;

namespace Westmarchestool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return BadRequest(new { message = "Username already taken" });
            }

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password, or account is locked" });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("unlock/{userId}")]

        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(dto.UserId, dto.NewPassword);

            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        public async Task<IActionResult> UnlockUser(int userId)
        {
            var result = await _authService.UnlockUserAsync(userId);

            if (!result)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(new { message = "User unlocked successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/Auth/roles
        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        // POST: api/Auth/assign-role
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user exists
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if role exists
            var role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            // Check if user already has this role
            var existingRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

            if (existingRole)
            {
                return BadRequest(new { message = $"User already has the {role.Name} role" });
            }

            // Add role
            var userRole = new UserRole
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{role.Name} role assigned to user successfully" });
        }

        // DELETE: api/Auth/remove-role
        [Authorize(Roles = "Admin")]
        [HttpDelete("remove-role")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

            if (userRole == null)
            {
                return NotFound(new { message = "User does not have this role" });
            }

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role removed successfully" });
        }

        [Authorize]
        [HttpPost("make-me-admin")]
        public async Task<IActionResult> MakeMeAdmin()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            // Get admin role
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return NotFound(new { message = "Admin role not found" });

            // Check if already admin
            var existingRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == adminRole.Id);

            if (existingRole)
            {
                return Ok(new { message = "You are already an admin" });
            }

            // Add admin role
            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = adminRole.Id,
                AssignedDate = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return Ok(new { message = "You are now an admin! Please log in again to get new token with admin role." });
        }
    }

    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}