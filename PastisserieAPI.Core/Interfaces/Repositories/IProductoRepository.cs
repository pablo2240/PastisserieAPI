using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        /// Obtener productos por ID de categoría
        Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);

        /// Obtener productos activos
        Task<IEnumerable<Producto>> GetProductosActivosAsync();

        /// Obtener productos con bajo stock
        Task<IEnumerable<Producto>> GetProductosBajoStockAsync();

        /// Obtener producto con sus reviews
        Task<Producto?> GetByIdWithReviewsAsync(int id);

        /// Obtener producto con categoría incluida
        Task<Producto?> GetByIdWithCategoriaAsync(int id);

        /// Obtener todos los productos con categoría incluida
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();


        // ========== NUEVOS PARA F2 ==========

        /// Buscar productos por nombre (insensible a mayúsculas)
        Task<IEnumerable<Producto>> SearchByNameAsync(string nombre);

        /// Filtrar productos por rango de precio
        Task<IEnumerable<Producto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax);


        /// Obtener productos disponibles (stock > 0)
        Task<IEnumerable<Producto>> GetDisponiblesAsync();

        /// Búsqueda avanzada con múltiples filtros
        Task<IEnumerable<Producto>> SearchAsync(
            string? nombre = null,
            int? categoriaId = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            bool? soloDisponibles = null
        );
    }
}
    
