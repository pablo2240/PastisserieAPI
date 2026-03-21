using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Infrastructure.Data;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class TiendaService : ITiendaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;

        public TiendaService(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<ConfiguracionTienda?> GetConfiguracionAsync()
        {
            return await _context.ConfiguracionTienda
                .Include(c => c.HorariosPorDia)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsStoreOpenAsync()
        {
            var config = await GetConfiguracionAsync();
            if (config == null) return true; // Default to open if no config found

            return EstaAbierto(config);
        }

        public bool EstaAbierto(ConfiguracionTienda config)
        {
            // 1. Si SistemaActivoManual es false → retornar false
            if (!config.SistemaActivoManual) return false;

            // 2. Si UsarControlHorario es false → retornar true (24/7)
            if (!config.UsarControlHorario) return true;

            // 3. Obtener hora actual en Medellín (TimeZoneInfo robusto)
            DateTime localNow;
            try {
                var medellinZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, medellinZone);
            } catch {
                // Fallback manual si la zona horaria no se encuentra en el OS (ej: Linux sin tzdata)
                localNow = DateTime.UtcNow.AddHours(-5);
            }
            
            var horaActual = localNow.TimeOfDay;
            var diaActualInt = (int)localNow.DayOfWeek;

            // 4. Validar HorariosPorDia si están configurados
            if (config.HorariosPorDia != null && config.HorariosPorDia.Any())
            {
                // Manejar Domingo como 0 o 7 de forma robusta
                var horarioDia = config.HorariosPorDia.FirstOrDefault(h => 
                    h.DiaSemana == diaActualInt || (diaActualInt == 0 && h.DiaSemana == 7));
                
                if (horarioDia == null || !horarioDia.Abierto) return false;

                // Normalizar a hh:mm para evitar problemas de precisión de milisegundos
                var apertura = new TimeSpan(horarioDia.HoraApertura.Hours, horarioDia.HoraApertura.Minutes, 0);
                var cierre = new TimeSpan(horarioDia.HoraCierre.Hours, horarioDia.HoraCierre.Minutes, 59); // Buffer de 59s al cierre
                var actual = new TimeSpan(horaActual.Hours, horaActual.Minutes, horaActual.Seconds);

                return actual >= apertura && actual <= cierre;
            }

            // 4.5 Fallback: DiasLaborales legacy
            var diaActualStr = diaActualInt.ToString();
            if (!string.IsNullOrEmpty(config.DiasLaborales) && !config.DiasLaborales.Contains(diaActualStr))
                return false;

            // 5. Fallback HoraApertura/HoraCierre globales
            var aperturaGlobal = new TimeSpan(config.HoraApertura.Hours, config.HoraApertura.Minutes, 0);
            var cierreGlobal = new TimeSpan(config.HoraCierre.Hours, config.HoraCierre.Minutes, 0);
            var actualNorm = new TimeSpan(horaActual.Hours, horaActual.Minutes, 0);
            return actualNorm >= aperturaGlobal && actualNorm <= cierreGlobal;
        }
    }
}
