using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Data;

namespace PastisserieAPI.Infrastructure.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtener productos por ID de categoría
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.CategoriaProductoId == categoriaId && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos activos
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosActivosAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos con bajo stock
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosBajoStockAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.StockMinimo.HasValue && p.Stock <= p.StockMinimo.Value)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener producto con sus reviews
        /// </summary>
        public async Task<Producto?> GetByIdWithReviewsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Include(p => p.Reviews.Where(r => r.Aprobada)) // Solo reviews aprobadas
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtener producto con categoría incluida
        /// </summary>
        public async Task<Producto?> GetByIdWithCategoriaAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtener todos los productos con categoría incluida
        /// </summary>
        public async Task<IEnumerable<Producto>> GetAllWithCategoriaAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Override del GetByIdAsync para incluir categoría por defecto
        /// </summary>
        public override async Task<Producto?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Override del GetAllAsync para incluir categoría por defecto
        /// </summary>
        public override async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Buscar productos por nombre
        /// </summary>
        public async Task<IEnumerable<Producto>> SearchByNameAsync(string nombre)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .Where(p => p.Activo &&
                            p.Nombre.ToLower().Contains(nombre.ToLower()))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Filtrar por rango de precio
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .Where(p => p.Activo &&
                            p.Precio >= precioMin &&
                            p.Precio <= precioMax)
                .OrderBy(p => p.Precio)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos disponibles (stock > 0)
        /// </summary>
        public async Task<IEnumerable<Producto>> GetDisponiblesAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .Where(p => p.Activo && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Búsqueda avanzada con múltiples filtros
        /// </summary>
        public async Task<IEnumerable<Producto>> SearchAsync(
            string? nombre = null,
            int? categoriaId = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            bool? soloDisponibles = null)
        {
            var query = _dbSet
                .Include(p => p.CategoriaProducto)
                .Where(p => p.Activo)
                .AsQueryable();

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
            }

            // Filtro por categoría
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaProductoId == categoriaId.Value);
            }

            // Filtro por rango de precio
            if (precioMin.HasValue)
            {
                query = query.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                query = query.Where(p => p.Precio <= precioMax.Value);
            }

            // Filtro por disponibilidad
            if (soloDisponibles == true)
            {
                query = query.Where(p => p.Stock > 0);
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }
    }
}