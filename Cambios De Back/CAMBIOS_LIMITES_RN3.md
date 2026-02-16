# 🔄 CAMBIOS EN LÍMITES DE PRODUCTOS (RN3)

## 📝 AJUSTES REALIZADOS

### ❌ ANTES:
```
RN3: Máximo 20 unidades por producto
Máximo 50 productos diferentes por pedido
```

### ✅ DESPUÉS:
```
RN3: Máximo 10 unidades por producto
Máximo 30 productos diferentes por pedido
```

---

## 📂 ARCHIVOS A ACTUALIZAR (2)

### ARCHIVO 1: CreatePedidoRequestValidator.cs ✅
**Ubicación**: `PastisserieAPI.Services/Validators/CreatePedidoRequestValidator.cs`

**Cambios:**
```csharp
// ❌ ANTES: Máximo 50 productos
.Must(items => items.Count <= 50)
.WithMessage("No puede agregar más de 50 productos diferentes al pedido");

// ✅ DESPUÉS: Máximo 30 productos
.Must(items => items.Count <= 30)
.WithMessage("No puede agregar más de 30 productos diferentes al pedido");

// ❌ ANTES: Máximo 20 unidades
.LessThanOrEqualTo(20)
.WithMessage("No puede agregar más de 20 unidades por producto (RN3)");

// ✅ DESPUÉS: Máximo 10 unidades
.LessThanOrEqualTo(10)
.WithMessage("No puede agregar más de 10 unidades por producto (RN3)");
```

---

### ARCHIVO 2: CarritoService.cs ✅
**Ubicación**: `PastisserieAPI.Services/Services/CarritoService.cs`

**Cambios en el método `AddItemAsync`:**

```csharp
// ❌ ANTES: Línea ~73
if (request.Cantidad > 20)
    throw new Exception("No puedes agregar más de 20 unidades por producto");

// ✅ DESPUÉS:
if (request.Cantidad > 10)
    throw new Exception("No puedes agregar más de 10 unidades por producto");

// ❌ ANTES: Línea ~84
if (nuevaCantidad > 20)
    throw new Exception("No puedes tener más de 20 unidades de este producto en tu carrito");

// ✅ DESPUÉS:
if (nuevaCantidad > 10)
    throw new Exception("No puedes tener más de 10 unidades de este producto en tu carrito");
```

**Cambios en el método `UpdateItemAsync`:**

```csharp
// ❌ ANTES: Línea ~145
if (request.Cantidad > 20)
    throw new Exception("No puedes tener más de 20 unidades de este producto");

// ✅ DESPUÉS:
if (request.Cantidad > 10)
    throw new Exception("No puedes tener más de 10 unidades de este producto");
```

---

## 🔧 INSTRUCCIONES DE ACTUALIZACIÓN

### PASO 1: Actualizar CreatePedidoRequestValidator.cs

1. Abre `PastisserieAPI.Services/Validators/CreatePedidoRequestValidator.cs`
2. **OPCIÓN A - Reemplazar todo:**
   - Borra todo el contenido
   - Copia el código del archivo `CreatePedidoRequestValidator_Actualizado.cs`
   - Guarda

3. **OPCIÓN B - Cambiar manualmente:**
   - Buscar `.Must(items => items.Count <= 50)` → Cambiar a `30`
   - Buscar `.LessThanOrEqualTo(20)` → Cambiar a `10`
   - Buscar los mensajes de error y ajustarlos
   - Guarda

---

### PASO 2: Actualizar CarritoService.cs

1. Abre `PastisserieAPI.Services/Services/CarritoService.cs`

2. **Buscar y reemplazar (3 ocurrencias):**

**Primera ocurrencia (línea ~73):**
```csharp
// BUSCAR:
if (request.Cantidad > 20)
    throw new Exception("No puedes agregar más de 20 unidades por producto");

// REEMPLAZAR POR:
if (request.Cantidad > 10)
    throw new Exception("No puedes agregar más de 10 unidades por producto");
```

**Segunda ocurrencia (línea ~84):**
```csharp
// BUSCAR:
if (nuevaCantidad > 20)
    throw new Exception("No puedes tener más de 20 unidades de este producto en tu carrito");

// REEMPLAZAR POR:
if (nuevaCantidad > 10)
    throw new Exception("No puedes tener más de 10 unidades de este producto en tu carrito");
```

**Tercera ocurrencia (línea ~145):**
```csharp
// BUSCAR:
if (request.Cantidad > 20)
    throw new Exception("No puedes tener más de 20 unidades de este producto");

// REEMPLAZAR POR:
if (request.Cantidad > 10)
    throw new Exception("No puedes tener más de 10 unidades de este producto");
```

3. Guarda el archivo

---

## ✅ VERIFICACIÓN

### Compilar:
```bash
dotnet build
```

Debe compilar sin errores.

### Probar en Swagger:

**Test 1: Agregar 11 unidades al carrito (DEBE FALLAR)**
```json
POST /api/Carrito/items
{
  "productoId": 1,
  "cantidad": 11
}

❌ Error: "No puedes agregar más de 10 unidades por producto"
```

**Test 2: Crear pedido con 11 unidades (DEBE FALLAR)**
```json
POST /api/Pedidos
{
  "usuarioId": 1,
  "direccionEnvioId": 1,
  "items": [
    { "productoId": 1, "cantidad": 11 }
  ]
}

❌ Error: "No puede agregar más de 10 unidades por producto (RN3)"
```

**Test 3: Agregar 10 unidades (DEBE PASAR)**
```json
POST /api/Carrito/items
{
  "productoId": 1,
  "cantidad": 10
}

✅ Item agregado correctamente
```

**Test 4: Crear pedido con 31 productos diferentes (DEBE FALLAR)**
```json
POST /api/Pedidos
{
  "usuarioId": 1,
  "direccionEnvioId": 1,
  "items": [
    { "productoId": 1, "cantidad": 1 },
    { "productoId": 2, "cantidad": 1 },
    ...
    { "productoId": 31, "cantidad": 1 }
  ]
}

❌ Error: "No puede agregar más de 30 productos diferentes al pedido"
```

---

## 📊 RESUMEN DE CAMBIOS

| Validación | ANTES | DESPUÉS |
|------------|-------|---------|
| Unidades por producto | 20 | **10** |
| Productos diferentes | 50 | **30** |

### Archivos Modificados:
- ✅ CreatePedidoRequestValidator.cs (2 cambios)
- ✅ CarritoService.cs (3 cambios)

### Total de cambios: 5 líneas

---

## 📝 NOTAS

### ¿Por qué estos límites?
- **10 unidades**: Evita acaparamiento, permite mejor distribución de inventario
- **30 productos**: Límite razonable para un pedido de pastelería

### Impacto en el usuario:
- ✅ Puede seguir haciendo pedidos normales
- ✅ Puede comprar múltiples productos diferentes
- ⚠️ No puede comprar más de 10 unidades del mismo producto
- ⚠️ No puede agregar más de 30 productos diferentes en un solo pedido

---

**Fecha**: Febrero 16, 2026  
**Cambios**: RN3 ajustado de 20→10 unidades, límite de 50→30 productos  
**Estado**: Listo para aplicar
