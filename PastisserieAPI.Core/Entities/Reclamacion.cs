using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Reclamacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(1000)]
        public string Motivo { get; set; } = string.Empty;

        /// <summary>Estado de la reclamación: Pendiente, EnRevision, Resuelta, Rechazada.</summary>
        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        // ============ RELACIONES ============
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;
    }
}
