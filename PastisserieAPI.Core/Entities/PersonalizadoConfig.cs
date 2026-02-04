using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class PersonalizadoConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [MaxLength(100)]
        public string? Sabor { get; set; }

        [MaxLength(50)]
        public string? Tamano { get; set; }

        [MaxLength(50)]
        public string? Forma { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public int Niveles { get; set; } = 1;

        [MaxLength(500)]
        public string? Diseno { get; set; }

        [MaxLength(500)]
        public string? ImagenReferenciaUrl { get; set; }

        [MaxLength(1000)]
        public string? InstruccionesEspeciales { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioAdicional { get; set; } = 0;

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;

        public virtual ICollection<PersonalizadoConfigIngrediente> Ingredientes { get; set; } = new List<PersonalizadoConfigIngrediente>();
    }
}