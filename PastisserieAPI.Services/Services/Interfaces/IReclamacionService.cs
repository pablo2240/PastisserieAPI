using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IReclamacionService
    {
        Task<ReclamacionResponseDto> CreateAsync(int usuarioId, int pedidoId, string motivo);
        Task<List<ReclamacionResponseDto>> GetByUsuarioIdAsync(int usuarioId);
        Task<List<ReclamacionResponseDto>> GetAllAsync();
        Task<ReclamacionResponseDto?> UpdateEstadoAsync(int id, string estado);
    }
}
