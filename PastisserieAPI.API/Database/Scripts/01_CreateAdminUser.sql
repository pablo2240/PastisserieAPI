USE PastisserieDB;
GO

-- CREAR USUARIO ADMINISTRADOR CON HASH PROPORCIONADO
-- ==================================================

-- Tu hash BCrypt
DECLARE @PasswordHash NVARCHAR(500) = '$2a$15$Bl/IVp3jrpDYQlwfuFn9XO2ma1diW926HzpPc51cqcmJvhuzaRqSm';

-- La contraseña del hash
DECLARE @PasswordOriginal NVARCHAR(50) = 'Admin7342M!'; 

PRINT '═════════════════════════════════';
PRINT ' CREANDO USUARIO ADMINISTRADOR';


-- Verificar que no existe
IF EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@pastisserie.com')
BEGIN
    PRINT ' ERROR: El usuario administrador ya existe';
    PRINT '   Ejecuta primero el script de limpieza';
    RETURN;
END

-- Insertar usuario administrador
INSERT INTO Users (
    Nombre, 
    Email, 
    PasswordHash, 
    Telefono, 
    EmailVerificado, 
    FechaRegistro, 
    UltimoAcceso, 
    Activo, 
    FechaCreacion, 
    FechaActualizacion
)
VALUES (
    'Administrador Principal',
    'admin@pastisserie.com',
    @PasswordHash,
    '3001234567',
    1, -- Email verificado
    GETUTCDATE(),
    NULL,
    1, -- Activo
    GETUTCDATE(),
    NULL
);

-- Obtener ID del usuario recién creado
DECLARE @AdminUserId INT = SCOPE_IDENTITY();

PRINT 'Usuario administrador creado con ID: ' + CAST(@AdminUserId AS NVARCHAR);

-- Obtener ID del rol Admin
DECLARE @RolAdminId INT = (SELECT Id FROM Roles WHERE Nombre = 'Admin');

IF @RolAdminId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el rol "Admin"';
    PRINT '   Ejecuta primero las migraciones de Entity Framework';
    RETURN;
END

-- Asignar rol Admin al usuario
INSERT INTO UserRoles (UsuarioId, RolId, FechaAsignacion)
VALUES (@AdminUserId, @RolAdminId, GETUTCDATE());

PRINT ' Rol Admin asignado correctamente';

-- Crear carrito para el administrador
INSERT INTO CarritosCompra (UsuarioId, FechaCreacion, FechaActualizacion)
VALUES (@AdminUserId, GETUTCDATE(), NULL);

PRINT ' Carrito creado para el administrador';

-- Verificar la creación
PRINT '';
PRINT ' ADMINISTRADOR CREADO EXITOSAMENTE';
PRINT '════════════════════════════════════';
PRINT '';

SELECT 
    u.Id AS 'ID Usuario',
    u.Nombre AS 'Nombre',
    u.Email AS 'Email',
    u.Telefono AS 'Teléfono',
    r.Nombre AS 'Rol',
    u.Activo AS 'Activo',
    u.EmailVerificado AS 'Email Verificado',
    u.FechaRegistro AS 'Fecha Registro'
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UsuarioId
INNER JOIN Roles r ON ur.RolId = r.Id
WHERE u.Email = 'admin@pastisserie.com';

PRINT '';
PRINT ' Email:    admin@pastisserie.com';
PRINT ' Password: ' + @PasswordOriginal;
PRINT '';
PRINT '  GUARDA ESTAS CREDENCIALES EN UN LUGAR SEGURO';

GO