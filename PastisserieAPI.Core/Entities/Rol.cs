using System.ComponentModel.DataAnnotations;

namespace PastisserieAPI.Core.Entities
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<UserRol> UserRoles { get; set; } = new List<UserRol>();
    }
}