using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IProductoService
    {
        Task<PagedResult<ProductoResponseDto>> GetAllAsync(PaginationDto pagination);
        Task<ProductoResponseDto?> GetByIdAsync(int id);
        Task<List<ProductoResponseDto>> GetByCategoriaAsync(string categoria);
        Task<List<ProductoResponseDto>> GetActivosAsync();
        Task<ProductoResponseDto> CreateAsync(CreateProductoRequestDto request);
        Task<ProductoResponseDto?> UpdateAsync(int id, UpdateProductoRequestDto request);
        Task<bool> DeleteAsync(int id);
        Task<List<ProductoResponseDto>> GetProductosBajoStockAsync();
    }
}