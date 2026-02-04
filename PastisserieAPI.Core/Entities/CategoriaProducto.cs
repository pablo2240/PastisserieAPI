using System.ComponentModel.DataAnnotations;

namespace PastisserieAPI.Core.Entities
{
    public class CategoriaProducto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public bool Activa { get; set; } = true;

        // Relaciones
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
