using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class MetodoPagoUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int TipoMetodoPagoId { get; set; }

        [MaxLength(500)]
        public string? TokenPago { get; set; }

        [MaxLength(10)]
        public string? UltimosDigitos { get; set; }

        public DateTime? FechaExpiracion { get; set; }

        public bool EsPredeterminado { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        [ForeignKey("TipoMetodoPagoId")]
        public virtual TipoMetodoPago TipoMetodoPago { get; set; } = null!;

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}