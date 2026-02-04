using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<UserResponseDto?> RegisterAsync(RegisterRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByEmailAsync(string email);
    }
}
