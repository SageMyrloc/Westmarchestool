using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Westmarchestool.API.Data;
using Westmarchestool.API.DTOs;
using Westmarchestool.API.Models;

namespace Westmarchestool.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return null; // Username taken
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create user
            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign default "Player" role
            var playerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Player");
            if (playerRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = playerRole.Id
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            // Generate token
            var roles = new List<string> { "Player" };
            var token = GenerateJwtToken(user.Id, user.Username, roles);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return null; // User not found
            }

            // Check if account is locked
            if (user.IsLocked && user.LockoutEnd > DateTime.UtcNow)
            {
                return null; // Account is locked
            }

            // Reset lockout if expired
            if (user.IsLocked && user.LockoutEnd <= DateTime.UtcNow)
            {
                user.IsLocked = false;
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                // Increment failed attempts
                user.FailedLoginAttempts++;

                // Lock account after 10 failed attempts
                if (user.FailedLoginAttempts >= 10)
                {
                    user.IsLocked = true;
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                }

                await _context.SaveChangesAsync();
                return null; // Invalid password
            }

            // Reset failed attempts on successful login
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

            // Get roles
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            // Generate token
            var token = GenerateJwtToken(user.Id, user.Username, roles);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Roles = roles,
                ExpiresAt = DateTime.UtcNow.AddHours(
                    _configuration.GetValue<int>("JwtSettings:ExpirationHours"))
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedDate = user.CreatedDate,
                IsLocked = user.IsLocked
            };
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsLocked = false;
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(int userId, string username, List<string> roles)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    _configuration.GetValue<int>("JwtSettings:ExpirationHours")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}