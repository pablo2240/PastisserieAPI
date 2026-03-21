namespace PastisserieAPI.Services.DTOs.Response
{
    public class DireccionEnvioResponseDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Barrio { get; set; }
        public string? Referencia { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public bool EsPredeterminada { get; set; }
    }
}