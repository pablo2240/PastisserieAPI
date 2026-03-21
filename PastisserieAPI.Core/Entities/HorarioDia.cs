using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class HorarioDia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ConfiguracionTiendaId { get; set; }

        /// <summary>0=Domingo, 1=Lunes, ..., 6=Sábado</summary>
        [Required]
        public int DiaSemana { get; set; }

        public bool Abierto { get; set; } = true;

        public TimeSpan HoraApertura { get; set; } = new TimeSpan(8, 0, 0);

        public TimeSpan HoraCierre { get; set; } = new TimeSpan(18, 0, 0);

        [ForeignKey("ConfiguracionTiendaId")]
        public virtual ConfiguracionTienda? ConfiguracionTienda { get; set; }
    }
}
