using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public int? StockMinimo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        public bool EsPersonalizable { get; set; } = false;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();
        public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();
    }
}