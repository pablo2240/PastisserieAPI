namespace PastisserieAPI.Services.DTOs.Response
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public bool EmailVerificado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}