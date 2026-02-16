using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class CarritoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarritoId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

        // ========== RESERVA TEMPORAL (RN2) ==========
        public DateTime? ReservaHasta { get; set; } // Null = no reservado, DateTime = reservado hasta

        // Relaciones
        [ForeignKey("CarritoId")]
        public virtual CarritoCompra Carrito { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
}