using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<IEnumerable<Review>> GetByProductoIdAsync(int productoId);
        Task<IEnumerable<Review>> GetByUsuarioIdAsync(int usuarioId);
        Task<double> GetPromedioCalificacionAsync(int productoId);
        Task<IEnumerable<Review>> GetReviewsPendientesAprobacionAsync();

        // 👇 NUEVO: Para el Home (Destacadas)
        Task<IEnumerable<Review>> GetFeaturedAsync();
        
        // 👇 NUEVO: Obtener reseña específica de un usuario para un producto
        Task<Review?> GetByProductoYUsuarioAsync(int productoId, int usuarioId);
    }
}