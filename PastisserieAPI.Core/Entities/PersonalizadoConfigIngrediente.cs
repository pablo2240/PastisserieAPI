using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class PersonalizadoConfigIngrediente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonalizadoConfigId { get; set; }

        [Required]
        public int IngredienteId { get; set; }

        [Required]
        public int Cantidad { get; set; } = 1;

        // Relaciones
        [ForeignKey("PersonalizadoConfigId")]
        public virtual PersonalizadoConfig PersonalizadoConfig { get; set; } = null!;

        [ForeignKey("IngredienteId")]
        public virtual Ingrediente Ingrediente { get; set; } = null!;
    }
}