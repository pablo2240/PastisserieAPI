using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal IVA { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(500)]
        public string? RutaArchivo { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;
    }
}