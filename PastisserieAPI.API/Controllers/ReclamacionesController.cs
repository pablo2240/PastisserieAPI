using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.Services.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReclamacionesController : ControllerBase
    {
        private readonly IReclamacionService _reclamacionService;

        public ReclamacionesController(IReclamacionService reclamacionService)
        {
            _reclamacionService = reclamacionService;
        }

        /// <summary>Crea una nueva reclamación (máximo 3 días después de la entrega).</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReclamacionRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            try
            {
                var result = await _reclamacionService.CreateAsync(userId, request.PedidoId, request.Motivo);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Reclamación creada exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        /// <summary>Obtiene reclamaciones del usuario autenticado.</summary>
        [HttpGet("mis-reclamaciones")]
        public async Task<IActionResult> GetMisReclamaciones()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            var result = await _reclamacionService.GetByUsuarioIdAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>Admin: Obtiene todas las reclamaciones.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _reclamacionService.GetAllAsync();
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>Admin: Actualiza el estado de una reclamación.</summary>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoReclamacionDto request)
        {
            var result = await _reclamacionService.UpdateEstadoAsync(id, request.Estado);
            if (result == null)
                return NotFound(ApiResponse<string>.ErrorResponse("Reclamación no encontrada"));
            return Ok(ApiResponse<object>.SuccessResponse(result, "Estado de reclamación actualizado"));
        }
    }
}
