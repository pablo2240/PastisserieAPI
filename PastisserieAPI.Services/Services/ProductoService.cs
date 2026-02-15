using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagedResult<ProductoResponseDto>> GetAllAsync(PaginationDto pagination)
        {
            var productos = await _unitOfWork.Productos.GetAllAsync();
            var totalCount = productos.Count();

            var pagedProductos = productos
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToList();

            var productosDto = _mapper.Map<List<ProductoResponseDto>>(pagedProductos);

            return new PagedResult<ProductoResponseDto>
            {
                Items = productosDto,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public async Task<ProductoResponseDto?> GetByIdAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdWithReviewsAsync(id);
            return producto == null ? null : _mapper.Map<ProductoResponseDto>(producto);
        }

        public async Task<List<ProductoResponseDto>> GetByCategoriaAsync(string categoria)
        {
            var productos = await _unitOfWork.Productos.GetByCategoriaAsync(categoria);
            return _mapper.Map<List<ProductoResponseDto>>(productos);
        }

        public async Task<List<ProductoResponseDto>> GetActivosAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosActivosAsync();
            return _mapper.Map<List<ProductoResponseDto>>(productos);
        }

        public async Task<ProductoResponseDto> CreateAsync(CreateProductoRequestDto request)
        {
            var producto = _mapper.Map<Producto>(request);
            await _unitOfWork.Productos.AddAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoResponseDto>(producto);
        }

        public async Task<ProductoResponseDto?> UpdateAsync(int id, UpdateProductoRequestDto request)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);

            if (producto == null)
                return null;

            _mapper.Map(request, producto);
            producto.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Productos.UpdateAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductoResponseDto>(producto);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);

            if (producto == null)
                return false;

            await _unitOfWork.Productos.DeleteAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductoResponseDto>> GetProductosBajoStockAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosBajoStockAsync();
            return _mapper.Map<List<ProductoResponseDto>>(productos);
        }
    }
}