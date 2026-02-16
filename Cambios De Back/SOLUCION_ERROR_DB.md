# 🚨 SOLUCIÓN: Error al Registrar Usuario (DbUpdateException)

## 🔍 DIAGNÓSTICO

### Error en Terminal:
```
Microsoft.EntityFrameworkCore.DbUpdateException: 
An error occurred while saving the entity changes. 
See the inner exception for details.
```

### Causa Probable:
❌ **La base de datos NO ha sido creada o actualizada** con las migraciones que incluyen los cambios de:
- CarritoItem.ReservaHasta (campo nuevo)
- User con campos de bloqueo (IntentosLoginFallidos, etc.)
- Eliminación de tablas de Ingredientes, PersonalizadoConfig, MetodoPagoUsuario

---

## ✅ SOLUCIÓN: Crear Base de Datos con Migraciones

### PASO 1: Verificar Estado Actual

```bash
# Ver si existe la base de datos
dotnet ef database list --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

---

### PASO 2: Eliminar Base de Datos Anterior (si existe)

```bash
# Eliminar base de datos
dotnet ef database drop --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API --force
```

**Resultado esperado:**
```
Dropping database 'PastisserieDB' on server '(localdb)\mssqllocaldb'.
Successfully dropped database 'PastisserieDB'.
```

---

### PASO 3: Eliminar Migraciones Antiguas

**En Windows:**
```bash
rmdir /s PastisserieAPI.Infrastructure\Migrations
```

**En Linux/Mac:**
```bash
rm -rf PastisserieAPI.Infrastructure/Migrations
```

**O manualmente:**
- Navegar a la carpeta `PastisserieAPI.Infrastructure/Migrations`
- Eliminar toda la carpeta `Migrations`

---

### PASO 4: Crear Nueva Migración

```bash
dotnet ef migrations add MigracionInicial --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

**Resultado esperado:**
```
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
```

**Esto creará:**
- `Migrations/[Timestamp]_MigracionInicial.cs`
- `Migrations/[Timestamp]_MigracionInicial.Designer.cs`
- `Migrations/ApplicationDbContextModelSnapshot.cs`

---

### PASO 5: Aplicar Migración (Crear Base de Datos)

```bash
dotnet ef database update --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

**Resultado esperado:**
```
Build started...
Build succeeded.
Applying migration '20260216000000_MigracionInicial'.
Done.
```

---

### PASO 6: Verificar Que Se Creó Correctamente

**Opción A: SQL Server Management Studio (SSMS)**
1. Abrir SSMS
2. Conectar a `(localdb)\mssqllocaldb`
3. Ver base de datos `PastisserieDB`
4. Expandir `Tables`
5. Verificar que existan las tablas:
   - Users
   - Roles
   - UserRoles
   - Productos
   - CategoriasProducto
   - CarritosCompra
   - CarritoItems (con columna ReservaHasta)
   - Pedidos
   - etc.

**Opción B: Comando CLI**
```bash
dotnet ef database list --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

---

### PASO 7: Verificar Tablas Creadas

```bash
# Ver las migraciones aplicadas
dotnet ef migrations list --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API
```

**Resultado esperado:**
```
MigracionInicial (Applied)
```

---

### PASO 8: Crear Usuario Administrador

Una vez que la base de datos esté creada, necesitas crear un usuario administrador.

**Ubicación del script**: `PastisserieAPI.API/Database/Scripts/01_CreateAdminUser.sql`

**Ejecutar el script SQL:**

```sql
-- 1. Crear el usuario admin
INSERT INTO Users (Nombre, Email, PasswordHash, Telefono, EmailVerificado, Activo, FechaRegistro, FechaCreacion, IntentosLoginFallidos, CuentaBloqueada)
VALUES (
    'Administrador',
    'admin@pastisserie.com',
    '$2a$11$YourHashedPasswordHere', -- Generar con BCrypt
    NULL,
    1, -- Email verificado
    1, -- Activo
    GETDATE(),
    GETDATE(),
    0, -- Sin intentos fallidos
    0  -- No bloqueado
);

-- 2. Obtener el ID del usuario recién creado
DECLARE @UserId INT = SCOPE_IDENTITY();

-- 3. Asignar rol de Admin (RolId = 2)
INSERT INTO UserRoles (UsuarioId, RolId)
VALUES (@UserId, 2);
```

**💡 Para generar el hash de la contraseña:**

Usa el programa en: `PastisserieAPI.API/Database/Scripts/BCryptHashGenerator.cs`

O genera el hash en: https://bcrypt-generator.com/
- Contraseña: `Admin123!`
- Rounds: 11

---

## 🧪 PROBAR REGISTRO DESPUÉS DE CREAR BD

### Test 1: Registrar Usuario

```json
POST /api/Auth/register
{
  "nombre": "Juan Pérez",
  "email": "juan@example.com",
  "password": "Pass123!",
  "telefono": "3001234567"
}
```

**Resultado esperado:**
```json
{
  "success": true,
  "message": "Usuario registrado exitosamente",
  "data": {
    "id": 2,
    "nombre": "Juan Pérez",
    "email": "juan@example.com",
    "roles": ["Usuario"]
  }
}
```

---

### Test 2: Login

```json
POST /api/Auth/login
{
  "email": "juan@example.com",
  "password": "Pass123!"
}
```

**Resultado esperado:**
```json
{
  "success": true,
  "message": "Inicio de sesión exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": 2,
      "nombre": "Juan Pérez",
      "email": "juan@example.com",
      "roles": ["Usuario"]
    }
  }
}
```

---

## 🔍 SI PERSISTE EL ERROR

### Verificar ConnectionString

**Ubicación**: `PastisserieAPI.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PastisserieDB;Trusted_Connection=true;TrustServerCertificate=true"
  }
}
```

**Verificar:**
- ✅ Server está correcto: `(localdb)\\mssqllocaldb`
- ✅ Database name: `PastisserieDB`
- ✅ Trusted_Connection: `true`

---

### Verificar SQL Server LocalDB

```bash
# Ver instancias de LocalDB
sqllocaldb info

# Si no existe, crear una
sqllocaldb create mssqllocaldb

# Iniciar la instancia
sqllocaldb start mssqllocaldb
```

---

### Ver Log Completo del Error

En la terminal donde ejecutas `dotnet run`, busca el **inner exception** completo.

El error completo dirá algo como:

```
Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred...
---> Microsoft.Data.SqlClient.SqlException (0x80131904): 
Invalid column name 'ReservaHasta'.
```

Esto confirmaría que la base de datos no tiene la columna nueva.

---

## 📋 CHECKLIST DE VERIFICACIÓN

```
[ ] Base de datos antigua eliminada
[ ] Carpeta Migrations eliminada
[ ] Nueva migración creada (dotnet ef migrations add)
[ ] Migración aplicada (dotnet ef database update)
[ ] Base de datos existe en SSMS
[ ] Tablas existen y tienen las columnas correctas
[ ] CarritoItems tiene columna ReservaHasta
[ ] Users tiene columnas IntentosLoginFallidos, CuentaBloqueada
[ ] Usuario admin creado
[ ] dotnet run ejecuta sin errores
[ ] POST /api/Auth/register funciona
[ ] POST /api/Auth/login funciona
```

---

## 🎯 COMANDO COMPLETO (COPIAR Y PEGAR)

```bash
# 1. Detener la aplicación (Ctrl+C)

# 2. Eliminar base de datos
dotnet ef database drop --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API --force

# 3. Eliminar migraciones (en carpeta raíz del proyecto)
# Windows:
rmdir /s PastisserieAPI.Infrastructure\Migrations
# Linux/Mac:
# rm -rf PastisserieAPI.Infrastructure/Migrations

# 4. Crear nueva migración
dotnet ef migrations add MigracionInicial --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API

# 5. Aplicar migración
dotnet ef database update --project PastisserieAPI.Infrastructure --startup-project PastisserieAPI.API

# 6. Compilar
dotnet build

# 7. Ejecutar
dotnet run --project PastisserieAPI.API

# 8. Probar en Swagger
# POST /api/Auth/register
```

---

## 📝 NOTAS IMPORTANTES

### ¿Por qué pasa esto?
- Modificaste entidades (agregaste ReservaHasta, campos de bloqueo)
- Modificaste configuraciones (Fluent API)
- Eliminaste entidades (PersonalizadoConfig, Ingredientes)
- La base de datos antigua NO tiene estos cambios
- Entity Framework no puede guardar en una estructura desactualizada

### ¿Cuándo crear migraciones?
Cada vez que:
- ✅ Agregas una nueva entidad
- ✅ Modificas una entidad existente (nuevas propiedades)
- ✅ Cambias tipos de datos
- ✅ Modificas relaciones
- ✅ Cambias configuraciones Fluent API

### ¿Cuándo NO es necesario migrar?
- ❌ Cambios en Controllers
- ❌ Cambios en Services
- ❌ Cambios en DTOs
- ❌ Cambios en Validators
- ❌ Cambios en configuración (appsettings.json)

---

**Estado**: Solución para DbUpdateException  
**Acción requerida**: Ejecutar comandos del PASO 2 (Crear base de datos)  
**Prioridad**: ALTA - Bloqueante para continuar
