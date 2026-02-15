using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    /// <summary>
    /// Servicio de negocio para gestión de reseñas
    /// Implementa validaciones y lógica de aprobación de reviews
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ReviewResponseDto> CreateAsync(int usuarioId, CreateReviewRequestDto request)
        {
            // Validar que el producto existe
            var producto = await _unitOfWork.Productos.GetByIdAsync(request.ProductoId);
            if (producto == null || !producto.Activo)
            {
                throw new Exception($"Producto con ID {request.ProductoId} no encontrado o inactivo");
            }

            // Validar que el usuario existe
            var usuario = await _unitOfWork.Users.GetByIdAsync(usuarioId);
            if (usuario == null || !usuario.Activo)
            {
                throw new Exception("Usuario no encontrado o inactivo");
            }

            // TODO: Validar que el usuario haya comprado el producto antes de reseñar
            // Esta validación puede agregarse consultando la tabla Pedidos

            // Crear la review
            var review = new Review
            {
                UsuarioId = usuarioId,
                ProductoId = request.ProductoId,
                Calificacion = request.Calificacion,
                Comentario = request.Comentario,
                Fecha = DateTime.UtcNow,
                Aprobada = false // Las reseñas requieren aprobación de admin
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Obtener review completa con relaciones
            var reviewCompleta = await _unitOfWork.Reviews.GetByIdAsync(review.Id);
            return _mapper.Map<ReviewResponseDto>(reviewCompleta!);
        }

        public async Task<ReviewResponseDto?> GetByIdAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            return review == null ? null : _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<List<ReviewResponseDto>> GetByProductoIdAsync(int productoId, bool soloAprobadas = true)
        {
            var reviews = await _unitOfWork.Reviews.GetByProductoIdAsync(productoId);

            // Filtrar solo aprobadas si se solicita
            if (soloAprobadas)
            {
                reviews = reviews.Where(r => r.Aprobada).ToList();
            }

            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }

        public async Task<List<ReviewResponseDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var reviews = await _unitOfWork.Reviews.GetByUsuarioIdAsync(usuarioId);
            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }

        public async Task<double> GetPromedioCalificacionAsync(int productoId)
        {
            // Obtener promedio de calificaciones solo de reviews aprobadas
            return await _unitOfWork.Reviews.GetPromedioCalificacionAsync(productoId);
        }

        public async Task<List<ReviewResponseDto>> GetPendientesAprobacionAsync()
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsPendientesAprobacionAsync();
            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }

        public async Task<bool> AprobarAsync(int id, int aprobadaPor)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                throw new Exception("Reseña no encontrada");
            }

            if (review.Aprobada)
            {
                throw new Exception("La reseña ya está aprobada");
            }

            review.Aprobada = true;
            review.AprobadaPor = aprobadaPor;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(id);
            if (review == null)
            {
                throw new Exception("Reseña no encontrada");
            }

            await _unitOfWork.Reviews.DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}