using System.ComponentModel.DataAnnotations;

namespace PastisserieAPI.Core.Entities
{
    public class TipoMetodoPago
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<MetodoPagoUsuario> MetodosPagoUsuarios { get; set; } = new List<MetodoPagoUsuario>();
    }
}