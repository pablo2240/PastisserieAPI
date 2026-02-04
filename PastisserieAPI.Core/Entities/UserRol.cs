using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class UserRol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int RolId { get; set; }

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        [ForeignKey("RolId")]
        public virtual Rol Rol { get; set; } = null!;
    }
}