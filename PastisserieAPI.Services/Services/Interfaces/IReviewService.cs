using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewResponseDto>> GetByProductIdAsync(int productId);
        Task<List<ReviewResponseDto>> GetFeaturedAsync(); // Para el Home
        Task<ReviewResponseDto?> GetMyReviewAsync(int productId, int userId);
        Task<ReviewResponseDto> CreateAsync(int userId, CreateReviewRequestDto request);
        Task<ReviewResponseDto> UpdateAsync(int reviewId, int userId, UpdateReviewRequestDto request);
        Task<IEnumerable<ReviewResponseDto>> GetPendingAsync();
        Task ApproveAsync(int reviewId);
        Task DeleteAsync(int reviewId);
    }
}