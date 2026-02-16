using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastisserieAPI.Services.DTOs.Request
{
    public class ProductoSearchDto
    {
        public string? Nombre { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public bool? SoloDisponibles { get; set; }
    }
}
