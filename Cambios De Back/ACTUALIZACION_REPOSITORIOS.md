# 🔧 ACTUALIZACIÓN DE REPOSITORIOS
## Compatibilidad con Cambios de Categoría y Personalización

---

## 📋 REPOSITORIOS A ACTUALIZAR

1. **ProductoRepository.cs** - Cambiar Categoria string a CategoriaProductoId
2. **PedidoRepository.cs** - Eliminar PersonalizadoConfig e Ingredientes
3. **IProductoRepository.cs** - Actualizar firma del método GetByCategoriaAsync

---

# ARCHIVO 1: IProductoRepository.cs

**Ubicación**: `PastisserieAPI.Core/Interfaces/Repositories/IProductoRepository.cs`

**REEMPLAZAR TODO EL ARCHIVO:**

```csharp
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        /// <summary>
        /// Obtener productos por ID de categoría
        /// </summary>
        Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);

        /// <summary>
        /// Obtener productos activos
        /// </summary>
        Task<IEnumerable<Producto>> GetProductosActivosAsync();

        /// <summary>
        /// Obtener productos con bajo stock
        /// </summary>
        Task<IEnumerable<Producto>> GetProductosBajoStockAsync();

        /// <summary>
        /// Obtener producto con sus reviews
        /// </summary>
        Task<Producto?> GetByIdWithReviewsAsync(int id);

        /// <summary>
        /// Obtener producto con categoría incluida
        /// </summary>
        Task<Producto?> GetByIdWithCategoriaAsync(int id);

        /// <summary>
        /// Obtener todos los productos con categoría incluida
        /// </summary>
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
    }
}
```

---

# ARCHIVO 2: ProductoRepository.cs

**Ubicación**: `PastisserieAPI.Infrastructure/Repositorie/ProductoRepository.cs`

**REEMPLAZAR TODO EL ARCHIVO:**

```csharp
using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Data;

namespace PastisserieAPI.Infrastructure.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtener productos por ID de categoría
        /// </summary>
        public async Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.CategoriaProductoId == categoriaId && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos activos
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosActivosAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener productos con bajo stock
        /// </summary>
        public async Task<IEnumerable<Producto>> GetProductosBajoStockAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Where(p => p.StockMinimo.HasValue && p.Stock <= p.StockMinimo.Value)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtener producto con sus reviews
        /// </summary>
        public async Task<Producto?> GetByIdWithReviewsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto) // Incluir categoría
                .Include(p => p.Reviews.Where(r => r.Aprobada)) // Solo reviews aprobadas
                    .ThenInclude(r => r.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtener producto con categoría incluida
        /// </summary>
        public async Task<Producto?> GetByIdWithCategoriaAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtener todos los productos con categoría incluida
        /// </summary>
        public async Task<IEnumerable<Producto>> GetAllWithCategoriaAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Override del GetByIdAsync para incluir categoría por defecto
        /// </summary>
        public override async Task<Producto?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Override del GetAllAsync para incluir categoría por defecto
        /// </summary>
        public override async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.CategoriaProducto)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
    }
}
```

---

# ARCHIVO 3: PedidoRepository.cs

**Ubicación**: `PastisserieAPI.Infrastructure/Repositorie/PedidoRepository.cs`

**REEMPLAZAR TODO EL ARCHIVO:**

```csharp
using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Data;

namespace PastisserieAPI.Infrastructure.Repositories
{
    public class PedidoRepository : Repository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pedido>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.CategoriaProducto) // Incluir categoría del producto
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetByEstadoAsync(string estado)
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == estado)
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(prod => prod.CategoriaProducto) // Incluir categoría
                .Include(p => p.Usuario)
                .Include(p => p.DireccionEnvio)
                .Include(p => p.Envio)
                    .ThenInclude(e => e!.Repartidor)
                .Include(p => p.Factura)
                .Include(p => p.Historial)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> GetPedidosPendientesAsync()
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == "Pendiente" || p.Estado == "Confirmado")
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetPedidosEnPreparacionAsync()
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == "EnPreparacion")
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }
    }
}
```

---

# ARCHIVO 4: IProductoService.cs (Actualizar firma)

**Ubicación**: `PastisserieAPI.Services/Services/Interfaces/IProductoService.cs`

**BUSCAR:**
```csharp
Task<List<ProductoResponseDto>> GetByCategoriaAsync(string categoria);
Task<List<ProductoResponseDto>> GetPersonalizablesAsync();
```

**REEMPLAZAR CON:**
```csharp
Task<List<ProductoResponseDto>> GetByCategoriaIdAsync(int categoriaId);
```

**RESULTADO FINAL:**

```csharp
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
    }
}
```

---

# ARCHIVO 5: ProductoService.cs

**Ubicación**: `PastisserieAPI.Services/Services/ProductoService.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public async Task<List<ProductoResponseDto>> GetByCategoriaAsync(string categoria)
{
    var productos = await _unitOfWork.Productos.GetByCategoriaAsync(categoria);
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}

public async Task<List<ProductoResponseDto>> GetPersonalizablesAsync()
{
    var productos = await _unitOfWork.Productos.GetPersonalizablesAsync();
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}
```

**AGREGAR:**
```csharp
public async Task<List<ProductoResponseDto>> GetByCategoriaIdAsync(int categoriaId)
{
    var productos = await _unitOfWork.Productos.GetByCategoriaIdAsync(categoriaId);
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}
```

**RESULTADO FINAL COMPLETO:**

```csharp
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
```

---

# ARCHIVO 6: ProductoResponseDto.cs (VERIFICAR)

**Ubicación**: `PastisserieAPI.Services/DTOs/Response/ProductoResponseDto.cs`

**DEBE TENER:**

```csharp
namespace PastisserieAPI.Services.DTOs.Response
{
    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int? StockMinimo { get; set; }
        
        // Categoría
        public int CategoriaProductoId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty; // Nombre de la categoría
        
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
    }
}
```

---

# ARCHIVO 7: CreateProductoRequestDto.cs (ACTUALIZAR)

**Ubicación**: `PastisserieAPI.Services/DTOs/Request/CreateProductoRequestDto.cs`

**DEBE QUEDAR ASÍ:**

```csharp
namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreateProductoRequestDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int? StockMinimo { get; set; }
        public int CategoriaProductoId { get; set; } // FK a CategoriaProducto
        public string? ImagenUrl { get; set; }
    }
}
```

---

# ARCHIVO 8: UpdateProductoRequestDto.cs (ACTUALIZAR)

**Ubicación**: `PastisserieAPI.Services/DTOs/Request/UpdateProductoRequestDto.cs`

**DEBE QUEDAR ASÍ:**

```csharp
namespace PastisserieAPI.Services.DTOs.Request
{
    public class UpdateProductoRequestDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public int? Stock { get; set; }
        public int? StockMinimo { get; set; }
        public int? CategoriaProductoId { get; set; } // FK a CategoriaProducto
        public string? ImagenUrl { get; set; }
        public bool? Activo { get; set; }
    }
}
```

---

# ARCHIVO 9: MappingProfile.cs (ACTUALIZAR MAPPING DE PRODUCTO)

**Ubicación**: `PastisserieAPI.Services/Mappings/MappingProfile.cs`

**BUSCAR la sección de Producto mappings y ACTUALIZAR:**

```csharp
// ============ PRODUCTO MAPPINGS ============
CreateMap<Producto, ProductoResponseDto>()
    .ForMember(dest => dest.CategoriaNombre, 
               opt => opt.MapFrom(src => src.CategoriaProducto.Nombre));

CreateMap<CreateProductoRequestDto, Producto>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true))
    .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
    .ForMember(dest => dest.FechaActualizacion, opt => opt.Ignore())
    .ForMember(dest => dest.CategoriaProducto, opt => opt.Ignore())
    .ForMember(dest => dest.Reviews, opt => opt.Ignore())
    .ForMember(dest => dest.PedidoItems, opt => opt.Ignore())
    .ForMember(dest => dest.CarritoItems, opt => opt.Ignore());
```

---

# ARCHIVO 10: ProductosController.cs (ACTUALIZAR ENDPOINT)

**Ubicación**: `PastisserieAPI.API/Controllers/ProductosController.cs`

**BUSCAR:**
```csharp
[HttpGet("categoria/{categoria}")]
public async Task<IActionResult> GetByCategoria(string categoria)
```

**REEMPLAZAR CON:**
```csharp
[HttpGet("categoria/{categoriaId}")]
public async Task<IActionResult> GetByCategoria(int categoriaId)
{
    try
    {
        var productos = await _productoService.GetByCategoriaIdAsync(categoriaId);

        return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
            productos,
            $"Se encontraron {productos.Count} productos"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener productos por categoría {CategoriaId}", categoriaId);
        return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
            "Error al obtener productos",
            new List<string> { ex.Message }
        ));
    }
}
```

**ELIMINAR el endpoint:**
```csharp
[HttpGet("personalizables")]
```

---

# ✅ RESUMEN DE CAMBIOS

## Archivos Actualizados (10)

| Archivo | Cambio Principal |
|---------|------------------|
| IProductoRepository.cs | GetByCategoriaAsync(string) → GetByCategoriaIdAsync(int) |
| ProductoRepository.cs | Usar CategoriaProductoId, Include CategoriaProducto |
| PedidoRepository.cs | Eliminar Include PersonalizadoConfig |
| IProductoService.cs | Actualizar firmas de métodos |
| ProductoService.cs | Implementar GetByCategoriaIdAsync |
| ProductoResponseDto.cs | Agregar CategoriaProductoId y CategoriaNombre |
| CreateProductoRequestDto.cs | CategoriaProductoId en lugar de Categoria string |
| UpdateProductoRequestDto.cs | CategoriaProductoId en lugar de Categoria string |
| MappingProfile.cs | Mapear CategoriaNombre desde navegación |
| ProductosController.cs | Endpoint por categoriaId |

---

## Orden de Aplicación

1. ✅ Actualizar Interfaces (IProductoRepository, IProductoService)
2. ✅ Actualizar Repositorios (ProductoRepository, PedidoRepository)
3. ✅ Actualizar DTOs (Request y Response)
4. ✅ Actualizar MappingProfile
5. ✅ Actualizar ProductoService
6. ✅ Actualizar ProductosController
7. ✅ Compilar y verificar

---

## Compilación

```bash
dotnet clean
dotnet restore
dotnet build
```

**Debe compilar sin errores**

---

**Fecha**: Febrero 15, 2026  
**Cambios**: Repositorios actualizados para Categoría obligatoria  
**Estado**: Listo para aplicar
