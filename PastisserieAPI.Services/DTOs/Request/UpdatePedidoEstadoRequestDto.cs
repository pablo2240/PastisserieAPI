namespace PastisserieAPI.Services.DTOs.Request
{
    public class UpdatePedidoEstadoRequestDto
    {
        public string Estado { get; set; } = string.Empty;
        public string? Notas { get; set; }
    }
}