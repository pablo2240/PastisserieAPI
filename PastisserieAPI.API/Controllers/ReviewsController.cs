using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("producto/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var result = await _reviewService.GetByProductIdAsync(productId);
            return Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("producto/{productId}/mi-resena")]
        [Authorize]
        public async Task<IActionResult> GetMyReview(int productId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            var review = await _reviewService.GetMyReviewAsync(productId, userId);
            return Ok(ApiResponse<ReviewResponseDto?>.SuccessResponse(review));
        }

        [HttpGet]
        public async Task<IActionResult> GetFeatured()
        {
            var result = await _reviewService.GetFeaturedAsync();
            return Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(result));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            var result = await _reviewService.CreateAsync(userId, request);
            return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(result, "Reseña creada y enviada a moderación"));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            try
            {
                var result = await _reviewService.UpdateAsync(id, userId, request);
                return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(result, "Reseña actualizada y enviada a moderación"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpGet("pendientes")]
        [Authorize(Roles = "Admin,Administrador,Administrator")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _reviewService.GetPendingAsync();
            return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.SuccessResponse(result));
        }

        [HttpPut("{id}/aprobar")]
        [Authorize(Roles = "Admin,Administrador,Administrator")]
        public async Task<IActionResult> Approve(int id)
        {
            await _reviewService.ApproveAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("Reseña aprobada con éxito"));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Administrador,Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            await _reviewService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("Reseña eliminada con éxito"));
        }
    }
}