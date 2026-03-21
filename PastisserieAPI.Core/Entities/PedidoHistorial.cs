using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class PedidoHistorial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EstadoAnterior { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EstadoNuevo { get; set; } = string.Empty;

        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        public int? CambiadoPor { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;
    }
}