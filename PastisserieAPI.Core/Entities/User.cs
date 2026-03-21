using System.ComponentModel.DataAnnotations;

namespace PastisserieAPI.Core.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public bool EmailVerificado { get; set; } = false;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoAcceso { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        public virtual ICollection<UserRol> UserRoles { get; set; } = new List<UserRol>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
        public virtual CarritoCompra? CarritoCompra { get; set; }
        public virtual ICollection<DireccionEnvio> Direcciones { get; set; } = new List<DireccionEnvio>();
        public virtual ICollection<MetodoPagoUsuario> MetodosPago { get; set; } = new List<MetodoPagoUsuario>();
        public virtual ICollection<Envio> EnviosAsignados { get; set; } = new List<Envio>();
    }
}
