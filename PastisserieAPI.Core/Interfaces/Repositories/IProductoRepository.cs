using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetByCategoriaAsync(string categoria);
        Task<IEnumerable<Producto>> GetPersonalizablesAsync();
        Task<IEnumerable<Producto>> GetProductosActivosAsync();
        Task<IEnumerable<Producto>> GetProductosBajoStockAsync();
        Task<Producto?> GetByIdWithReviewsAsync(int id);
    }
}