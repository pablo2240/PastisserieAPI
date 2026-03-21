using System.ComponentModel.DataAnnotations;

namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreateReclamacionRequestDto
    {
        [Required]
        public int PedidoId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Motivo { get; set; } = string.Empty;
    }

    public class UpdateEstadoReclamacionDto
    {
        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = string.Empty;
    }
}
