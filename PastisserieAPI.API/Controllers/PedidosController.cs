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
    /// Controlador para gestión de pedidos
    /// Maneja la creación, consulta y actualización de estados de pedidos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
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
        /// Crear un nuevo pedido
        /// Requiere autenticación
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePedidoRequestDto request)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<PedidoResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                // Asignar el usuario autenticado al pedido
                request.UsuarioId = usuarioId;

                var pedido = await _pedidoService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = pedido.Id },
                    ApiResponse<PedidoResponseDto>.SuccessResponse(
                        pedido,
                        "Pedido creado exitosamente"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear pedido para usuario {UsuarioId}", request.UsuarioId);
                return StatusCode(500, ApiResponse<PedidoResponseDto>.ErrorResponse(
                    "Error al crear pedido",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener pedido por ID
        /// Usuarios solo pueden ver sus propios pedidos
        /// Admin/Gerente pueden ver todos
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var rol = GetUsuarioRol();

                var pedido = await _pedidoService.GetByIdAsync(id);

                if (pedido == null)
                {
                    return NotFound(ApiResponse<PedidoResponseDto>.ErrorResponse(
                        $"Pedido con ID {id} no encontrado"
                    ));
                }

                // Validar que el usuario solo pueda ver sus propios pedidos
                // A menos que sea Admin o Gerente
                if (pedido.UsuarioId != usuarioId && rol != "Admin" && rol != "Gerente")
                {
                    return Forbid();
                }

                return Ok(ApiResponse<PedidoResponseDto>.SuccessResponse(pedido));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedido con ID {Id}", id);
                return StatusCode(500, ApiResponse<PedidoResponseDto>.ErrorResponse(
                    "Error al obtener pedido",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener pedidos del usuario autenticado
        /// </summary>
        [Authorize]
        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> GetMisPedidos()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                if (usuarioId == 0)
                {
                    return Unauthorized(ApiResponse<List<PedidoResponseDto>>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var pedidos = await _pedidoService.GetByUsuarioIdAsync(usuarioId);

                return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(
                    pedidos,
                    $"Se encontraron {pedidos.Count} pedidos"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos del usuario {UsuarioId}", GetUsuarioId());
                return StatusCode(500, ApiResponse<List<PedidoResponseDto>>.ErrorResponse(
                    "Error al obtener pedidos",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener pedidos por estado
        /// Solo Admin y Gerente
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpGet("por-estado/{estado}")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            try
            {
                var pedidos = await _pedidoService.GetByEstadoAsync(estado);

                return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(
                    pedidos,
                    $"Se encontraron {pedidos.Count} pedidos en estado '{estado}'"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos por estado {Estado}", estado);
                return StatusCode(500, ApiResponse<List<PedidoResponseDto>>.ErrorResponse(
                    "Error al obtener pedidos",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener pedidos pendientes de aprobación
        /// Solo Admin y Gerente
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            try
            {
                var pedidos = await _pedidoService.GetPedidosPendientesAsync();

                return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(
                    pedidos,
                    $"Hay {pedidos.Count} pedidos pendientes de aprobación"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pedidos pendientes");
                return StatusCode(500, ApiResponse<List<PedidoResponseDto>>.ErrorResponse(
                    "Error al obtener pedidos pendientes",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Actualizar estado de un pedido
        /// Solo Admin, Gerente y Domiciliario
        /// </summary>
        [Authorize(Roles = "Admin,Gerente,Domiciliario")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdatePedidoEstadoRequestDto request)
        {
            try
            {
                var pedido = await _pedidoService.UpdateEstadoAsync(id, request);

                if (pedido == null)
                {
                    return NotFound(ApiResponse<PedidoResponseDto>.ErrorResponse(
                        $"Pedido con ID {id} no encontrado"
                    ));
                }

                return Ok(ApiResponse<PedidoResponseDto>.SuccessResponse(
                    pedido,
                    $"Estado del pedido actualizado a '{request.Estado}'"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del pedido {Id}", id);
                return StatusCode(500, ApiResponse<PedidoResponseDto>.ErrorResponse(
                    "Error al actualizar estado del pedido",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Aprobar un pedido personalizado
        /// Solo Admin y Gerente
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpPost("{id}/aprobar")]
        public async Task<IActionResult> Aprobar(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var aprobado = await _pedidoService.AprobarPedidoAsync(id, usuarioId);

                if (!aprobado)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse(
                        $"Pedido con ID {id} no encontrado"
                    ));
                }

                return Ok(ApiResponse<bool>.SuccessResponse(
                    true,
                    "Pedido aprobado exitosamente"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar pedido {Id}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "Error al aprobar pedido",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}