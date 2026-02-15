using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        /// <summary>
        /// Obtener productos por ID de categoría
        /// </summary>
        Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);

        /// <summary>
        /// Obtener productos activos
        /// </summary>
        Task<IEnumerable<Producto>> GetProductosActivosAsync();

        /// <summary>
        /// Obtener productos con bajo stock
        /// </summary>
        Task<IEnumerable<Producto>> GetProductosBajoStockAsync();

        /// <summary>
        /// Obtener producto con sus reviews
        /// </summary>
        Task<Producto?> GetByIdWithReviewsAsync(int id);

        /// <summary>
        /// Obtener producto con categoría incluida
        /// </summary>
        Task<Producto?> GetByIdWithCategoriaAsync(int id);

        /// <summary>
        /// Obtener todos los productos con categoría incluida
        /// </summary>
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
    }
}