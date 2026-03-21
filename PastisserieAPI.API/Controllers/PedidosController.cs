using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;
using AutoMapper;
using PastisserieAPI.Core.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly PastisserieAPI.Services.Services.Interfaces.IInvoiceService _invoiceService;

        public PedidosController(
            IPedidoService pedidoService, 
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            PastisserieAPI.Services.Services.Interfaces.IInvoiceService invoiceService)
        {
            _pedidoService = pedidoService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _invoiceService = invoiceService;
        }

        // 👇👇👇 ESTA ES LA PARTE QUE TE FALTABA 👇👇👇
        [HttpGet("todos")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTodos()
        {
            var pedidos = await _pedidoService.GetAllAsync();
            return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(pedidos));
        }
        // 👆👆👆 FIN DE LA PARTE QUE FALTABA 👆👆👆

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePedidoRequestDto request)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado en token"));

            var pedido = await _pedidoService.CreateAsync(userId, request);
            return Ok(ApiResponse<PedidoResponseDto>.SuccessResponse(pedido, "Pedido creado exitosamente"));
        }

        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> GetMisPedidos()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado en token"));

            var pedidos = await _pedidoService.GetByUsuarioIdAsync(userId);
            return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(pedidos));
        }

        [HttpGet]
        // Este trae solo los pendientes (ruta por defecto)
        public async Task<IActionResult> GetAll()
        {
            var pedidos = await _pedidoService.GetPedidosPendientesAsync();
            return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(pedidos));
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePedidoEstadoRequestDto request)
        {
            var result = await _pedidoService.UpdateEstadoAsync(id, request);
            if (result == null) return NotFound(ApiResponse<string>.ErrorResponse("Pedido no encontrado"));
            return Ok(ApiResponse<PedidoResponseDto>.SuccessResponse(result, "Estado actualizado"));
        }

        [HttpPatch("{id}/asignar-repartidor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AsignarRepartidor(int id, [FromBody] int repartidorId)
        {
            var result = await _pedidoService.AsignarRepartidorAsync(id, repartidorId);
            if (result == null) return NotFound(ApiResponse<string>.ErrorResponse("Pedido no encontrado"));
            return Ok(ApiResponse<PedidoResponseDto>.SuccessResponse(result, "Repartidor asignado"));
        }

        [HttpGet("repartidor")]
        [Authorize(Roles = "Repartidor")]
        public async Task<IActionResult> GetMisEntregas()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("Usuario no identificado"));

            var pedidos = await _pedidoService.GetByRepartidorIdAsync(userId);
            return Ok(ApiResponse<List<PedidoResponseDto>>.SuccessResponse(pedidos));
        }

        [HttpGet("{id}/factura")]
        [Authorize]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(id) ?? await _unitOfWork.Pedidos.GetByIdAsync(id);
            if (pedido == null) return NotFound(ApiResponse.ErrorResponse("Pedido no encontrado"));
            
            var user = await _unitOfWork.Users.GetByIdAsync(pedido.UsuarioId);
            if (user == null) return NotFound(ApiResponse.ErrorResponse("Usuario no encontrado"));

            var currentUserIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdStr != pedido.UsuarioId.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var pdfBytes = _invoiceService.GenerateInvoicePdf(pedido, user);
            return File(pdfBytes, "application/pdf", $"Factura_Pedido_{id}.pdf");
        }
    }
}