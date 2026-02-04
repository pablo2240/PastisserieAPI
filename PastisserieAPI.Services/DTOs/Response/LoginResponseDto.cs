namespace PastisserieAPI.Services.DTOs.Response
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }
}