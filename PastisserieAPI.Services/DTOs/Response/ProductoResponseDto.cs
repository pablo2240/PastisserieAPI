namespace PastisserieAPI.Services.DTOs.Response
{
    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int? StockMinimo { get; set; }

        // Categoría
        public int CategoriaProductoId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty; // Nombre de la categoría

        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }

        // ========== DISPONIBILIDAD (F1) ==========
        public string EstadoDisponibilidad { get; set; } = string.Empty; // "Disponible" | "Sin stock"
        public bool EstaDisponible => Stock > 0;

        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        // Reviews (si se incluyen)
        public double? CalificacionPromedio { get; set; }
        public int? TotalReviews { get; set; }
    }
}