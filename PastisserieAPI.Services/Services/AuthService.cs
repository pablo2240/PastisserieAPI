// ===================================================================
// ARCHIVO: PastisserieAPI.Services/Services/AuthService.cs
// ===================================================================
using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Helpers;
using PastisserieAPI.Services.Services.Interfaces;
using BCrypt.Net;

namespace PastisserieAPI.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly JwtHelper _jwtHelper;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            // Buscar usuario por email con roles
            var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(request.Email);

            if (user == null || !user.Activo)
                return null;

            // Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            // Obtener roles
            var roles = user.UserRoles.Select(ur => ur.Rol.Nombre).ToList();

            // Generar token
            var token = _jwtHelper.GenerateToken(user, roles);

            // Actualizar último acceso
            user.UltimoAcceso = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = _mapper.Map<UserResponseDto>(user);

            return new LoginResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(1),
                User = userDto
            };
        }

        public async Task<UserResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            // Verificar si el email ya existe
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
                return null;

            // Crear usuario
            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Asignar rol por defecto "Usuario"
            var rolUsuario = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Nombre == "Usuario");

            if (rolUsuario != null)
            {
                var userRol = new UserRol
                {
                    UsuarioId = user.Id,
                    RolId = rolUsuario.Id,
                    FechaAsignacion = DateTime.UtcNow
                };

                // CORRECCIÓN: Agregar el UserRol al repositorio
                await _unitOfWork.UserRoles.AddAsync(userRol);
                await _unitOfWork.SaveChangesAsync();
            }

            // Crear carrito para el usuario
            var carrito = new CarritoCompra
            {
                UsuarioId = user.Id,
                FechaCreacion = DateTime.UtcNow
            };

            await _unitOfWork.Carritos.AddAsync(carrito);
            await _unitOfWork.SaveChangesAsync();

            // Obtener usuario con roles
            var userWithRoles = await _unitOfWork.Users.GetByEmailWithRolesAsync(user.Email);
            return _mapper.Map<UserResponseDto>(userWithRoles);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                return false;

            // Verificar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return false;

            // Actualizar contraseña
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(email);
            return user == null ? null : _mapper.Map<UserResponseDto>(user);
        }
    }
}