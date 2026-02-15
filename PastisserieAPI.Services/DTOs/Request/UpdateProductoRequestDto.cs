namespace PastisserieAPI.Services.DTOs.Request
{
    public class UpdateProductoRequestDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? Stock { get; set; }
        public int? StockMinimo { get; set; }
        public int? CategoriaProductoId { get; set; } // FK a CategoriaProducto
        public string? ImagenUrl { get; set; }
        public bool? Activo { get; set; }
    }
}