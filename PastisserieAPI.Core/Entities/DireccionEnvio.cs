using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class DireccionEnvio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Barrio { get; set; }

        [MaxLength(500)]
        public string? Referencia { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public bool EsPredeterminada { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}