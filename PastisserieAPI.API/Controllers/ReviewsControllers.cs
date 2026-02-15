using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    /// <summary>
    /// Controlador para gestión de reseñas y calificaciones de productos
    /// Permite crear, consultar y aprobar reviews
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Helper method para obtener el ID del usuario autenticado
        /// </summary>
        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// Helper method para obtener el rol del usuario autenticado
        /// </summary>
        private string GetUsuarioRol()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? string.Empty;
        }

        /// <summary>
        /// Crear una nueva reseña
        /// Requiere autenticación
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequestDto request)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<ReviewResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var review = await _reviewService.CreateAsync(usuarioId, request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = review.Id },
                    ApiResponse<ReviewResponseDto>.SuccessResponse(
                        review,
                        "Reseña creada exitosamente. Estará visible una vez sea aprobada por un administrador."
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reseña para producto {ProductoId}", request.ProductoId);
                return StatusCode(500, ApiResponse<ReviewResponseDto>.ErrorResponse(
                    "Error al crear reseña",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener reseña por ID
        /// Público para reviews aprobadas
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);

                if (review == null)
                {
                    return NotFound(ApiResponse<ReviewResponseDto>.ErrorResponse(
                        $"Reseña con ID {id} no encontrada"
                    ));
                }

                // Si la review no está aprobada, solo el autor o admin pueden verla
                if (!review.Aprobada)
                {
                    var usuarioId = GetUsuarioId();
                    var rol = GetUsuarioRol();

                    if (review.UsuarioId != usuarioId && rol != "Admin" && rol != "Gerente")
                    {
                        return NotFound(ApiResponse<ReviewResponseDto>.ErrorResponse(
                            $"Reseña con ID {id} no encontrada"
                        ));
                    }
                }

                return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(review));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reseña con ID {Id}", id);
                return StatusCode(500, ApiResponse<ReviewResponseDto>.ErrorResponse(
                    "Error al obtener reseña",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener todas las reseñas de un producto
        /// Público - Solo retorna reviews aprobadas
        /// </summary>
        [HttpGet("producto/{productoId}")]
        public async Task<IActionResult> GetByProductoId(int productoId)
        {
            try
            {
                // Por defecto solo retorna reviews aprobadas para usuarios públicos
                var reviews = await _reviewService.GetByProductoIdAsync(productoId, soloAprobadas: true);

                return Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(
                    reviews,
                    $"Se encontraron {reviews.Count} reseñas para el producto"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reseñas del producto {ProductoId}", productoId);
                return StatusCode(500, ApiResponse<List<ReviewResponseDto>>.ErrorResponse(
                    "Error al obtener reseñas",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener promedio de calificación de un producto
        /// Público
        /// </summary>
        [HttpGet("producto/{productoId}/promedio")]
        public async Task<IActionResult> GetPromedioCalificacion(int productoId)
        {
            try
            {
                var promedio = await _reviewService.GetPromedioCalificacionAsync(productoId);

                return Ok(ApiResponse<double>.SuccessResponse(
                    promedio,
                    $"Promedio de calificación: {promedio:F1}/5"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener promedio de calificación del producto {ProductoId}", productoId);
                return StatusCode(500, ApiResponse<double>.ErrorResponse(
                    "Error al obtener promedio de calificación",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener reseñas del usuario autenticado
        /// Requiere autenticación
        /// </summary>
        [Authorize]
        [HttpGet("mis-reviews")]
        public async Task<IActionResult> GetMisReviews()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<List<ReviewResponseDto>>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var reviews = await _reviewService.GetByUsuarioIdAsync(usuarioId);

                return Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(
                    reviews,
                    $"Se encontraron {reviews.Count} reseñas"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reseñas del usuario {UsuarioId}", GetUsuarioId());
                return StatusCode(500, ApiResponse<List<ReviewResponseDto>>.ErrorResponse(
                    "Error al obtener reseñas",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener reseñas pendientes de aprobación
        /// Solo Admin y Gerente
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            try
            {
                var reviews = await _reviewService.GetPendientesAprobacionAsync();

                return Ok(ApiResponse<List<ReviewResponseDto>>.SuccessResponse(
                    reviews,
                    $"Hay {reviews.Count} reseñas pendientes de aprobación"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reseñas pendientes");
                return StatusCode(500, ApiResponse<List<ReviewResponseDto>>.ErrorResponse(
                    "Error al obtener reseñas pendientes",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Aprobar una reseña
        /// Solo Admin y Gerente
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var aprobada = await _reviewService.AprobarAsync(id, usuarioId);

                return Ok(ApiResponse<bool>.SuccessResponse(
                    true,
                    "Reseña aprobada exitosamente"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar reseña {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Error al aprobar reseña",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Eliminar una reseña
        /// Solo Admin o el usuario que la creó
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var review = await _reviewService.GetByIdAsync(id);
                if (review == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        $"Reseña con ID {id} no encontrada"
                    ));
                }

                var usuarioId = GetUsuarioId();
                var rol = GetUsuarioRol();

                // Solo el autor o admin pueden eliminar
                if (review.UsuarioId != usuarioId && rol != "Admin" && rol != "Gerente")
                {
                    return Forbid();
                }

                var eliminada = await _reviewService.DeleteAsync(id);

                return Ok(ApiResponse<bool>.SuccessResponse(
                    true,
                    "Reseña eliminada exitosamente"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar reseña {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Error al eliminar reseña",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}