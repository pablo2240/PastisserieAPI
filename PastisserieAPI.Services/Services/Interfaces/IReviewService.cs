using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    /// <summary>
    /// Servicio para gestión de reseñas y calificaciones de productos
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Crear una nueva reseña para un producto
        /// </summary>
        /// <param name="usuarioId">ID del usuario que crea la reseña</param>
        /// <param name="request">Datos de la reseña</param>
        /// <returns>Reseña creada</returns>
        Task<ReviewResponseDto> CreateAsync(int usuarioId, CreateReviewRequestDto request);

        /// <summary>
        /// Obtener reseña por ID
        /// </summary>
        Task<ReviewResponseDto?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener todas las reseñas de un producto
        /// </summary>
        /// <param name="productoId">ID del producto</param>
        /// <param name="soloAprobadas">Si true, solo retorna reseñas aprobadas</param>
        /// <returns>Lista de reseñas del producto</returns>
        Task<List<ReviewResponseDto>> GetByProductoIdAsync(int productoId, bool soloAprobadas = true);

        /// <summary>
        /// Obtener todas las reseñas de un usuario
        /// </summary>
        Task<List<ReviewResponseDto>> GetByUsuarioIdAsync(int usuarioId);

        /// <summary>
        /// Obtener promedio de calificación de un producto
        /// </summary>
        Task<double> GetPromedioCalificacionAsync(int productoId);

        /// <summary>
        /// Obtener reseñas pendientes de aprobación (solo Admin/Gerente)
        /// </summary>
        Task<List<ReviewResponseDto>> GetPendientesAprobacionAsync();

        /// <summary>
        /// Aprobar una reseña (solo Admin/Gerente)
        /// </summary>
        /// <param name="id">ID de la reseña</param>
        /// <param name="aprobadaPor">ID del usuario que aprueba</param>
        /// <returns>True si se aprobó correctamente</returns>
        Task<bool> AprobarAsync(int id, int aprobadaPor);

        /// <summary>
        /// Eliminar una reseña (solo Admin o el usuario que la creó)
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}