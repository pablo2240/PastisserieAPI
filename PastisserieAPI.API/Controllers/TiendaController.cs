using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.Services.Interfaces;
using PastisserieAPI.Services.DTOs.Common;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiendaController : ControllerBase
    {
        private readonly ITiendaService _tiendaService;

        public TiendaController(ITiendaService tiendaService)
        {
            _tiendaService = tiendaService;
        }

        [HttpGet("estado")]
        public async Task<IActionResult> GetEstado()
        {
            var config = await _tiendaService.GetConfiguracionAsync();
            if (config == null)
            {
                return Ok(ApiResponse<object>.SuccessResponse(new { estaAbierto = true }, "Configuración no encontrada, asumiendo abierta"));
            }

            var estaAbierto = _tiendaService.EstaAbierto(config);
            
            return Ok(ApiResponse<object>.SuccessResponse(new 
            { 
                estaAbierto = estaAbierto,
                horaApertura = config.HoraApertura.ToString(@"hh\:mm"),
                horaCierre = config.HoraCierre.ToString(@"hh\:mm"),
                sistemaActivoManual = config.SistemaActivoManual,
                usarControlHorario = config.UsarControlHorario,
                diasLaborales = config.DiasLaborales,
                horariosPorDia = config.HorariosPorDia?.Select(h => new 
                {
                    diaSemana = h.DiaSemana,
                    abierto = h.Abierto,
                    horaApertura = h.HoraApertura.ToString(@"hh\:mm"),
                    horaCierre = h.HoraCierre.ToString(@"hh\:mm")
                }).OrderBy(h => h.diaSemana).ToList()
            }, "Estado de la tienda obtenido"));
        }
    }
}
