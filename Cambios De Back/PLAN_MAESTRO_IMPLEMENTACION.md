# 🎯 PLAN MAESTRO DE IMPLEMENTACIÓN
## Funcionalidades Definitivas del Backend

---

## 📋 ÍNDICE
1. [Estructura Final Confirmada](#estructura-final)
2. [FASE 1: Cambios en Entidades](#fase-1-entidades)
3. [FASE 2: Crear Migración y BD](#fase-2-migracion)
4. [FASE 3: Implementar Funcionalidades AHORA](#fase-3-ahora)
5. [FASE 4: Para el FUTURO](#fase-4-futuro)
6. [Orden de Ejecución](#orden-ejecucion)

---

## ✅ ESTRUCTURA FINAL CONFIRMADA

### Entidades (15 FINALES):
```
✅ Users, Roles, UserRoles
✅ CategoriaProducto, Producto, Review
✅ Pedido, PedidoItem, PedidoHistorial, Factura
✅ CarritoCompra, CarritoItem
✅ DireccionEnvio, Envio
✅ Notificacion
```

### LO QUE NO VAMOS A TENER:
```
❌ IVA (eliminado de Pedido y Factura)
❌ SKU en Producto
❌ Ingredientes (entidad eliminada)
❌ PersonalizadoConfig (eliminado)
❌ Cupones (no implementar)
❌ TipoMetodoPago, MetodoPagoUsuario (eliminados)
```

### LO QUE SÍ VAMOS A AGREGAR:
```
✅ Producto: Campo para calcular disponibilidad (lógica en DTO)
✅ CarritoItem: Campo ReservaHasta (DateTime?) para RN2
✅ User: Campos para bloqueo de cuenta (FUTURO)
✅ Pedido: Ya tiene EstadoPago, TransaccionId, MetodoPago para Wompi
```

---

## 🔧 FASE 1: CAMBIOS EN ENTIDADES

### 1.1 - CarritoItem.cs (AGREGAR RESERVA TEMPORAL)

**Ubicación**: `PastisserieAPI.Core/Entities/CarritoItem.cs`

**AGREGAR esta propiedad:**

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class CarritoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CarritoId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

        // ========== RESERVA TEMPORAL (RN2) ==========
        public DateTime? ReservaHasta { get; set; } // Null = no reservado, DateTime = reservado hasta

        // Relaciones
        [ForeignKey("CarritoId")]
        public virtual CarritoCompra Carrito { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual Producto Producto { get; set; } = null!;
    }
}
```

**Explicación:**
- `ReservaHasta`: Cuando se agrega un item al carrito, se establece a `DateTime.UtcNow.AddMinutes(10)`
- Si es `null`, el producto no está reservado
- Si expira (ReservaHasta < DateTime.UtcNow), se libera automáticamente

---

### 1.2 - User.cs (AGREGAR CAMPOS PARA FUTURO)

**Ubicación**: `PastisserieAPI.Core/Entities/User.cs`

**AGREGAR estas propiedades al final:**

```csharp
// ========== SEGURIDAD (PARA FUTURO) ==========
public int IntentosLoginFallidos { get; set; } = 0;
public DateTime? FechaUltimoIntentoFallido { get; set; }
public bool CuentaBloqueada { get; set; } = false;
public DateTime? FechaDesbloqueo { get; set; }
```

**NOTA**: Los dejamos en la entidad pero NO implementamos la lógica hasta FUTURO.

---

### 1.3 - Producto.cs (YA ESTÁ BIEN)

**NO MODIFICAR** - Ya tiene todo lo necesario:
```csharp
✅ Nombre, Descripción, Precio
✅ Stock (para calcular disponibilidad)
✅ CategoriaProductoId (obligatorio)
✅ ImagenUrl
✅ Activo
```

---

### 1.4 - Pedido.cs (YA ESTÁ BIEN)

**NO MODIFICAR** - Ya tiene campos Wompi:
```csharp
✅ EstadoPago = "Pendiente" | "Aprobado" | "Rechazado"
✅ TransaccionId (ID de Wompi)
✅ MetodoPago = "Efectivo" | "Tarjeta" | "PSE" | "Nequi"
✅ SIN IVA (correcto)
```

---

## 🗄️ FASE 2: CREAR MIGRACIÓN Y BASE DE DATOS

### 2.1 - Eliminar Base de Datos y Migraciones Antiguas

```bash
# 1. Eliminar base de datos
dotnet ef database drop --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API --force

# 2. Eliminar carpeta de migraciones
rm -rf PastisserieAPI.Infrastructure/Migrations
# O en Windows: eliminar manualmente la carpeta Migrations
```

---

### 2.2 - Actualizar Configuración de CarritoItem

**Ubicación**: `PastisserieAPI.Infrastructure/Data/Configurations/CarritoItemConfiguration.cs`

**AGREGAR después de las relaciones:**

```csharp
// Configuración para ReservaHasta
builder.Property(ci => ci.ReservaHasta)
    .IsRequired(false); // Nullable

// Índice para búsquedas de reservas expiradas
builder.HasIndex(ci => ci.ReservaHasta)
    .HasDatabaseName("IX_CarritoItems_ReservaHasta");
```

---

### 2.3 - Crear Nueva Migración

```bash
dotnet ef migrations add ImplementacionFuncionalidadesFinales --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

---

### 2.4 - Aplicar Migración

```bash
dotnet ef database update --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

---

### 2.5 - Crear Usuario Administrador

Seguir instrucciones en: `PastisserieAPI.API/Database/Scripts/README.md`

---

## 🚀 FASE 3: IMPLEMENTAR FUNCIONALIDADES AHORA

### F1. VISUALIZACIÓN DE CATÁLOGO - Estado Disponibilidad

#### Actualizar ProductoResponseDto

**Ubicación**: `PastisserieAPI.Services/DTOs/Response/ProductoResponseDto.cs`

**AGREGAR:**

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
        public string CategoriaNombre { get; set; } = string.Empty;
        
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        
        // ========== DISPONIBILIDAD (F1) ==========
        public string EstadoDisponibilidad { get; set; } = string.Empty; // "Disponible" | "Sin stock"
        public bool EstaDisponible => Stock > 0;
        
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        // Reviews (si se incluyen)
        public double? CalificacionPromedio { get; set; }
        public int? TotalReviews { get; set; }
    }
}
```

---

#### Actualizar MappingProfile

**Ubicación**: `PastisserieAPI.Services/Mappings/MappingProfile.cs`

**BUSCAR el mapping de Producto y ACTUALIZAR:**

```csharp
// ============ PRODUCTO MAPPINGS ============
CreateMap<Producto, ProductoResponseDto>()
    .ForMember(dest => dest.CategoriaNombre, 
               opt => opt.MapFrom(src => src.CategoriaProducto.Nombre))
    .ForMember(dest => dest.EstadoDisponibilidad,
               opt => opt.MapFrom(src => src.Stock > 0 ? "Disponible" : "Sin stock"))
    .ForMember(dest => dest.CalificacionPromedio,
               opt => opt.MapFrom(src => src.Reviews.Any(r => r.Aprobada) 
                   ? src.Reviews.Where(r => r.Aprobada).Average(r => r.Calificacion)
                   : (double?)null))
    .ForMember(dest => dest.TotalReviews,
               opt => opt.MapFrom(src => src.Reviews.Count(r => r.Aprobada)));
```

---

### F2. BÚSQUEDA Y FILTRADO

#### Actualizar IProductoRepository

**Ubicación**: `PastisserieAPI.Core/Interfaces/Repositories/IProductoRepository.cs`

**AGREGAR estos métodos:**

```csharp
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Core.Interfaces.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetByCategoriaIdAsync(int categoriaId);
        Task<IEnumerable<Producto>> GetProductosActivosAsync();
        Task<IEnumerable<Producto>> GetProductosBajoStockAsync();
        Task<Producto?> GetByIdWithReviewsAsync(int id);
        Task<Producto?> GetByIdWithCategoriaAsync(int id);
        Task<IEnumerable<Producto>> GetAllWithCategoriaAsync();
        
        // ========== NUEVOS PARA F2 ==========
        
        /// <summary>
        /// Buscar productos por nombre (insensible a mayúsculas)
        /// </summary>
        Task<IEnumerable<Producto>> SearchByNameAsync(string nombre);
        
        /// <summary>
        /// Filtrar productos por rango de precio
        /// </summary>
        Task<IEnumerable<Producto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax);
        
        /// <summary>
        /// Obtener productos disponibles (stock > 0)
        /// </summary>
        Task<IEnumerable<Producto>> GetDisponiblesAsync();
        
        /// <summary>
        /// Búsqueda avanzada con múltiples filtros
        /// </summary>
        Task<IEnumerable<Producto>> SearchAsync(
            string? nombre = null,
            int? categoriaId = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            bool? soloDisponibles = null
        );
    }
}
```

---

#### Actualizar ProductoRepository

**Ubicación**: `PastisserieAPI.Infrastructure/Repositorie/ProductoRepository.cs`

**AGREGAR estos métodos al final:**

```csharp
/// <summary>
/// Buscar productos por nombre
/// </summary>
public async Task<IEnumerable<Producto>> SearchByNameAsync(string nombre)
{
    return await _dbSet
        .Include(p => p.CategoriaProducto)
        .Where(p => p.Activo && 
                    p.Nombre.ToLower().Contains(nombre.ToLower()))
        .OrderBy(p => p.Nombre)
        .ToListAsync();
}

/// <summary>
/// Filtrar por rango de precio
/// </summary>
public async Task<IEnumerable<Producto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax)
{
    return await _dbSet
        .Include(p => p.CategoriaProducto)
        .Where(p => p.Activo && 
                    p.Precio >= precioMin && 
                    p.Precio <= precioMax)
        .OrderBy(p => p.Precio)
        .ToListAsync();
}

/// <summary>
/// Obtener productos disponibles (stock > 0)
/// </summary>
public async Task<IEnumerable<Producto>> GetDisponiblesAsync()
{
    return await _dbSet
        .Include(p => p.CategoriaProducto)
        .Where(p => p.Activo && p.Stock > 0)
        .OrderBy(p => p.Nombre)
        .ToListAsync();
}

/// <summary>
/// Búsqueda avanzada con múltiples filtros
/// </summary>
public async Task<IEnumerable<Producto>> SearchAsync(
    string? nombre = null,
    int? categoriaId = null,
    decimal? precioMin = null,
    decimal? precioMax = null,
    bool? soloDisponibles = null)
{
    var query = _dbSet
        .Include(p => p.CategoriaProducto)
        .Where(p => p.Activo)
        .AsQueryable();

    // Filtro por nombre
    if (!string.IsNullOrWhiteSpace(nombre))
    {
        query = query.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
    }

    // Filtro por categoría
    if (categoriaId.HasValue)
    {
        query = query.Where(p => p.CategoriaProductoId == categoriaId.Value);
    }

    // Filtro por rango de precio
    if (precioMin.HasValue)
    {
        query = query.Where(p => p.Precio >= precioMin.Value);
    }

    if (precioMax.HasValue)
    {
        query = query.Where(p => p.Precio <= precioMax.Value);
    }

    // Filtro por disponibilidad
    if (soloDisponibles == true)
    {
        query = query.Where(p => p.Stock > 0);
    }

    return await query.OrderBy(p => p.Nombre).ToListAsync();
}
```

---

#### Actualizar IProductoService

**Ubicación**: `PastisserieAPI.Services/Services/Interfaces/IProductoService.cs`

**AGREGAR:**

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
        
        // ========== NUEVOS PARA F2 ==========
        Task<List<ProductoResponseDto>> SearchByNameAsync(string nombre);
        Task<List<ProductoResponseDto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax);
        Task<List<ProductoResponseDto>> GetDisponiblesAsync();
        Task<List<ProductoResponseDto>> SearchAsync(ProductoSearchDto filtros);
    }
}
```

---

#### Crear ProductoSearchDto

**Ubicación**: `PastisserieAPI.Services/DTOs/Request/ProductoSearchDto.cs`

**CREAR NUEVO ARCHIVO:**

```csharp
namespace PastisserieAPI.Services.DTOs.Request
{
    public class ProductoSearchDto
    {
        public string? Nombre { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }
        public bool? SoloDisponibles { get; set; }
    }
}
```

---

#### Actualizar ProductoService

**Ubicación**: `PastisserieAPI.Services/Services/ProductoService.cs`

**AGREGAR estos métodos al final:**

```csharp
public async Task<List<ProductoResponseDto>> SearchByNameAsync(string nombre)
{
    var productos = await _unitOfWork.Productos.SearchByNameAsync(nombre);
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}

public async Task<List<ProductoResponseDto>> GetByPriceRangeAsync(decimal precioMin, decimal precioMax)
{
    var productos = await _unitOfWork.Productos.GetByPriceRangeAsync(precioMin, precioMax);
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}

public async Task<List<ProductoResponseDto>> GetDisponiblesAsync()
{
    var productos = await _unitOfWork.Productos.GetDisponiblesAsync();
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}

public async Task<List<ProductoResponseDto>> SearchAsync(ProductoSearchDto filtros)
{
    var productos = await _unitOfWork.Productos.SearchAsync(
        nombre: filtros.Nombre,
        categoriaId: filtros.CategoriaId,
        precioMin: filtros.PrecioMin,
        precioMax: filtros.PrecioMax,
        soloDisponibles: filtros.SoloDisponibles
    );
    
    return _mapper.Map<List<ProductoResponseDto>>(productos.ToList());
}
```

---

#### Agregar Endpoints en ProductosController

**Ubicación**: `PastisserieAPI.API/Controllers/ProductosController.cs`

**AGREGAR estos endpoints:**

```csharp
/// <summary>
/// Buscar productos por nombre
/// </summary>
[HttpGet("search")]
public async Task<IActionResult> SearchByName([FromQuery] string nombre)
{
    try
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
                "Debe proporcionar un término de búsqueda"
            ));
        }

        var productos = await _productoService.SearchByNameAsync(nombre);

        return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
            productos,
            $"Se encontraron {productos.Count} productos"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al buscar productos por nombre: {Nombre}", nombre);
        return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
            "Error al buscar productos",
            new List<string> { ex.Message }
        ));
    }
}

/// <summary>
/// Filtrar productos por rango de precio
/// </summary>
[HttpGet("precio-rango")]
public async Task<IActionResult> GetByPriceRange([FromQuery] decimal precioMin, [FromQuery] decimal precioMax)
{
    try
    {
        if (precioMin < 0 || precioMax < 0 || precioMin > precioMax)
        {
            return BadRequest(ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
                "Rango de precio inválido"
            ));
        }

        var productos = await _productoService.GetByPriceRangeAsync(precioMin, precioMax);

        return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
            productos,
            $"Se encontraron {productos.Count} productos en el rango ${precioMin} - ${precioMax}"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al filtrar por precio");
        return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
            "Error al filtrar productos",
            new List<string> { ex.Message }
        ));
    }
}

/// <summary>
/// Obtener productos disponibles (con stock)
/// </summary>
[HttpGet("disponibles")]
public async Task<IActionResult> GetDisponibles()
{
    try
    {
        var productos = await _productoService.GetDisponiblesAsync();

        return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
            productos,
            $"Se encontraron {productos.Count} productos disponibles"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener productos disponibles");
        return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
            "Error al obtener productos disponibles",
            new List<string> { ex.Message }
        ));
    }
}

/// <summary>
/// Búsqueda avanzada con múltiples filtros
/// </summary>
[HttpPost("busqueda-avanzada")]
public async Task<IActionResult> SearchAdvanced([FromBody] ProductoSearchDto filtros)
{
    try
    {
        var productos = await _productoService.SearchAsync(filtros);

        return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
            productos,
            $"Se encontraron {productos.Count} productos"
        ));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en búsqueda avanzada");
        return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
            "Error al buscar productos",
            new List<string> { ex.Message }
        ));
    }
}
```

---

### F3. CARRITO - Validaciones

#### Actualizar CarritoService

**Ubicación**: `PastisserieAPI.Services/Services/CarritoService.cs`

**ACTUALIZAR el método `AddItemAsync`:**

```csharp
public async Task<CarritoResponseDto> AddItemAsync(int usuarioId, AddToCarritoRequestDto request)
{
    // Obtener o crear carrito
    var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
    
    if (carrito == null)
    {
        carrito = new CarritoCompra
        {
            UsuarioId = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };
        await _unitOfWork.Carritos.AddAsync(carrito);
        await _unitOfWork.SaveChangesAsync();
    }

    // Obtener producto
    var producto = await _unitOfWork.Productos.GetByIdAsync(request.ProductoId);
    
    if (producto == null)
        throw new Exception("Producto no encontrado");

    if (!producto.Activo)
        throw new Exception("El producto no está disponible");

    // ========== RN1: VALIDAR STOCK ==========
    if (producto.Stock <= 0)
        throw new Exception("Producto sin stock disponible");

    if (request.Cantidad > producto.Stock)
        throw new Exception($"Solo hay {producto.Stock} unidades disponibles");

    // ========== RN3: LÍMITE 20 UNIDADES ==========
    if (request.Cantidad > 20)
        throw new Exception("No puedes agregar más de 20 unidades por producto");

    // ========== F3: AUMENTAR CANTIDAD SI YA EXISTE ==========
    var itemExistente = carrito.Items
        .FirstOrDefault(i => i.ProductoId == request.ProductoId);

    if (itemExistente != null)
    {
        int nuevaCantidad = itemExistente.Cantidad + request.Cantidad;
        
        // Validar stock para nueva cantidad
        if (nuevaCantidad > producto.Stock)
            throw new Exception($"Solo hay {producto.Stock} unidades disponibles");

        // Validar límite de 20
        if (nuevaCantidad > 20)
            throw new Exception("No puedes tener más de 20 unidades de este producto en tu carrito");

        itemExistente.Cantidad = nuevaCantidad;
        
        // ========== RN2: ACTUALIZAR RESERVA ==========
        itemExistente.ReservaHasta = DateTime.UtcNow.AddMinutes(10);
    }
    else
    {
        // Crear nuevo item
        var nuevoItem = new CarritoItem
        {
            CarritoId = carrito.Id,
            ProductoId = request.ProductoId,
            Cantidad = request.Cantidad,
            FechaAgregado = DateTime.UtcNow,
            
            // ========== RN2: ESTABLECER RESERVA ==========
            ReservaHasta = DateTime.UtcNow.AddMinutes(10)
        };

        carrito.Items.Add(nuevoItem);
    }

    carrito.FechaActualizacion = DateTime.UtcNow;
    
    await _unitOfWork.SaveChangesAsync();

    // Obtener carrito actualizado
    var carritoActualizado = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
    return _mapper.Map<CarritoResponseDto>(carritoActualizado);
}
```

---

#### Crear Job para Liberar Reservas Expiradas

**Ubicación**: `PastisserieAPI.Services/Services/ReservaStockService.cs`

**CREAR NUEVO ARCHIVO:**

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PastisserieAPI.Core.Interfaces;

namespace PastisserieAPI.Services.Services
{
    /// <summary>
    /// Servicio en background para liberar reservas de stock expiradas
    /// </summary>
    public class ReservaStockService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservaStockService> _logger;

        public ReservaStockService(
            IServiceProvider serviceProvider,
            ILogger<ReservaStockService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de liberación de reservas iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await LiberarReservasExpiradasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al liberar reservas");
                }

                // Ejecutar cada 2 minutos
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        private async Task LiberarReservasExpiradasAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Buscar items con reservas expiradas
                var itemsExpirados = await unitOfWork.Carritos
                    .FindAsync(ci => ci.ReservaHasta.HasValue && 
                                     ci.ReservaHasta.Value < DateTime.UtcNow);

                if (itemsExpirados.Any())
                {
                    _logger.LogInformation(
                        "Liberando {Count} reservas expiradas", 
                        itemsExpirados.Count()
                    );

                    // Eliminar items expirados
                    await unitOfWork.Carritos.DeleteRangeAsync(itemsExpirados);
                    await unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar reservas expiradas");
            }
        }
    }
}
```

---

#### Registrar el Background Service

**Ubicación**: `PastisserieAPI.API/Program.cs`

**AGREGAR después de los servicios:**

```csharp
// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// ========== BACKGROUND SERVICE PARA RESERVAS ==========
builder.Services.AddHostedService<ReservaStockService>();
```

---

### F5. CHECKOUT - Validación Completa

#### Actualizar CreatePedidoRequestValidator

**Ubicación**: `PastisserieAPI.Services/Validators/CreatePedidoRequestValidator.cs`

**MEJORAR las validaciones:**

```csharp
using FluentValidation;
using PastisserieAPI.Services.DTOs.Request;

namespace PastisserieAPI.Services.Validators
{
    public class CreatePedidoRequestValidator : AbstractValidator<CreatePedidoRequestDto>
    {
        public CreatePedidoRequestValidator()
        {
            RuleFor(x => x.UsuarioId)
                .GreaterThan(0)
                .WithMessage("El ID de usuario es inválido");

            RuleFor(x => x.MetodoPago)
                .NotEmpty()
                .WithMessage("Debe seleccionar un método de pago")
                .Must(metodo => new[] { "Efectivo", "Tarjeta", "PSE", "Nequi", "Daviplata" }
                    .Contains(metodo))
                .WithMessage("Método de pago no válido");

            // ========== VALIDACIÓN DIRECCIÓN (F5) ==========
            RuleFor(x => x.DireccionEnvioId)
                .NotNull()
                .WithMessage("Debe seleccionar una dirección de envío")
                .GreaterThan(0)
                .WithMessage("La dirección de envío es inválida");

            // ========== VALIDACIÓN ITEMS (F5) ==========
            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("El pedido debe tener al menos un producto")
                .Must(items => items.Count <= 50)
                .WithMessage("No puede agregar más de 50 productos al pedido");

            RuleForEach(x => x.Items).SetValidator(new PedidoItemRequestValidator());

            RuleFor(x => x.NotasCliente)
                .MaximumLength(1000)
                .WithMessage("Las notas no pueden exceder 1000 caracteres")
                .When(x => !string.IsNullOrEmpty(x.NotasCliente));
        }
    }

    public class PedidoItemRequestValidator : AbstractValidator<PedidoItemRequestDto>
    {
        public PedidoItemRequestValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0)
                .WithMessage("ID de producto inválido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor a 0")
                .LessThanOrEqualTo(20)
                .WithMessage("No puede agregar más de 20 unidades por producto");
        }
    }
}
```

---

## 📅 FASE 4: PARA EL FUTURO

### F4. Validación de Email

**Pendiente implementar:**
- Generar token único al registrar
- Enviar email con link de verificación
- Endpoint: `POST /api/Auth/verify-email?token=xxx`
- Actualizar `User.EmailVerificado = true`

---

### F4. Bloqueo de Cuenta (3 intentos)

**Pendiente implementar en AuthService.LoginAsync:**

```csharp
// Ya tenemos los campos en User:
// - IntentosLoginFallidos
// - CuentaBloqueada
// - FechaDesbloqueo

// Lógica a implementar:
if (user.CuentaBloqueada && user.FechaDesbloqueo > DateTime.UtcNow)
    throw new Exception("Cuenta bloqueada temporalmente");

if (!BCrypt.Verify(password, user.PasswordHash))
{
    user.IntentosLoginFallidos++;
    user.FechaUltimoIntentoFallido = DateTime.UtcNow;
    
    if (user.IntentosLoginFallidos >= 3)
    {
        user.CuentaBloqueada = true;
        user.FechaDesbloqueo = DateTime.UtcNow.AddMinutes(30);
    }
    
    await _unitOfWork.SaveChangesAsync();
    throw new Exception("Credenciales incorrectas");
}

// Reset intentos en login exitoso
user.IntentosLoginFallidos = 0;
user.CuentaBloqueada = false;
```

---

### F6. Integración Wompi Completa

**Pendiente:**
1. Endpoint: `POST /api/Pagos/procesar-wompi`
2. Webhook: `POST /api/Pagos/webhook-wompi`
3. Lógica de reintentos (máximo 3)
4. Actualizar `Pedido.EstadoPago` y `TransaccionId`

---

### F7. Envío de Recibo por Correo

**Pendiente:**
1. Integrar SendGrid o SMTP
2. Crear template HTML del recibo
3. Enviar en `PedidoService.CreateAsync` después de pago exitoso
4. Implementar cola de reintentos

---

### RN5. Cancelación Solo Efectivo

**Pendiente en PedidoService:**

```csharp
public async Task<bool> CancelarPedidoAsync(int pedidoId, int usuarioId)
{
    var pedido = await _unitOfWork.Pedidos.GetByIdAsync(pedidoId);
    
    if (pedido.UsuarioId != usuarioId)
        throw new Exception("No autorizado");
    
    // Solo se puede cancelar si es efectivo
    if (pedido.MetodoPago != "Efectivo")
        throw new Exception("Solo se pueden cancelar pedidos en efectivo");
    
    if (pedido.Estado != "Pendiente")
        throw new Exception("El pedido ya no se puede cancelar");
    
    pedido.Estado = "Cancelado";
    await _unitOfWork.SaveChangesAsync();
    
    return true;
}
```

---

## ✅ ORDEN DE EJECUCIÓN

### PASO 1: Actualizar Entidades (10 min)
1. Agregar `ReservaHasta` en CarritoItem.cs
2. Agregar campos de bloqueo en User.cs

### PASO 2: Crear Base de Datos (5 min)
1. Actualizar CarritoItemConfiguration.cs
2. Drop database
3. Delete migrations
4. Create migration
5. Update database
6. Crear admin

### PASO 3: Implementar F1 (15 min)
1. Actualizar ProductoResponseDto
2. Actualizar MappingProfile

### PASO 4: Implementar F2 (30 min)
1. Actualizar IProductoRepository
2. Actualizar ProductoRepository
3. Crear ProductoSearchDto
4. Actualizar IProductoService
5. Actualizar ProductoService
6. Agregar endpoints en ProductosController

### PASO 5: Implementar F3 (45 min)
1. Actualizar CarritoService.AddItemAsync
2. Crear ReservaStockService
3. Registrar BackgroundService en Program.cs

### PASO 6: Implementar F5 (15 min)
1. Actualizar CreatePedidoRequestValidator
2. Crear PedidoItemRequestValidator

### PASO 7: Verificar (10 min)
1. dotnet build
2. dotnet run
3. Probar en Swagger

**TIEMPO TOTAL: 2-3 horas**

---

**Fecha**: Febrero 15, 2026  
**Estado**: Plan Definitivo Aprobado  
**Próximo**: Ejecutar PASO 1
