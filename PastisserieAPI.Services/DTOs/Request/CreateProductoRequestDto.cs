namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreateProductoRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int? StockMinimo { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public bool EsPersonalizable { get; set; } = false;
    }
}