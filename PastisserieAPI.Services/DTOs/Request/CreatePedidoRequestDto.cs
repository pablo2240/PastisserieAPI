namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreatePedidoRequestDto
    {
        public int UsuarioId { get; set; }
        public int MetodoPagoId { get; set; }
        public int? DireccionEnvioId { get; set; }
        public string? NotasCliente { get; set; }
        public List<PedidoItemRequestDto> Items { get; set; } = new();
    }

    public class PedidoItemRequestDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    } 
}