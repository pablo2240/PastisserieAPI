using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Envio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        public int? RepartidorId { get; set; }

        [MaxLength(100)]
        public string? NumeroGuia { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaDespacho { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;

        [ForeignKey("RepartidorId")]
        public virtual User? Repartidor { get; set; }
    }
}