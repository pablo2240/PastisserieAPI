using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class CarritoCompra
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        public virtual ICollection<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }
}