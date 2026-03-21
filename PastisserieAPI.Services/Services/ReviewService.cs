using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ReviewResponseDto>> GetByProductIdAsync(int productId)
        {
            // CORRECCIÓN: Usamos el método específico del repositorio (GetByProductoIdAsync)
            // en lugar del genérico GetAsync que daba error.
            var reviews = await _unitOfWork.Reviews.GetByProductoIdAsync(productId);

            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }

        public async Task<List<ReviewResponseDto>> GetFeaturedAsync()
        {
            // CORRECCIÓN: Usamos el método específico para destacadas
            var reviews = await _unitOfWork.Reviews.GetFeaturedAsync();

            return _mapper.Map<List<ReviewResponseDto>>(reviews);
        }

        public async Task<ReviewResponseDto?> GetMyReviewAsync(int productId, int userId)
        {
            var review = await _unitOfWork.Reviews.GetByProductoYUsuarioAsync(productId, userId);
            return review == null ? null : _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<ReviewResponseDto> CreateAsync(int userId, CreateReviewRequestDto request)
        {
            var existing = await _unitOfWork.Reviews.GetByProductoYUsuarioAsync(request.ProductoId, userId);
            if (existing != null)
                throw new Exception("Ya has enviado una reseña para este producto.");

            var review = _mapper.Map<Review>(request);
            review.UsuarioId = userId;
            review.Fecha = DateTime.UtcNow;

            // AddAsync suele ser estándar en el repositorio base. 
            // Si te da error aquí, avísame, pero debería funcionar.
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Retornamos el DTO mapeado directamente
            return _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<ReviewResponseDto> UpdateAsync(int reviewId, int userId, UpdateReviewRequestDto request)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.UsuarioId != userId)
                throw new Exception("Reseña no encontrada o no autorizada.");

            review.Calificacion = request.Calificacion;
            review.Comentario = request.Comentario;
            // Optionally set Aprobada to false if editing requires re-approval
            review.Aprobada = false;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ReviewResponseDto>(review);
        }

        public async Task<IEnumerable<ReviewResponseDto>> GetPendingAsync()
        {
            var reviews = await _unitOfWork.Reviews.GetReviewsPendientesAprobacionAsync();
            return _mapper.Map<IEnumerable<ReviewResponseDto>>(reviews);
        }

        public async Task ApproveAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review != null)
            {
                review.Aprobada = true;
                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review != null)
            {
                await _unitOfWork.Reviews.DeleteAsync(review);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}