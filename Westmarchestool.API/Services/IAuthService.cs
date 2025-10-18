using Westmarchestool.API.DTOs;

namespace Westmarchestool.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<List<UserDto>> GetAllUsersAsync();

    }
}