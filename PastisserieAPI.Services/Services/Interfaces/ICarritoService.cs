using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface ICarritoService
    {
        Task<CarritoResponseDto?> GetByUsuarioIdAsync(int usuarioId);
        Task<CarritoResponseDto> AddItemAsync(int usuarioId, AddToCarritoRequestDto request);
        Task<CarritoResponseDto?> UpdateItemAsync(int usuarioId, int itemId, UpdateCarritoItemRequestDto request);
        Task<bool> RemoveItemAsync(int usuarioId, int itemId);
        Task<bool> ClearCarritoAsync(int usuarioId);
    }
}