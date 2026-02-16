# 🛒 PASO 5: IMPLEMENTAR F3 - CARRITO CON VALIDACIONES
## Reglas de Negocio: RN1, RN2, RN3 y Funcionalidad F3

---

## 📋 QUÉ VAMOS A IMPLEMENTAR

### ✅ F3 - Carrito de Compras:
- **No agregar más del stock** (RN1)
- **Aumentar cantidad si ya existe** (en lugar de duplicar)
- **Recálculo automático** de totales

### ✅ RN1 - Stock:
- No se puede vender un producto si el stock es 0
- Validación en frontend Y backend

### ✅ RN2 - Reserva de Productos:
- Los productos en carrito se reservan por **10 minutos**
- Durante ese tiempo, el stock se reserva
- Si no se compra en ese plazo, el stock se libera automáticamente

### ✅ RN3 - Límite por Cliente:
- Un cliente no puede comprar más de **20 unidades** del mismo producto por pedido

---

## 🔧 ARCHIVOS A MODIFICAR/CREAR

### Modificar (1):
1. ✅ `PastisserieAPI.Services/Services/CarritoService.cs`

### Crear (1):
2. ✅ `PastisserieAPI.Services/Services/ReservaStockService.cs`

### Configurar (1):
3. ✅ `PastisserieAPI.API/Program.cs`

---

## 📝 INSTRUCCIONES PASO A PASO

### PARTE 1: ACTUALIZAR CarritoService

**Ubicación**: `PastisserieAPI.Services/Services/CarritoService.cs`

#### PASO 1.1 - Reemplazar TODO el archivo

1. Abre el archivo `CarritoService.cs`
2. **BORRA TODO el contenido**
3. Copia el contenido del archivo `CarritoService_Mejorado.cs` que te proporcioné
4. Guarda el archivo

#### ✅ Cambios Implementados:

```csharp
// ========== NUEVAS VALIDACIONES ==========

// RN1: No vender si stock = 0
if (producto.Stock <= 0)
    throw new Exception("Producto sin stock disponible");

if (request.Cantidad > producto.Stock)
    throw new Exception($"Solo hay {producto.Stock} unidades disponibles");

// RN3: Límite 20 unidades
if (request.Cantidad > 20)
    throw new Exception("No puedes agregar más de 20 unidades por producto");

// F3: Aumentar cantidad si ya existe
var itemExistente = carrito!.Items
    .FirstOrDefault(i => i.ProductoId == request.ProductoId);

if (itemExistente != null)
{
    int nuevaCantidad = itemExistente.Cantidad + request.Cantidad;
    
    // Validar stock total
    if (nuevaCantidad > producto.Stock)
        throw new Exception($"Solo hay {producto.Stock} unidades disponibles en total");
    
    // Validar límite 20 total
    if (nuevaCantidad > 20)
        throw new Exception("No puedes tener más de 20 unidades de este producto");
    
    itemExistente.Cantidad = nuevaCantidad;
    
    // RN2: Renovar reserva
    itemExistente.ReservaHasta = DateTime.UtcNow.AddMinutes(10);
}
else
{
    // RN2: Establecer reserva en nuevo item
    var nuevoItem = new CarritoItem
    {
        CarritoId = carrito.Id,
        ProductoId = request.ProductoId,
        Cantidad = request.Cantidad,
        FechaAgregado = DateTime.UtcNow,
        ReservaHasta = DateTime.UtcNow.AddMinutes(10)  // ← 10 minutos
    };
}
```

#### ✅ Nuevo Método Auxiliar:

```csharp
// Libera items expirados cada vez que el usuario consulta su carrito
private async Task LiberarItemsExpiradosUsuarioAsync(int usuarioId)
{
    var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
    
    if (carrito == null || !carrito.Items.Any())
        return;
    
    var itemsExpirados = carrito.Items
        .Where(i => i.ReservaHasta.HasValue && 
                    i.ReservaHasta.Value < DateTime.UtcNow)
        .ToList();
    
    if (itemsExpirados.Any())
    {
        foreach (var item in itemsExpirados)
        {
            carrito.Items.Remove(item);
        }
        
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

### PARTE 2: CREAR ReservaStockService (Background Service)

**Ubicación**: `PastisserieAPI.Services/Services/ReservaStockService.cs`

#### PASO 2.1 - Crear archivo nuevo

1. En Visual Studio, click derecho en carpeta `Services`
2. Agregar → Nueva clase
3. Nombre: `ReservaStockService.cs`
4. Copia el contenido del archivo `ReservaStockService.cs` que te proporcioné
5. Guarda el archivo

#### ✅ Qué hace este servicio:

```csharp
// 🔄 Se ejecuta automáticamente cada 2 minutos
// 🧹 Busca items de carrito con reservas expiradas
// 🗑️  Los elimina automáticamente
// 📊 Registra logs detallados

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            await LiberarReservasExpiradasAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al liberar reservas");
        }
        
        // ⏱️ Esperar 2 minutos antes de la siguiente ejecución
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
    }
}
```

#### ✅ Logs que verás en la consola:

```
✅ Servicio de liberación de reservas de stock INICIADO
⏱️  Se ejecutará cada 2 minutos
ℹ️  No hay reservas expiradas en este momento
🧹 Liberando 3 reservas expiradas de stock
   📦 ProductoId: 5 (Torta Red Velvet) - Cantidad: 2 - Expiró: 2026-02-15 15:30:00
   📦 ProductoId: 8 (Pan Integral) - Cantidad: 1 - Expiró: 2026-02-15 15:28:00
   📦 ProductoId: 12 (Galletas) - Cantidad: 3 - Expiró: 2026-02-15 15:25:00
✅ Reservas liberadas exitosamente
```

---

### PARTE 3: REGISTRAR EL BACKGROUND SERVICE

**Ubicación**: `PastisserieAPI.API/Program.cs`

#### PASO 3.1 - Agregar el servicio

**BUSCAR esta sección (alrededor de la línea 50-60):**

```csharp
// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
```

**AGREGAR DESPUÉS:**

```csharp
// ========== BACKGROUND SERVICE PARA LIBERAR RESERVAS (RN2) ==========
builder.Services.AddHostedService<ReservaStockService>();
```

#### ✅ Resultado Final:

```csharp
// Servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// ========== BACKGROUND SERVICE PARA LIBERAR RESERVAS (RN2) ==========
builder.Services.AddHostedService<ReservaStockService>();
```

**IMPORTANTE**: Asegúrate de agregar el `using` al inicio del archivo:

```csharp
using PastisserieAPI.Services.Services;
```

---

## ✅ VERIFICACIÓN

### PASO FINAL: Compilar y Ejecutar

```bash
# 1. Compilar
dotnet build

# 2. Ejecutar
dotnet run --project PastisserieAPI.API
```

### ✅ Verificar que todo funciona:

#### 1. **Ver logs del Background Service**

Al iniciar la aplicación, deberías ver:

```
info: PastisserieAPI.Services.Services.ReservaStockService[0]
      ✅ Servicio de liberación de reservas de stock INICIADO
info: PastisserieAPI.Services.Services.ReservaStockService[0]
      ⏱️  Se ejecutará cada 2 minutos
```

#### 2. **Probar en Swagger**

**Escenario de prueba:**

```
1. POST /api/Carrito/items
   Body: { "productoId": 1, "cantidad": 2 }
   ✅ Debería agregarse con ReservaHasta = ahora + 10 min

2. POST /api/Carrito/items  (mismo producto)
   Body: { "productoId": 1, "cantidad": 3 }
   ✅ Debería incrementar cantidad a 5 (no duplicar)
   ✅ ReservaHasta se renueva

3. POST /api/Carrito/items
   Body: { "productoId": 1, "cantidad": 25 }
   ❌ Error: "No puedes agregar más de 20 unidades"

4. POST /api/Carrito/items
   Body: { "productoId": 999, "cantidad": 1 }  (producto sin stock)
   ❌ Error: "Producto sin stock disponible"

5. Esperar 10 minutos + 2 minutos (del background service)
   GET /api/Carrito
   ✅ Items expirados deberían haberse eliminado automáticamente
```

#### 3. **Verificar Logs en Tiempo Real**

Cada 2 minutos verás:

```
info: PastisserieAPI.Services.Services.ReservaStockService[0]
      ℹ️  No hay reservas expiradas en este momento
```

O si hay items expirados:

```
warn: PastisserieAPI.Services.Services.ReservaStockService[0]
      🧹 Liberando 2 reservas expiradas de stock
info: PastisserieAPI.Services.Services.ReservaStockService[0]
         📦 ProductoId: 1 (Producto X) - Cantidad: 2 - Expiró: 2026-02-15 15:30:00
info: PastisserieAPI.Services.Services.ReservaStockService[0]
      ✅ Reservas liberadas exitosamente
```

---

## 📊 RESUMEN DE VALIDACIONES IMPLEMENTADAS

### En AddItemAsync:

| Validación | Código | Mensaje de Error |
|------------|--------|------------------|
| **RN1: Stock = 0** | `if (producto.Stock <= 0)` | "Producto sin stock disponible" |
| **RN1: Cantidad > Stock** | `if (request.Cantidad > producto.Stock)` | "Solo hay X unidades disponibles" |
| **RN3: Límite 20** | `if (request.Cantidad > 20)` | "No puedes agregar más de 20 unidades" |
| **F3: Stock total** | `if (nuevaCantidad > producto.Stock)` | "Solo hay X unidades disponibles en total" |
| **F3: Límite 20 total** | `if (nuevaCantidad > 20)` | "No puedes tener más de 20 unidades" |
| **RN2: Reserva** | `ReservaHasta = DateTime.UtcNow.AddMinutes(10)` | - |

### En UpdateItemAsync:

| Validación | Código | Mensaje de Error |
|------------|--------|------------------|
| **RN1: Stock = 0** | `if (producto.Stock <= 0)` | "Producto sin stock disponible" |
| **RN1: Cantidad > Stock** | `if (request.Cantidad > producto.Stock)` | "Solo hay X unidades disponibles" |
| **RN3: Límite 20** | `if (request.Cantidad > 20)` | "No puedes tener más de 20 unidades" |
| **RN2: Renovar** | `ReservaHasta = DateTime.UtcNow.AddMinutes(10)` | - |

### Background Service:

| Acción | Frecuencia | Qué hace |
|--------|-----------|----------|
| **Liberar reservas** | Cada 2 minutos | Elimina items con `ReservaHasta < ahora` |
| **Logging** | Cada ejecución | Registra items eliminados |

---

## 🎯 CHECKLIST DE VERIFICACIÓN

```
[ ] CarritoService.cs actualizado con validaciones
[ ] ReservaStockService.cs creado
[ ] Program.cs actualizado con AddHostedService
[ ] dotnet build sin errores
[ ] dotnet run ejecuta correctamente
[ ] Se ven logs del background service
[ ] Validación RN1 funciona (no vender sin stock)
[ ] Validación RN3 funciona (máximo 20 unidades)
[ ] F3 funciona (aumenta cantidad si existe)
[ ] RN2 funciona (reserva por 10 minutos)
[ ] Background service libera items expirados
```

---

## 🚨 TROUBLESHOOTING

### Error: "CarritoItem no tiene propiedad ReservaHasta"
**Solución**: Asegúrate de haber ejecutado el PASO 2 completo (crear migración y aplicarla)

### Error: "ReservaStockService no se encuentra"
**Solución**: Verifica que el archivo esté en `PastisserieAPI.Services/Services/` y que hayas agregado el `using` en Program.cs

### Background service no aparece en logs
**Solución**: Verifica que agregaste `builder.Services.AddHostedService<ReservaStockService>();` en Program.cs

### Items no se eliminan automáticamente
**Solución**: 
1. Verifica que ReservaHasta esté configurado correctamente
2. Espera al menos 12 minutos (10 de reserva + 2 del intervalo)
3. Revisa los logs para ver si hay errores

---

## 📈 PRÓXIMO PASO

Una vez que esto compile y funcione correctamente, pasaremos al **PASO 6: Implementar F5 - Validaciones de Checkout**.

---

**Estado**: PASO 5 - Implementación F3 y Reglas RN1, RN2, RN3  
**Tiempo estimado**: 15-20 minutos  
**Archivos**: 3 (1 modificado, 1 creado, 1 configurado)
