# 🔍 ANÁLISIS DE RELACIONES DE BASE DE DATOS - INFRASTRUCTURE LAYER

## 📊 ÍNDICE
1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Diagrama Actual de Relaciones](#diagrama-actual-de-relaciones)
3. [Análisis por Módulo](#análisis-por-módulo)
4. [Problemas Identificados](#problemas-identificados)
5. [Recomendaciones de Optimización](#recomendaciones-de-optimización)
6. [Cambios Propuestos](#cambios-propuestos)

---

## 📊 RESUMEN EJECUTIVO

### Estado Actual (DESPUÉS de limpieza)
- **17 Tablas** operativas
- **30+ Relaciones** de Foreign Keys
- **3 Configuraciones Fluent API** personalizadas
- **Relaciones adicionales** en ApplicationDbContext.ConfigureRelationships

### Hallazgos Principales

✅ **BIEN DISEÑADO:**
- Separación clara de responsabilidades
- Uso correcto de relaciones 1:1, 1:N, N:M
- Índices en columnas de búsqueda frecuente
- Delete Behaviors apropiados

⚠️ **OPORTUNIDADES DE MEJORA:**
- Redundancia: `CategoriaProducto` vs columna `Producto.Categoria` (string)
- Complejidad: `MetodoPagoUsuario` podría simplificarse
- Falta: Configuración Fluent API para algunas entidades
- Indexación: Faltan índices compuestos importantes

---

## 🗺️ DIAGRAMA ACTUAL DE RELACIONES

### MÓDULO 1: USUARIOS Y AUTENTICACIÓN

```
┌─────────────────┐
│      User       │
│─────────────────│
│ Id (PK)         │
│ Nombre          │
│ Email (UNIQUE)  │
└─────────────────┘
         │
         │ 1:N
         ├────────────────────────────────────┐
         │                                    │
┌────────▼─────────┐                  ┌──────▼────────┐
│    UserRol       │   N:1            │   Pedido      │
│──────────────────│◄────────────────►│───────────────│
│ Id (PK)          │                  │ Id (PK)       │
│ UsuarioId (FK)   │                  │ UsuarioId(FK) │
│ RolId (FK)       │                  └───────────────┘
└──────────────────┘
         │ N:1
         │
┌────────▼─────────┐
│      Rol         │
│──────────────────│
│ Id (PK)          │
│ Nombre (UNIQUE)  │
└──────────────────┘
```

**Relaciones:**
- User → UserRol (1:N) - Un usuario puede tener múltiples roles
- UserRol → Rol (N:1) - Tabla intermedia para N:M
- User → Pedido (1:N) - Un usuario puede tener múltiples pedidos
- User → Review (1:N) - Un usuario puede hacer múltiples reviews
- User → CarritoCompra (1:1) - Un usuario tiene un carrito
- User → DireccionEnvio (1:N) - Un usuario puede tener múltiples direcciones
- User → MetodoPagoUsuario (1:N) - Un usuario puede tener múltiples métodos de pago
- User → Notificacion (1:N) - Un usuario recibe múltiples notificaciones
- User → Envio (1:N como Repartidor) - Un domiciliario puede tener múltiples envíos asignados

**DeleteBehavior:**
- UserRol, CarritoCompra, DireccionEnvio, MetodoPagoUsuario, Notificacion: **Cascade**
- Pedido, Review, Envio (Repartidor): **Restrict** ✅ (Correcto: preserva datos históricos)

---

### MÓDULO 2: PRODUCTOS Y CATÁLOGO

```
┌──────────────────────┐
│  CategoriaProducto   │
│──────────────────────│
│ Id (PK)              │
│ Nombre               │
│ Activa               │
└──────────────────────┘
         │ 1:N (OPCIONAL)
         ▼
┌──────────────────────┐
│     Producto         │
│──────────────────────│
│ Id (PK)              │
│ Nombre (INDEX)       │
│ Categoria (string)   │◄─────── ⚠️ REDUNDANCIA
│ Precio               │
│ Stock                │
│ CategoriaProductoId? │◄─────── FK Opcional
└──────────────────────┘
         │ 1:N
         ├──────────────────────────┐
         │                          │
┌────────▼─────────┐      ┌────────▼─────────┐
│  PedidoItem      │      │   CarritoItem    │
│──────────────────│      │──────────────────│
│ ProductoId (FK)  │      │ ProductoId (FK)  │
└──────────────────┘      └──────────────────┘
         │
         │ 1:N
┌────────▼─────────┐
│     Review       │
│──────────────────│
│ ProductoId (FK)  │
│ UsuarioId (FK)   │
└──────────────────┘
```

**Problema Identificado:**
```sql
-- ⚠️ REDUNDANCIA: Producto tiene AMBOS
Producto.Categoria (string) = "Tortas"
Producto.CategoriaProductoId = 1  -- FK a tabla CategoriaProducto

-- Solo debería tener UNO
```

---

### MÓDULO 3: CARRITO DE COMPRAS

```
┌─────────────────┐
│      User       │
└────────┬────────┘
         │ 1:1
         ▼
┌─────────────────────┐
│  CarritoCompra      │
│─────────────────────│
│ Id (PK)             │
│ UsuarioId (FK)      │ UNIQUE
└──────────┬──────────┘
           │ 1:N
           ▼
┌─────────────────────┐
│   CarritoItem       │
│─────────────────────│
│ Id (PK)             │
│ CarritoId (FK)      │
│ ProductoId (FK)     │
│ Cantidad            │
└─────────────────────┘
```

**DeleteBehavior:**
- CarritoCompra → User: **Cascade** ✅
- CarritoItem → CarritoCompra: **Cascade** ✅
- CarritoItem → Producto: **Cascade** ⚠️ (¿Debería ser Restrict?)

---

### MÓDULO 4: PEDIDOS (NÚCLEO DEL SISTEMA)

```
┌─────────────────┐
│    Pedido       │
│─────────────────│
│ Id (PK)         │
│ UsuarioId (FK)  │
│ MetodoPagoId(FK)│
│ DireccionId(FK) │
│ Estado (INDEX)  │
│ FechaPedido(IDX)│
└────────┬────────┘
         │
         ├─── 1:N ────► PedidoItem
         │
         ├─── 1:1 ────► Factura
         │
         ├─── 1:0..1 ─► Envio
         │
         └─── 1:N ────► PedidoHistorial
```

**Relaciones Detalladas:**

1. **Pedido → PedidoItem (1:N)**
   - DeleteBehavior: **Cascade** ✅
   - Cuando se elimina pedido, se eliminan items

2. **Pedido → User (N:1)**
   - DeleteBehavior: **Restrict** ✅
   - No se puede eliminar usuario con pedidos

3. **Pedido → MetodoPagoUsuario (N:1)**
   - DeleteBehavior: **Cascade** ⚠️
   - ¿Debería ser Restrict para preservar histórico?

4. **Pedido → DireccionEnvio (N:1)**
   - DeleteBehavior: **No Action/Null**
   - FK es nullable (opcional)

5. **Pedido → Factura (1:1)**
   - DeleteBehavior: **Cascade** ✅
   - Factura depende totalmente del pedido

6. **Pedido → Envio (1:0..1)**
   - DeleteBehavior: **Cascade** ✅
   - No todos los pedidos tienen envío (recogida en tienda)

7. **Pedido → PedidoHistorial (1:N)**
   - DeleteBehavior: **Cascade** ✅
   - Historial de cambios de estado

---

### MÓDULO 5: MÉTODOS DE PAGO

```
┌──────────────────────┐
│  TipoMetodoPago      │
│──────────────────────│
│ Id (PK)              │
│ Nombre ("Efectivo")  │
│ Descripcion          │
└──────────┬───────────┘
           │ 1:N
           ▼
┌──────────────────────┐
│ MetodoPagoUsuario    │
│──────────────────────│
│ Id (PK)              │
│ UsuarioId (FK)       │
│ TipoMetodoPagoId(FK) │
│ TokenPago            │
│ UltimosDigitos       │
│ EsPredeterminado     │
└──────────┬───────────┘
           │ 1:N
           ▼
┌──────────────────────┐
│       Pedido         │
│──────────────────────│
│ MetodoPagoId (FK)    │
└──────────────────────┘
```

**Análisis:**
- ✅ Bien normalizado
- ⚠️ `TokenPago` almacena tokens de pasarelas (Wompi, etc.)
- ⚠️ Considerar: ¿Es necesaria esta tabla o mejor almacenar solo TipoMetodoPagoId directo en Pedido?

---

### MÓDULO 6: ENVÍOS Y DIRECCIONES

```
┌─────────────────┐
│      User       │
└────────┬────────┘
         │ 1:N
         ▼
┌──────────────────────┐
│  DireccionEnvio      │
│──────────────────────│
│ Id (PK)              │
│ UsuarioId (FK)       │
│ Direccion            │
│ Barrio               │
│ Telefono             │
│ EsPredeterminada     │
└──────────┬───────────┘
           │ 1:N
           ▼
┌──────────────────────┐
│       Pedido         │
│──────────────────────│
│ DireccionEnvioId(FK) │
└──────────┬───────────┘
           │ 1:0..1
           ▼
┌──────────────────────┐
│       Envio          │
│──────────────────────│
│ Id (PK)              │
│ PedidoId (FK) UNIQUE │
│ RepartidorId (FK)    │
│ NumeroGuia           │
│ Estado               │
│ FechaDespacho        │
│ FechaEntrega         │
└──────────────────────┘
           │ N:1
           ▼
┌──────────────────────┐
│   User (Repartidor)  │
│──────────────────────│
│ Id (con rol          │
│   Domiciliario)      │
└──────────────────────┘
```

**DeleteBehavior:**
- DireccionEnvio → User: **Cascade** ✅
- Envio → Pedido: **Cascade** ✅
- Envio → User (Repartidor): **Restrict** ✅

---

### MÓDULO 7: REVIEWS Y CALIFICACIONES

```
┌─────────────────┐       ┌─────────────────┐
│      User       │       │    Producto     │
└────────┬────────┘       └────────┬────────┘
         │                         │
         │ N:1              N:1    │
         └──────────┬──────────────┘
                    │
                    ▼
           ┌─────────────────┐
           │     Review      │
           │─────────────────│
           │ Id (PK)         │
           │ UsuarioId (FK)  │
           │ ProductoId (FK) │
           │ Calificacion    │
           │ Comentario      │
           │ Aprobada        │
           │ AprobadaPor     │
           └─────────────────┘
```

**DeleteBehavior:**
- Review → User: **Restrict** ✅ (Preserva reviews aunque se elimine usuario)
- Review → Producto: **Cascade** ✅ (Si se elimina producto, se eliminan sus reviews)

---

## 🚨 PROBLEMAS IDENTIFICADOS

### 1. REDUNDANCIA: CategoriaProducto

**Problema:**
```csharp
// Producto.cs tiene AMBOS:
public string Categoria { get; set; } = string.Empty;  // String directo
public int? CategoriaProductoId { get; set; }          // FK a tabla (OPCIONAL)
```

**Consecuencias:**
- Duplicación de datos
- Posible inconsistencia (¿cuál es la fuente de verdad?)
- Confusión para desarrolladores
- FK opcional hace que la tabla CategoriaProducto sea poco útil

**Impacto:** ⚠️ **MEDIO** - No rompe funcionalidad pero es mala práctica

---

### 2. COMPLEJIDAD: MetodoPagoUsuario

**Problema:**
```
User → MetodoPagoUsuario → TipoMetodoPago → Pedido
```

**Análisis:**
- La tabla `MetodoPagoUsuario` almacena tokens de pago guardados
- Para pagos simples (efectivo, contra entrega), esta complejidad es innecesaria
- ¿Realmente necesitamos guardar todos los métodos de pago del usuario?

**Pregunta:** ¿El negocio requiere que los usuarios guarden métodos de pago o solo seleccionan en cada compra?

**Impacto:** ⚠️ **BAJO-MEDIO** - Depende del modelo de negocio

---

### 3. DELETE BEHAVIORS INCONSISTENTES

**Problema:**
```csharp
// Pedido → MetodoPagoUsuario
.OnDelete(DeleteBehavior.Cascade)  // ⚠️ Si elimino método de pago, se borran pedidos históricos

// CarritoItem → Producto
.OnDelete(DeleteBehavior.Cascade)  // ⚠️ Si elimino producto, se vacían carritos automáticamente
```

**Debería ser:**
```csharp
// Pedido → MetodoPagoUsuario
.OnDelete(DeleteBehavior.Restrict)  // ✅ No permitir eliminar métodos de pago con pedidos

// CarritoItem → Producto
.OnDelete(DeleteBehavior.SetNull o Restrict)  // ✅ Mejor manejar en lógica de negocio
```

**Impacto:** ⚠️ **ALTO** - Puede causar pérdida de datos históricos

---

### 4. FALTA: Configuraciones Fluent API

**Problema:**
Solo existen 3 archivos de configuración:
- `UserConfiguration.cs`
- `ProductoConfiguration.cs`
- `PedidoConfiguration.cs`

**Faltan configuraciones para:**
- `Review`
- `CarritoCompra`
- `DireccionEnvio`
- `MetodoPagoUsuario`
- `Envio`
- `Factura`
- `Notificacion`
- Y 7 entidades más...

**Consecuencia:**
- Todas las relaciones están en `ApplicationDbContext.ConfigureRelationships`
- Difícil de mantener
- No sigue el patrón establecido

**Impacto:** ⚠️ **MEDIO** - Mantenibilidad reducida

---

### 5. ÍNDICES: Faltan índices compuestos

**Faltan índices en:**
```sql
-- Búsquedas frecuentes sin índice:
Reviews: (ProductoId, Aprobada)  -- Filtrar reviews aprobadas por producto
Pedidos: (UsuarioId, Estado)     -- Mis pedidos por estado
Envios: (RepartidorId, Estado)   -- Envíos pendientes de un domiciliario
Notificaciones: (UsuarioId, Leida)  -- Notificaciones no leídas
```

**Impacto:** ⚠️ **MEDIO-ALTO** - Performance en producción

---

## ✅ RECOMENDACIONES DE OPTIMIZACIÓN

### PRIORIDAD ALTA 🔴

#### 1. **Corregir Delete Behaviors**

**Cambiar en `ApplicationDbContext.ConfigureRelationships`:**

```csharp
// ❌ ANTES:
modelBuilder.Entity<Pedido>()
    .HasOne(p => p.MetodoPago)
    .WithMany(mp => mp.Pedidos)
    .HasForeignKey(p => p.MetodoPagoId)
    .OnDelete(DeleteBehavior.Cascade);

// ✅ DESPUÉS:
modelBuilder.Entity<Pedido>()
    .HasOne(p => p.MetodoPago)
    .WithMany(mp => mp.Pedidos)
    .HasForeignKey(p => p.MetodoPagoId)
    .OnDelete(DeleteBehavior.Restrict);  // Preservar histórico
```

**Aplicar también a:**
- `CarritoItem → Producto`: Cambiar a `Restrict` o `SetNull`

---

#### 2. **Resolver Redundancia de Categoría**

**OPCIÓN A: Usar solo FK (RECOMENDADO)**

```csharp
// Producto.cs - ELIMINAR:
public string Categoria { get; set; }  // ❌ ELIMINAR

// MANTENER SOLO:
[Required]
public int CategoriaId { get; set; }   // ✅ FK obligatorio
public virtual CategoriaProducto Categoria { get; set; } = null!;
```

**Migración necesaria:**
```sql
-- 1. Migrar datos existentes
UPDATE Productos 
SET CategoriaProductoId = (
    SELECT Id FROM CategoriasProducto 
    WHERE Nombre = Productos.Categoria
)
WHERE CategoriaProductoId IS NULL;

-- 2. Hacer FK NOT NULL
ALTER TABLE Productos ALTER COLUMN CategoriaProductoId INT NOT NULL;

-- 3. Eliminar columna string
ALTER TABLE Productos DROP COLUMN Categoria;
```

**OPCIÓN B: Usar solo String (más simple para proyecto pequeño)**

```csharp
// Producto.cs - MANTENER SOLO:
[Required]
public string Categoria { get; set; } = string.Empty;  // ✅

// ELIMINAR:
public int? CategoriaProductoId { get; set; }  // ❌
```

**Y eliminar tabla:**
```sql
DROP TABLE CategoriasProducto;
```

---

### PRIORIDAD MEDIA 🟡

#### 3. **Crear Configuraciones Fluent API faltantes**

**Crear archivo: `ReviewConfiguration.cs`**
```csharp
public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Calificacion)
            .IsRequired();

        builder.Property(r => r.Comentario)
            .HasMaxLength(1000);

        // Índice compuesto para búsquedas optimizadas
        builder.HasIndex(r => new { r.ProductoId, r.Aprobada })
            .HasDatabaseName("IX_Reviews_ProductoId_Aprobada");

        // Relaciones (moverlas desde ApplicationDbContext)
        builder.HasOne(r => r.Usuario)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Producto)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Crear también:**
- `EnvioConfiguration.cs`
- `FacturaConfiguration.cs`
- `DireccionEnvioConfiguration.cs`
- `CarritoConfiguration.cs`
- etc.

---

#### 4. **Agregar Índices Compuestos**

**En cada configuración, agregar:**

```csharp
// PedidoConfiguration.cs
builder.HasIndex(p => new { p.UsuarioId, p.Estado })
    .HasDatabaseName("IX_Pedidos_UsuarioId_Estado");

builder.HasIndex(p => new { p.UsuarioId, p.FechaPedido })
    .HasDatabaseName("IX_Pedidos_UsuarioId_FechaPedido");

// ReviewConfiguration.cs
builder.HasIndex(r => new { r.ProductoId, r.Aprobada })
    .HasDatabaseName("IX_Reviews_ProductoId_Aprobada");

// EnvioConfiguration.cs (CREAR)
builder.HasIndex(e => new { e.RepartidorId, e.Estado })
    .HasDatabaseName("IX_Envios_RepartidorId_Estado");

// NotificacionConfiguration.cs (CREAR)
builder.HasIndex(n => new { n.UsuarioId, n.Leida })
    .HasDatabaseName("IX_Notificaciones_UsuarioId_Leida");
```

---

### PRIORIDAD BAJA 🟢

#### 5. **Simplificar MetodoPagoUsuario (Opcional)**

**SI el negocio NO requiere guardar métodos de pago:**

```csharp
// Pedido.cs - Simplificar
[Required]
public int TipoMetodoPagoId { get; set; }  // Directo a tabla de tipos

// ELIMINAR:
public int MetodoPagoId { get; set; }  // ❌
public virtual MetodoPagoUsuario MetodoPago { get; set; }  // ❌

// AGREGAR:
public virtual TipoMetodoPago TipoMetodoPago { get; set; } = null!;  // ✅
```

**Y eliminar tabla `MetodoPagoUsuario`**

**VENTAJAS:**
- Menos complejidad
- Menos joins en queries
- Modelo más simple

**DESVENTAJAS:**
- No se pueden guardar tarjetas del usuario
- No hay tokenización de pagos

---

#### 6. **Agregar Validación de Unicidad**

**Para evitar reviews duplicadas:**

```csharp
// ReviewConfiguration.cs
builder.HasIndex(r => new { r.UsuarioId, r.ProductoId })
    .IsUnique()
    .HasDatabaseName("IX_Reviews_UsuarioId_ProductoId_Unique");
```

**Para evitar carritos duplicados (ya existe en UserConfiguration):**
```csharp
// Ya implementado ✅
builder.HasIndex(u => u.Email).IsUnique();
```

---

## 📝 CAMBIOS PROPUESTOS - PLAN DE ACCIÓN

### FASE 1: Correcciones Críticas (HACER AHORA)

**1. Actualizar PedidoConfiguration.cs**

```csharp
// Ubicación: PastisserieAPI.Infrastructure/Data/Configurations/PedidoConfiguration.cs

// BUSCAR línea ~43:
builder.HasOne(p => p.PersonalizadoConfig)  // ❌ ELIMINAR (ya no existe)
    .WithOne(pc => pc.Pedido)
    .HasForeignKey<PersonalizadoConfig>(pc => pc.PedidoId)
    .OnDelete(DeleteBehavior.Cascade);

// ELIMINAR línea ~15:
builder.Property(p => p.IVA)  // ❌ ELIMINAR (columna eliminada)
    .HasColumnType("decimal(18,2)");
```

---

**2. Actualizar ApplicationDbContext.ConfigureRelationships**

```csharp
// Ubicación: PastisserieAPI.Infrastructure/Data/ApplicationDbContext.cs

// CAMBIAR Delete Behavior:
modelBuilder.Entity<Pedido>()
    .HasOne(p => p.MetodoPago)
    .WithMany(mp => mp.Pedidos)
    .HasForeignKey(p => p.MetodoPagoId)
    .OnDelete(DeleteBehavior.Restrict);  // ✅ Cambiar de Cascade a Restrict
```

---

**3. Decidir sobre CategoriaProducto**

**DECISIÓN REQUERIDA:** ¿Mantener tabla o usar solo string?

**Si tu respuesta es "Tabla CategoriaProducto":**
- Eliminar columna `Producto.Categoria` (string)
- Hacer `CategoriaProductoId` NOT NULL
- Crear migración

**Si tu respuesta es "String simple":**
- Eliminar tabla `CategoriaProducto`
- Eliminar FK `CategoriaProductoId`
- Mantener columna string
- Crear migración

---

### FASE 2: Mejoras de Mantenibilidad (DESPUÉS)

**1. Crear configuraciones Fluent API faltantes**
- ReviewConfiguration.cs
- EnvioConfiguration.cs
- FacturaConfiguration.cs
- CarritoConfiguration.cs
- DireccionEnvioConfiguration.cs
- NotificacionConfiguration.cs

**2. Mover relaciones desde ConfigureRelationships a archivos específicos**

---

### FASE 3: Optimizaciones de Performance (FUTURO)

**1. Agregar índices compuestos**
**2. Considerar simplificar MetodoPagoUsuario**
**3. Agregar índices de unicidad donde aplique**

---

## 📊 RESUMEN DE ESTADO

### Calificación General: 7/10

**Fortalezas:**
- ✅ Separación clara de módulos
- ✅ Uso apropiado de Delete Behaviors en la mayoría de casos
- ✅ Índices básicos implementados
- ✅ Relaciones bien definidas

**Áreas de Mejora:**
- ⚠️ Redundancia Producto.Categoria
- ⚠️ Falta de configuraciones Fluent API
- ⚠️ Algunos Delete Behaviors incorrectos
- ⚠️ Faltan índices compuestos para queries comunes

---

## 🎯 RECOMENDACIÓN FINAL

Para un proyecto de pastelería de tamaño pequeño-mediano:

### 🔴 HACER AHORA (Crítico):
1. ✅ Actualizar PedidoConfiguration (eliminar IVA y PersonalizadoConfig)
2. ✅ Cambiar Delete Behavior de Pedido → MetodoPago a Restrict
3. ✅ **DECIDIR** y resolver redundancia de Categoría

### 🟡 HACER PRONTO (Importante):
4. Crear ReviewConfiguration con índice compuesto
5. Crear EnvioConfiguration con índice compuesto

### 🟢 HACER DESPUÉS (Mejora):
6. Completar todas las configuraciones Fluent API
7. Agregar índices compuestos adicionales
8. Evaluar simplificar MetodoPagoUsuario según negocio

---

**Fecha de Análisis:** Febrero 15, 2026  
**Estado:** Pendiente de decisión sobre CategoriaProducto  
**Prioridad:** Alta para correcciones, Media para optimizaciones
