using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IEnvioRepository : IRepository<Envio>
    {
        Task<IEnumerable<Envio>> GetByRepartidorIdAsync(int repartidorId);
        Task<IEnumerable<Envio>> GetEnviosPendientesAsync();
        Task<Envio?> GetByPedidoIdAsync(int pedidoId);
    }
}