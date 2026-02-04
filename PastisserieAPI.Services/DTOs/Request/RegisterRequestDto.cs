namespace PastisserieAPI.Services.DTOs.Request
{
    public class RegisterRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }
}