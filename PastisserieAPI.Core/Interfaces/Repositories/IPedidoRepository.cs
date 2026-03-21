using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IPedidoRepository : IRepository<Pedido>
    {
        Task<IEnumerable<Pedido>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<Pedido>> GetByEstadoAsync(string estado);
        Task<Pedido?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Pedido>> GetPedidosPendientesAsync();
        Task<IEnumerable<Pedido>> GetPedidosEnPreparacionAsync();
    }
}