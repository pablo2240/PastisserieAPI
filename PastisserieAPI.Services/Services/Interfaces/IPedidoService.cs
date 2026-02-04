using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponseDto> CreateAsync(CreatePedidoRequestDto request);
        Task<PedidoResponseDto?> GetByIdAsync(int id);
        Task<List<PedidoResponseDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<List<PedidoResponseDto>> GetByEstadoAsync(string estado);
        Task<PedidoResponseDto?> UpdateEstadoAsync(int id, UpdatePedidoEstadoRequestDto request);
        Task<bool> AprobarPedidoAsync(int id, int aprobadoPor);
        Task<List<PedidoResponseDto>> GetPedidosPendientesAsync();
    }
}