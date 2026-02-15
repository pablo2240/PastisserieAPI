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

        public async Task<List<ProductoResponseDto>> GetByCategoriaIdAsync(int categoriaId)
        {
            var productos = await _unitOfWork.Productos.GetByCategoriaIdAsync(categoriaId);
            return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
        }

        public async Task<List<ProductoResponseDto>> GetActivosAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosActivosAsync();
            return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
        }

        public async Task<ProductoResponseDto> CreateAsync(CreateProductoRequestDto request)
        {
            var producto = _mapper.Map<Producto>(request);
            producto.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Productos.AddAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            // Obtener producto con categoría
            var productoConCategoria = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(producto.Id);
            return _mapper.Map<ProductoResponseDto>(productoConCategoria!);
        }

        public async Task<ProductoResponseDto?> UpdateAsync(int id, UpdateProductoRequestDto request)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);

            if (producto == null)
                return null;

            // Mapear solo propiedades no nulas
            if (!string.IsNullOrEmpty(request.Nombre))
                producto.Nombre = request.Nombre;

            if (request.Descripcion != null)
                producto.Descripcion = request.Descripcion;

            if (request.Precio.HasValue)
                producto.Precio = request.Precio.Value;

            if (request.Stock.HasValue)
                producto.Stock = request.Stock.Value;

            if (request.StockMinimo.HasValue)
                producto.StockMinimo = request.StockMinimo.Value;

            if (request.CategoriaProductoId.HasValue)
                producto.CategoriaProductoId = request.CategoriaProductoId.Value;

            if (request.ImagenUrl != null)
                producto.ImagenUrl = request.ImagenUrl;

            if (request.Activo.HasValue)
                producto.Activo = request.Activo.Value;

            producto.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Productos.UpdateAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            // Obtener producto actualizado con categoría
            var productoActualizado = await _unitOfWork.Productos.GetByIdWithCategoriaAsync(producto.Id);
            return _mapper.Map<ProductoResponseDto>(productoActualizado);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var producto = await _unitOfWork.Productos.GetByIdAsync(id);

            if (producto == null)
                return false;

            // Soft delete
            producto.Activo = false;
            producto.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Productos.UpdateAsync(producto);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductoResponseDto>> GetProductosBajoStockAsync()
        {
            var productos = await _unitOfWork.Productos.GetProductosBajoStockAsync();
            return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
        }
    }
}