using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<UserResponseDto?> RegisterAsync(RegisterRequestDto request);
        Task<UserResponseDto?> CreateUserAsync(CreateUserRequestDto request);
        Task<bool> ChangeUserRoleAsync(int userId, string nuevoRol);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> UpdateProfileAsync(int userId, UpdateUserRequestDto request);
        Task<string> ForgotPasswordAsync(string email);
        Task<bool> ValidateResetTokenAsync(string email, string token);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
