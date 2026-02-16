using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;

namespace PastisserieAPI.Services.Services.Interfaces
{
    public interface IProductoService
    {
        Task<PagedResult<ProductoResponseDto>> GetAllAsync(PaginationDto pagination);
        Task<ProductoResponseDto?> GetByIdAsync(int id);
        Task<List<ProductoResponseDto>> GetByCategoriaIdAsync(int categoriaId);
        Task<List<ProductoResponseDto>> GetActivosAsync();
        Task<ProductoResponseDto> CreateAsync(CreateProductoRequestDto request);
        Task<ProductoResponseDto?> UpdateAsync(int id, UpdateProductoRequestDto request);
        Task<bool> DeleteAsync(int id);
        Task<List<ProductoResponseDto>> GetProductosBajoStockAsync();

        // ========== NUEVOS PARA F2 ==========
        Task<List<ProductoResponseDto>> SearchByNameAsync(string nombre);
        Task<List<ProductoResponseDto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax);
        Task<List<ProductoResponseDto>> GetDisponiblesAsync();
        Task<List<ProductoResponseDto>> SearchAsync(ProductoSearchDto filtros);
    }
}