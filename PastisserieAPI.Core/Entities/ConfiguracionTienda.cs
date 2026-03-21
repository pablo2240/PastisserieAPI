using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class ConfiguracionTienda
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NombreTienda { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EmailContacto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoEnvio { get; set; }

        [MaxLength(50)]
        public string Moneda { get; set; } = "COP";

        [MaxLength(500)]
        public string MensajeBienvenida { get; set; } = string.Empty;

        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        // ============ HORARIO LABORAL ============
        /// <summary>Si false, cierra la tienda independientemente del horario.</summary>
        public bool SistemaActivoManual { get; set; } = true;

        /// <summary>Si true, valida el horario automático.</summary>
        public bool UsarControlHorario { get; set; } = true;

        /// <summary>Hora de apertura (Medellín).</summary>
        public TimeSpan HoraApertura { get; set; } = new TimeSpan(8, 0, 0);

        /// <summary>Hora de cierre (Medellín).</summary>
        public TimeSpan HoraCierre { get; set; } = new TimeSpan(18, 0, 0);

        /// <summary>Días laborales separados por coma (ej: 1,2,3,4,5). 0=Domingo, 1=Lunes...</summary>
        [MaxLength(50)]
        public string DiasLaborales { get; set; } = "1,2,3,4,5,6";

        [Obsolete("Usar UsarControlHorario")]
        public bool HorarioActivo { get; set; } = true;

        [Obsolete("Usar HoraApertura")]
        [MaxLength(5)]
        public string HorarioApertura { get; set; } = "08:00";

        [Obsolete("Usar HoraCierre")]
        [MaxLength(5)]
        public string HorarioCierre { get; set; } = "18:00";

        public virtual ICollection<HorarioDia> HorariosPorDia { get; set; } = new List<HorarioDia>();

        // ============ REDES SOCIALES ============
        [MaxLength(200)]
        public string? InstagramUrl { get; set; }

        [MaxLength(200)]
        public string? FacebookUrl { get; set; }

        [MaxLength(200)]
        public string? WhatsappUrl { get; set; }
    }
}
