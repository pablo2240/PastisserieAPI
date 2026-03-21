using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface ITiendaService
    {
        bool EstaAbierto(ConfiguracionTienda config);
        Task<ConfiguracionTienda?> GetConfiguracionAsync();
        Task<bool> IsStoreOpenAsync();
    }
}
