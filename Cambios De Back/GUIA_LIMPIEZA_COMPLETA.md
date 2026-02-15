# 🧹 GUÍA COMPLETA DE LIMPIEZA DEL PROYECTO

## 📋 ÍNDICE
1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Análisis de Cambios](#análisis-de-cambios)
3. [FASE 1: Archivos a Eliminar](#fase-1-archivos-a-eliminar)
4. [FASE 2: Modificación de Entidades (Core)](#fase-2-modificación-de-entidades-core)
5. [FASE 3: Infrastructure (DbContext y UnitOfWork)](#fase-3-infrastructure-dbcontext-y-unitofwork)
6. [FASE 4: Services (DTOs y Servicios)](#fase-4-services-dtos-y-servicios)
7. [FASE 5: Migraciones y Base de Datos](#fase-5-migraciones-y-base-de-datos)
8. [FASE 6: Verificación](#fase-6-verificación)

---

## 📊 RESUMEN EJECUTIVO

### 🎯 Objetivos de Limpieza

1. **ELIMINAR funcionalidad de PERSONALIZACIÓN:**
   - Tortas personalizables
   - Ingredientes adicionales
   - Configuraciones de personalización

2. **ELIMINAR cálculo de IVA:**
   - Productos alimenticios no llevan IVA
   - Simplificar cálculo de totales

### 📈 Impacto en Base de Datos

| Elemento | ANTES | DESPUÉS | Cambio |
|----------|-------|---------|--------|
| **Tablas** | 20 | 17 | -3 tablas |
| **Columnas en Pedido** | 14 | 12 | -2 columnas |
| **Columnas en Factura** | 7 | 6 | -1 columna |
| **Columnas en Producto** | 10 | 9 | -1 columna |

### 🗑️ Tablas a Eliminar
- `PersonalizadoConfigs`
- `Ingredientes`
- `PersonalizadoConfigIngredientes`

### 📝 Columnas a Eliminar
- `Pedido.IVA`
- `Pedido.EsPersonalizado`
- `Factura.IVA`
- `Producto.EsPersonalizable`

---

## 🔍 ANÁLISIS DE CAMBIOS

### Cambios por Capa

```
📦 CORE LAYER (Entidades)
   ├─ ❌ Eliminar: PersonalizadoConfig.cs
   ├─ ❌ Eliminar: Ingrediente.cs
   ├─ ❌ Eliminar: PersonalizadoConfigIngrediente.cs
   ├─ 📝 Modificar: Producto.cs (quitar EsPersonalizable)
   ├─ 📝 Modificar: Pedido.cs (quitar EsPersonalizado, IVA, relación)
   └─ 📝 Modificar: Factura.cs (quitar IVA)

📦 INFRASTRUCTURE LAYER
   ├─ 📝 Modificar: ApplicationDbContext.cs (DbSets, seed data)
   ├─ 📝 Modificar: UnitOfWork.cs (quitar Ingredientes)
   ├─ 📝 Modificar: IUnitOfWork.cs (quitar Ingredientes)
   └─ 🔄 Recrear: Migraciones completas

📦 SERVICES LAYER
   ├─ 📝 Modificar: CreatePedidoRequestDto.cs
   ├─ 📝 Modificar: CreateProductoRequestDto.cs
   ├─ 📝 Modificar: UpdateProductoRequestDto.cs
   ├─ 📝 Modificar: PedidoResponseDto.cs
   ├─ 📝 Modificar: PedidoService.cs
   └─ 📝 Modificar: MappingProfile.cs
```

---

## ⚡ FASE 1: ARCHIVOS A ELIMINAR

### 📂 PastisserieAPI.Core/Entities/

Eliminar estos **3 archivos completos**:

```bash
# Navegar a la carpeta de entidades
cd PastisserieAPI.Core/Entities/

# Eliminar archivos
rm PersonalizadoConfig.cs
rm Ingrediente.cs
rm PersonalizadoConfigIngrediente.cs
```

O manualmente:
- ❌ `PersonalizadoConfig.cs`
- ❌ `Ingrediente.cs`
- ❌ `PersonalizadoConfigIngrediente.cs`

---

## 📝 FASE 2: MODIFICACIÓN DE ENTIDADES (CORE)

### 1️⃣ Producto.cs
**Ubicación**: `PastisserieAPI.Core/Entities/Producto.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public bool EsPersonalizable { get; set; } = false;
```

**✅ RESULTADO FINAL:**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public int? StockMinimo { get; set; }

        [Required]
        [MaxLength(100)]
        public string Categoria { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<PedidoItem> PedidoItems { get; set; } = new List<PedidoItem>();
        public virtual ICollection<CarritoItem> CarritoItems { get; set; } = new List<CarritoItem>();
    }
}
```

---

### 2️⃣ Pedido.cs
**Ubicación**: `PastisserieAPI.Core/Entities/Pedido.cs`

**BUSCAR Y ELIMINAR estas 3 líneas:**
```csharp
public decimal IVA { get; set; }                              // ❌ ELIMINAR
public bool EsPersonalizado { get; set; } = false;            // ❌ ELIMINAR
public virtual PersonalizadoConfig? PersonalizadoConfig { get; set; }  // ❌ ELIMINAR
```

**✅ RESULTADO FINAL:**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public int MetodoPagoId { get; set; }

        public int? DireccionEnvioId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoEnvio { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public bool Aprobado { get; set; } = false;

        public DateTime? FechaAprobacion { get; set; }

        public DateTime? FechaEntregaEstimada { get; set; }

        [MaxLength(1000)]
        public string? NotasCliente { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; } = null!;

        [ForeignKey("MetodoPagoId")]
        public virtual MetodoPagoUsuario MetodoPago { get; set; } = null!;

        [ForeignKey("DireccionEnvioId")]
        public virtual DireccionEnvio? DireccionEnvio { get; set; }

        public virtual ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
        public virtual Factura? Factura { get; set; }
        public virtual Envio? Envio { get; set; }
        public virtual ICollection<PedidoHistorial> Historial { get; set; } = new List<PedidoHistorial>();
    }
}
```

---

### 3️⃣ Factura.cs
**Ubicación**: `PastisserieAPI.Core/Entities/Factura.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public decimal IVA { get; set; }  // ❌ ELIMINAR esta línea completa
```

**✅ RESULTADO FINAL:**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastisserieAPI.Core.Entities
{
    public class Factura
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NumeroFactura { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(500)]
        public string? RutaArchivo { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual Pedido Pedido { get; set; } = null!;
    }
}
```

---

## 🗄️ FASE 3: INFRASTRUCTURE (DBCONTEXT Y UNITOFWORK)

### 4️⃣ ApplicationDbContext.cs
**Ubicación**: `PastisserieAPI.Infrastructure/Data/ApplicationDbContext.cs`

#### **CAMBIO 1: Eliminar DbSets de Personalización**

**BUSCAR:**
```csharp
// ============ PERSONALIZACIÓN ============
public DbSet<PersonalizadoConfig> PersonalizadoConfigs { get; set; }
public DbSet<Ingrediente> Ingredientes { get; set; }
public DbSet<PersonalizadoConfigIngrediente> PersonalizadoConfigIngredientes { get; set; }
```

**ACCIÓN:** ❌ **ELIMINAR toda esta sección**

---

#### **CAMBIO 2: Eliminar Seed Data de Ingredientes**

**BUSCAR en el método `SeedInitialData`:**
```csharp
// ============ INGREDIENTES ============
modelBuilder.Entity<Ingrediente>().HasData(
    new Ingrediente { Id = 1, Nombre = "Arequipe", Descripcion = "Relleno de arequipe", PrecioAdicional = 5000, Activo = true },
    new Ingrediente { Id = 2, Nombre = "Crema de chocolate", Descripcion = "Crema de chocolate belga", PrecioAdicional = 7000, Activo = true },
    new Ingrediente { Id = 3, Nombre = "Fresas frescas", Descripcion = "Fresas naturales", PrecioAdicional = 8000, Activo = true },
    new Ingrediente { Id = 4, Nombre = "Frutas mixtas", Descripcion = "Variedad de frutas", PrecioAdicional = 10000, Activo = true },
    new Ingrediente { Id = 5, Nombre = "Chispas de chocolate", Descripcion = "Chispas de chocolate", PrecioAdicional = 3000, Activo = true },
    new Ingrediente { Id = 6, Nombre = "Nueces", Descripcion = "Nueces tostadas", PrecioAdicional = 6000, Activo = true },
    new Ingrediente { Id = 7, Nombre = "Coco rallado", Descripcion = "Coco natural rallado", PrecioAdicional = 4000, Activo = true }
);
```

**ACCIÓN:** ❌ **ELIMINAR toda esta sección**

---

#### **CAMBIO 3: Eliminar Categoría "Personalizados"**

**BUSCAR:**
```csharp
modelBuilder.Entity<CategoriaProducto>().HasData(
    new CategoriaProducto { Id = 1, Nombre = "Tortas", Descripcion = "Tortas y pasteles", Activa = true },
    new CategoriaProducto { Id = 2, Nombre = "Panes", Descripcion = "Variedad de panes artesanales", Activa = true },
    new CategoriaProducto { Id = 3, Nombre = "Postres", Descripcion = "Postres y dulces", Activa = true },
    new CategoriaProducto { Id = 4, Nombre = "Galletas", Descripcion = "Galletas caseras", Activa = true },
    new CategoriaProducto { Id = 5, Nombre = "Personalizados", Descripcion = "Productos personalizables", Activa = true } // ❌ ELIMINAR ESTA LÍNEA
);
```

**REEMPLAZAR con:**
```csharp
modelBuilder.Entity<CategoriaProducto>().HasData(
    new CategoriaProducto { Id = 1, Nombre = "Tortas", Descripcion = "Tortas y pasteles", Activa = true },
    new CategoriaProducto { Id = 2, Nombre = "Panes", Descripcion = "Variedad de panes artesanales", Activa = true },
    new CategoriaProducto { Id = 3, Nombre = "Postres", Descripcion = "Postres y dulces", Activa = true },
    new CategoriaProducto { Id = 4, Nombre = "Galletas", Descripcion = "Galletas caseras", Activa = true }
);
```

---

### 5️⃣ UnitOfWork.cs
**Ubicación**: `PastisserieAPI.Infrastructure/Repositorie/UnitOfWork.cs`

**BUSCAR Y ELIMINAR:**
```csharp
// En las declaraciones privadas (arriba):
private IRepository<Ingrediente>? _ingredientes;

// En las propiedades públicas:
public IRepository<Ingrediente> Ingredientes
{
    get { return _ingredientes ??= new Repository<Ingrediente>(_context); }
}
```

---

### 6️⃣ IUnitOfWork.cs
**Ubicación**: `PastisserieAPI.Core/Interfaces/IUnitOfWork.cs`

**BUSCAR Y ELIMINAR:**
```csharp
Repositories.IRepository<Entities.Ingrediente> Ingredientes { get; }
```

---

## 💼 FASE 4: SERVICES (DTOS Y SERVICIOS)

### 7️⃣ CreatePedidoRequestDto.cs
**Ubicación**: `PastisserieAPI.Services/DTOs/Request/CreatePedidoRequestDto.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public PersonalizadoConfigRequestDto? PersonalizadoConfig { get; set; }

// Y eliminar la clase completa PersonalizadoConfigRequestDto
public class PersonalizadoConfigRequestDto
{
    public string? Sabor { get; set; }
    public string? Tamano { get; set; }
    public string? Forma { get; set; }
    public string? Color { get; set; }
    public int Niveles { get; set; } = 1;
    public string? Diseno { get; set; }
    public string? ImagenReferenciaUrl { get; set; }
    public string? InstruccionesEspeciales { get; set; }
    public List<int> IngredientesIds { get; set; } = new();
}
```

**✅ RESULTADO FINAL:**
```csharp
namespace PastisserieAPI.Services.DTOs.Request
{
    public class CreatePedidoRequestDto
    {
        public int UsuarioId { get; set; }
        public int MetodoPagoId { get; set; }
        public int? DireccionEnvioId { get; set; }
        public string? NotasCliente { get; set; }
        public List<PedidoItemRequestDto> Items { get; set; } = new();
    }

    public class PedidoItemRequestDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
```

---

### 8️⃣ CreateProductoRequestDto.cs
**Ubicación**: `PastisserieAPI.Services/DTOs/Request/CreateProductoRequestDto.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public bool EsPersonalizable { get; set; } = false;
```

**✅ RESULTADO FINAL:**
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
        public string Categoria { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
    }
}
```

---

### 9️⃣ UpdateProductoRequestDto.cs
**Ubicación**: `PastisserieAPI.Services/DTOs/Request/UpdateProductoRequestDto.cs`

**BUSCAR Y ELIMINAR:**
```csharp
public bool? EsPersonalizable { get; set; }
```

---

### 🔟 PedidoResponseDto.cs
**Ubicación**: `PastisserieAPI.Services/DTOs/Response/PedidoResponseDto.cs`

**BUSCAR Y ELIMINAR estas 2 líneas:**
```csharp
public decimal IVA { get; set; }
public bool EsPersonalizado { get; set; }
```

**✅ RESULTADO FINAL:**
```csharp
namespace PastisserieAPI.Services.DTOs.Response
{
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal Total { get; set; }
        public bool Aprobado { get; set; }
        public DateTime? FechaEntregaEstimada { get; set; }
        public List<PedidoItemResponseDto> Items { get; set; } = new();
        public DireccionEnvioResponseDto? DireccionEnvio { get; set; }
    }

    public class PedidoItemResponseDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
```

---

### 1️⃣1️⃣ PedidoService.cs
**Ubicación**: `PastisserieAPI.Services/Services/PedidoService.cs`

#### **CAMBIO 1: Modificar cálculo de totales (eliminar IVA)**

**BUSCAR:**
```csharp
// Calcular IVA y total
pedido.Subtotal = subtotal;
pedido.IVA = subtotal * 0.19m; // 19% IVA Colombia
pedido.CostoEnvio = subtotal >= 50000 ? 0 : 5000;
pedido.Total = pedido.Subtotal + pedido.IVA + pedido.CostoEnvio;
```

**REEMPLAZAR con:**
```csharp
// Calcular totales (sin IVA para productos alimenticios)
pedido.Subtotal = subtotal;
pedido.CostoEnvio = subtotal >= 50000 ? 0 : 5000; // Envío gratis sobre $50,000
pedido.Total = pedido.Subtotal + pedido.CostoEnvio;
```

---

#### **CAMBIO 2: Eliminar lógica de personalización**

**BUSCAR Y ELIMINAR TODO ESTE BLOQUE:**
```csharp
// Si es personalizado, crear configuración
if (request.PersonalizadoConfig != null)
{
    var config = _mapper.Map<PersonalizadoConfig>(request.PersonalizadoConfig);
    config.PedidoId = pedido.Id;

    // Calcular precio adicional por ingredientes
    decimal precioIngredientes = 0;
    foreach (var ingredienteId in request.PersonalizadoConfig.IngredientesIds)
    {
        var ingrediente = await _unitOfWork.Ingredientes.GetByIdAsync(ingredienteId);
        if (ingrediente != null)
        {
            precioIngredientes += ingrediente.PrecioAdicional;
        }
    }

    config.PrecioAdicional = precioIngredientes;
    pedido.Total += precioIngredientes;

    await _unitOfWork.SaveChangesAsync();
}
```

**✅ MÉTODO COMPLETO DESPUÉS DE CAMBIOS:**
```csharp
public async Task<PedidoResponseDto> CreateAsync(CreatePedidoRequestDto request)
{
    // Crear pedido base
    var pedido = _mapper.Map<Pedido>(request);

    // Calcular totales y crear items
    decimal subtotal = 0;
    var pedidoItems = new List<PedidoItem>();

    foreach (var itemRequest in request.Items)
    {
        var producto = await _unitOfWork.Productos.GetByIdAsync(itemRequest.ProductoId);

        if (producto == null || !producto.Activo)
            throw new Exception($"Producto con ID {itemRequest.ProductoId} no encontrado o inactivo");

        if (producto.Stock < itemRequest.Cantidad)
            throw new Exception($"Stock insuficiente para el producto {producto.Nombre}");

        var pedidoItem = new PedidoItem
        {
            ProductoId = itemRequest.ProductoId,
            Cantidad = itemRequest.Cantidad,
            PrecioUnitario = producto.Precio,
            Subtotal = producto.Precio * itemRequest.Cantidad
        };

        subtotal += pedidoItem.Subtotal;
        pedidoItems.Add(pedidoItem);

        // Reducir stock
        producto.Stock -= itemRequest.Cantidad;
        await _unitOfWork.Productos.UpdateAsync(producto);
    }

    // Calcular totales (sin IVA para productos alimenticios)
    pedido.Subtotal = subtotal;
    pedido.CostoEnvio = subtotal >= 50000 ? 0 : 5000; // Envío gratis sobre $50,000
    pedido.Total = pedido.Subtotal + pedido.CostoEnvio;

    // Crear pedido
    await _unitOfWork.Pedidos.AddAsync(pedido);
    await _unitOfWork.SaveChangesAsync();

    // Agregar items al pedido
    foreach (var item in pedidoItems)
    {
        item.PedidoId = pedido.Id;
    }
    await _unitOfWork.SaveChangesAsync();

    // Crear historial
    var historial = new PedidoHistorial
    {
        PedidoId = pedido.Id,
        EstadoAnterior = "",
        EstadoNuevo = "Pendiente",
        FechaCambio = DateTime.UtcNow,
        CambiadoPor = request.UsuarioId,
        Notas = "Pedido creado"
    };

    await _unitOfWork.SaveChangesAsync();

    // Obtener pedido completo
    var pedidoCompleto = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(pedido.Id);
    return _mapper.Map<PedidoResponseDto>(pedidoCompleto!);
}
```

---

### 1️⃣2️⃣ MappingProfile.cs
**Ubicación**: `PastisserieAPI.Services/Mappings/MappingProfile.cs`

**BUSCAR Y ELIMINAR:**
```csharp
// En CreateMap<CreatePedidoRequestDto, Pedido>:
.ForMember(dest => dest.EsPersonalizado, opt => opt.MapFrom(src => src.PersonalizadoConfig != null))
.ForMember(dest => dest.PersonalizadoConfig, opt => opt.Ignore());

// Eliminar este mapping completo:
CreateMap<PersonalizadoConfigRequestDto, PersonalizadoConfig>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.PedidoId, opt => opt.Ignore())
    .ForMember(dest => dest.PrecioAdicional, opt => opt.Ignore())
    .ForMember(dest => dest.Ingredientes, opt => opt.Ignore());
```

**✅ MAPPING DE PEDIDO DESPUÉS DE CAMBIOS:**
```csharp
// ============ PEDIDO MAPPINGS ============
CreateMap<CreatePedidoRequestDto, Pedido>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.FechaPedido, opt => opt.MapFrom(src => DateTime.UtcNow))
    .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"))
    .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => 0))
    .ForMember(dest => dest.CostoEnvio, opt => opt.MapFrom(src => 0))
    .ForMember(dest => dest.Total, opt => opt.MapFrom(src => 0))
    .ForMember(dest => dest.Aprobado, opt => opt.MapFrom(src => false))
    .ForMember(dest => dest.Items, opt => opt.Ignore());

CreateMap<PedidoItemRequestDto, PedidoItem>()
    .ForMember(dest => dest.Id, opt => opt.Ignore())
    .ForMember(dest => dest.PedidoId, opt => opt.Ignore())
    .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore())
    .ForMember(dest => dest.Subtotal, opt => opt.Ignore());
```

---

## 🗄️ FASE 5: MIGRACIONES Y BASE DE DATOS

### ⚠️ IMPORTANTE: Elección de Estrategia

**Elige UNA de estas opciones:**

---

### 🔴 OPCIÓN A: PROYECTO NUEVO / SIN DATOS IMPORTANTES (RECOMENDADO)

Si no tienes datos importantes en la base de datos, esta es la opción más limpia.

#### **Paso 1: Eliminar Base de Datos**
```bash
dotnet ef database drop --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API --force
```

#### **Paso 2: Eliminar Carpeta de Migraciones**
```bash
# Desde la raíz del proyecto
rm -rf PastisserieAPI.Infrastructure/Migrations

# O manualmente en Windows:
# Eliminar la carpeta: PastisserieAPI.Infrastructure/Migrations
```

#### **Paso 3: Crear Nueva Migración Limpia**
```bash
dotnet ef migrations add InitialCreate --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

#### **Paso 4: Aplicar Migración**
```bash
dotnet ef database update --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

#### **Paso 5: Crear Usuario Administrador**
Seguir las instrucciones en: `PastisserieAPI.API/Database/Scripts/README.md`

---

### 🟡 OPCIÓN B: PROYECTO CON DATOS / BASE DE DATOS EN USO

Si tienes datos importantes y NO puedes eliminar la base de datos.

#### **Paso 1: Crear Migración de Eliminación**
```bash
dotnet ef migrations add RemovePersonalizacionAndIVA --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

Esta migración generará automáticamente:
- `DROP TABLE PersonalizadoConfigIngredientes`
- `DROP TABLE PersonalizadoConfigs`
- `DROP TABLE Ingredientes`
- `ALTER TABLE Productos DROP COLUMN EsPersonalizable`
- `ALTER TABLE Pedidos DROP COLUMN EsPersonalizado`
- `ALTER TABLE Pedidos DROP COLUMN IVA`
- `ALTER TABLE Facturas DROP COLUMN IVA`
- `DELETE FROM CategoriasProducto WHERE Id = 5` (Personalizados)

#### **Paso 2: Aplicar Migración**
```bash
dotnet ef database update --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

---

## ✅ FASE 6: VERIFICACIÓN

### Checklist de Archivos

#### ❌ Archivos Eliminados:
- [ ] `PastisserieAPI.Core/Entities/PersonalizadoConfig.cs`
- [ ] `PastisserieAPI.Core/Entities/Ingrediente.cs`
- [ ] `PastisserieAPI.Core/Entities/PersonalizadoConfigIngrediente.cs`

#### 📝 Archivos Modificados (CORE):
- [ ] `Producto.cs` - sin EsPersonalizable
- [ ] `Pedido.cs` - sin IVA, EsPersonalizado, PersonalizadoConfig
- [ ] `Factura.cs` - sin IVA

#### 📝 Archivos Modificados (INFRASTRUCTURE):
- [ ] `ApplicationDbContext.cs` - sin DbSets de personalización, sin seed data
- [ ] `UnitOfWork.cs` - sin Ingredientes
- [ ] `IUnitOfWork.cs` - sin Ingredientes

#### 📝 Archivos Modificados (SERVICES):
- [ ] `CreatePedidoRequestDto.cs` - sin PersonalizadoConfigRequestDto
- [ ] `CreateProductoRequestDto.cs` - sin EsPersonalizable
- [ ] `UpdateProductoRequestDto.cs` - sin EsPersonalizable
- [ ] `PedidoResponseDto.cs` - sin IVA, EsPersonalizado
- [ ] `PedidoService.cs` - sin lógica de IVA y personalización
- [ ] `MappingProfile.cs` - sin mappings de personalización

---

### Verificación de Compilación

```bash
# 1. Limpiar proyecto
dotnet clean

# 2. Restaurar paquetes
dotnet restore

# 3. Compilar (NO debe haber errores)
dotnet build

# ✅ Debe mostrar: Build succeeded. 0 Warning(s). 0 Error(s).
```

---

### Verificación de Base de Datos

**Conectar a SQL Server y verificar:**

```sql
-- Debe mostrar 17 tablas (no 20)
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Verificar que NO existen estas tablas:
-- ❌ PersonalizadoConfigs
-- ❌ Ingredientes
-- ❌ PersonalizadoConfigIngredientes

-- Verificar columnas de Pedido (debe tener 12 columnas, no 14)
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pedidos'
ORDER BY ORDINAL_POSITION;

-- NO deben existir estas columnas:
-- ❌ IVA
-- ❌ EsPersonalizado

-- Verificar columnas de Factura (debe tener 6 columnas, no 7)
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Facturas'
ORDER BY ORDINAL_POSITION;

-- NO debe existir:
-- ❌ IVA

-- Verificar columnas de Producto
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Productos'
ORDER BY ORDINAL_POSITION;

-- NO debe existir:
-- ❌ EsPersonalizable
```

---

### Verificación de Funcionalidad

```bash
# Ejecutar la API
dotnet run --project PastisserieAPI.API

# Debe arrancar sin errores en: http://localhost:5001
```

**Verificar en Swagger** (`http://localhost:5001/swagger`):
- [ ] Endpoint de Productos NO tiene campo `esPersonalizable`
- [ ] Endpoint de Pedidos NO tiene campo `iva` ni `esPersonalizado`
- [ ] NO existen endpoints de Ingredientes o PersonalizadoConfig

---

## 📊 RESUMEN DE CAMBIOS

### Antes vs Después

| Concepto | ANTES | DESPUÉS |
|----------|-------|---------|
| **Tablas** | 20 | 17 |
| **Entidades C#** | 20 | 17 |
| **DbSets** | 20 | 17 |
| **Columna Pedido.IVA** | ✅ Existe | ❌ Eliminada |
| **Columna Pedido.EsPersonalizado** | ✅ Existe | ❌ Eliminada |
| **Columna Factura.IVA** | ✅ Existe | ❌ Eliminada |
| **Columna Producto.EsPersonalizable** | ✅ Existe | ❌ Eliminada |
| **Cálculo Total Pedido** | Subtotal + IVA + Envío | Subtotal + Envío |

---

### Impacto en Funcionalidades

| Funcionalidad | Estado |
|---------------|--------|
| ✅ Autenticación JWT | **INTACTA** |
| ✅ CRUD de Productos | **INTACTA** |
| ✅ Carrito de Compras | **INTACTA** |
| ✅ Sistema de Pedidos | **SIMPLIFICADA** (sin IVA) |
| ✅ Reviews y Calificaciones | **INTACTA** |
| ✅ Envíos | **INTACTA** |
| ✅ Métodos de Pago | **INTACTA** |
| ❌ Tortas Personalizables | **ELIMINADA** |
| ❌ Ingredientes Adicionales | **ELIMINADA** |
| ❌ Cálculo de IVA | **ELIMINADA** |

---

## 🎯 RESULTADO ESPERADO

Después de aplicar todos los cambios:

✅ **API funcionando** sin errores de compilación  
✅ **Base de datos limpia** con 17 tablas  
✅ **Sistema simplificado** de pastelería estándar  
✅ **Pedidos sin IVA** (apropiado para productos alimenticios)  
✅ **Sin personalización** de productos  
✅ **Cálculo de totales**: Subtotal + CostoEnvío = Total  

---

## ⏱️ TIEMPO ESTIMADO

- **Modificación de archivos**: 30-45 minutos
- **Migraciones y BD**: 5-10 minutos
- **Testing y verificación**: 15-20 minutos
- **TOTAL**: 50-75 minutos

---

## 🆘 SOLUCIÓN DE PROBLEMAS

### Error: "Type PersonalizadoConfig not found"
**Causa**: Referencias no eliminadas en otros archivos  
**Solución**: Buscar en todo el proyecto (Ctrl+Shift+F) "PersonalizadoConfig" y eliminar referencias

### Error: "Column IVA does not exist"
**Causa**: Migraciones no aplicadas correctamente  
**Solución**: Recrear la base de datos (Opción A)

### Error: Compilación con warnings
**Causa**: Imports/usings no utilizados  
**Solución**: En Visual Studio → Quick Actions → Remove Unnecessary Usings

---

**Fecha**: Febrero 14, 2026  
**Alcance**: Eliminación de Personalización + IVA  
**Versión**: 1.0 - Documento Unificado
