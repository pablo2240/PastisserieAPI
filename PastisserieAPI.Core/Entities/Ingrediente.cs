using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Ingrediente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioAdicional { get; set; } = 0;

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<PersonalizadoConfigIngrediente> ConfiguracionesPersonalizadas { get; set; } = new List<PersonalizadoConfigIngrediente>();
    }
}
