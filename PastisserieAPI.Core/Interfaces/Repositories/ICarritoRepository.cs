using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface ICarritoRepository : IRepository<CarritoCompra>
    {
        Task<CarritoCompra?> GetByUsuarioIdAsync(int usuarioId);
        Task<CarritoCompra?> GetByUsuarioIdWithItemsAsync(int usuarioId);
        Task ClearCarritoAsync(int carritoId);
    }
}