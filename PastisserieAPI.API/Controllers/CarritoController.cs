using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoService _carritoService;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(ICarritoService carritoService, ILogger<CarritoController> logger)
        {
            _carritoService = carritoService;
            _logger = logger;
        }

        private int GetUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        /// <summary>
        /// Obtener carrito del usuario autenticado
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCarrito()
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<CarritoResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var carrito = await _carritoService.GetByUsuarioIdAsync(usuarioId);

                return Ok(ApiResponse<CarritoResponseDto>.SuccessResponse(carrito!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carrito");
                return StatusCode(500, ApiResponse<CarritoResponseDto>.ErrorResponse(
                    "Error al obtener carrito",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Agregar producto al carrito
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCarritoRequestDto request)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<CarritoResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var carrito = await _carritoService.AddItemAsync(usuarioId, request);

                return Ok(ApiResponse<CarritoResponseDto>.SuccessResponse(
                    carrito,
                    "Producto agregado al carrito"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar item al carrito");
                return StatusCode(500, ApiResponse<CarritoResponseDto>.ErrorResponse(
                    "Error al agregar producto al carrito",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Actualizar cantidad de un item
        /// </summary>
        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCarritoItemRequestDto request)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<CarritoResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var carrito = await _carritoService.UpdateItemAsync(usuarioId, itemId, request);

                if (carrito == null)
                {
                    return NotFound(ApiResponse<CarritoResponseDto>.ErrorResponse(
                        "Item no encontrado en el carrito"
                    ));
                }

                return Ok(ApiResponse<CarritoResponseDto>.SuccessResponse(
                    carrito,
                    "Cantidad actualizada"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar item del carrito");
                return StatusCode(500, ApiResponse<CarritoResponseDto>.ErrorResponse(
                    "Error al actualizar item",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Eliminar item del carrito
        /// </summary>
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("Usuario no autenticado"));
                }

                var result = await _carritoService.RemoveItemAsync(usuarioId, itemId);

                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Item no encontrado"));
                }

                return Ok(ApiResponse.SuccessResponse("Item eliminado del carrito"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar item del carrito");
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "Error al eliminar item",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Vaciar carrito
        /// </summary>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCarrito()
        {
            try
            {
                var usuarioId = GetUsuarioId();

                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("Usuario no autenticado"));
                }

                var result = await _carritoService.ClearCarritoAsync(usuarioId);

                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse("Carrito no encontrado"));
                }

                return Ok(ApiResponse.SuccessResponse("Carrito vaciado"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al vaciar carrito");
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "Error al vaciar carrito",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}