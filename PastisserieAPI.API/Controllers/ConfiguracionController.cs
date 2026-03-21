using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Common;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PastisserieAPI.Infrastructure.Data.ApplicationDbContext _context;

        public ConfiguracionController(IUnitOfWork unitOfWork, PastisserieAPI.Infrastructure.Data.ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfig()
        {
            var config = await _context.ConfiguracionTienda
                .Include(c => c.HorariosPorDia)
                .FirstOrDefaultAsync();
                
            return Ok(ApiResponse<ConfiguracionTienda>.SuccessResponse(config!, config == null ? "No se encontró configuración" : "Configuración cargada"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] ConfiguracionTienda newConfig)
        {
            var existingConfig = await _context.ConfiguracionTienda
                .Include(c => c.HorariosPorDia)
                .FirstOrDefaultAsync();
            
            if (existingConfig == null)
            {
                await _context.ConfiguracionTienda.AddAsync(newConfig);
            }
            else
            {
                existingConfig.NombreTienda = newConfig.NombreTienda;
                existingConfig.Direccion = newConfig.Direccion;
                existingConfig.Telefono = newConfig.Telefono;
                existingConfig.EmailContacto = newConfig.EmailContacto;
                existingConfig.CostoEnvio = newConfig.CostoEnvio;
                existingConfig.Moneda = newConfig.Moneda;
                existingConfig.MensajeBienvenida = newConfig.MensajeBienvenida;
                
                // Nuevos campos de control de horario
                existingConfig.HoraApertura = newConfig.HoraApertura;
                existingConfig.HoraCierre = newConfig.HoraCierre;
                existingConfig.SistemaActivoManual = newConfig.SistemaActivoManual;
                existingConfig.UsarControlHorario = newConfig.UsarControlHorario;
                existingConfig.DiasLaborales = newConfig.DiasLaborales;

                // Actualizar horarios
                if (newConfig.HorariosPorDia != null && newConfig.HorariosPorDia.Any())
                {
                    // Eliminar los anteriores
                    _context.HorariosPorDia.RemoveRange(existingConfig.HorariosPorDia);
                    existingConfig.HorariosPorDia.Clear();
                    
                    // Agregar los nuevos
                    foreach (var h in newConfig.HorariosPorDia)
                    {
                        h.Id = 0; // Para forzar creación
                        existingConfig.HorariosPorDia.Add(h);
                    }
                }

                // Redes sociales
                existingConfig.InstagramUrl = newConfig.InstagramUrl;
                existingConfig.FacebookUrl = newConfig.FacebookUrl;
                existingConfig.WhatsappUrl = newConfig.WhatsappUrl;

                existingConfig.FechaActualizacion = DateTime.UtcNow;
                _context.ConfiguracionTienda.Update(existingConfig);
            }

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<ConfiguracionTienda>.SuccessResponse(existingConfig ?? newConfig, "Configuración actualizada"));
        }
    }
}
