using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public int MetodoPagoId { get; set; }

        public int? DireccionEnvioId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoEnvio { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public bool Aprobado { get; set; } = false;

        public DateTime? FechaAprobacion { get; set; }

        public DateTime? FechaEntregaEstimada { get; set; }

        [MaxLength(1000)]
        public string? NotasCliente { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        [ForeignKey("MetodoPagoId")]
        public virtual MetodoPagoUsuario MetodoPago { get; set; } = null!;

        [ForeignKey("DireccionEnvioId")]
        public virtual DireccionEnvio? DireccionEnvio { get; set; }

        public virtual ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
        public virtual Factura? Factura { get; set; }
        public virtual Envio? Envio { get; set; }
        public virtual ICollection<PedidoHistorial> Historial { get; set; } = new List<PedidoHistorial>();
    }
}