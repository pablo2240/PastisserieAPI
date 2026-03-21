namespace PastisserieAPI.Services.DTOs.Response
{
    public class CarritoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public List<CarritoItemResponseDto> Items { get; set; } = new();
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
    }

    public class CarritoItemResponseDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public string? ImagenUrl { get; set; }
    }
}