using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registrar nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request);

                if (user == null)
                {
                    return BadRequest(ApiResponse<UserResponseDto>.ErrorResponse(
                        "El email ya está registrado"
                    ));
                }

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(
                    user,
                    "Usuario registrado exitosamente"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse(
                    "Error al registrar usuario",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Iniciar sesión
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);

                if (response == null)
                {
                    return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Email o contraseña incorrectos"
                    ));
                }

                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Inicio de sesión exitoso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión");
                return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse(
                    "Error al iniciar sesión",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Cambiar contraseña (requiere autenticación)
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("Usuario no autenticado"));
                }

                var userId = int.Parse(userIdClaim.Value);
                var result = await _authService.ChangePasswordAsync(userId, request);

                if (!result)
                {
                    return BadRequest(ApiResponse.ErrorResponse(
                        "La contraseña actual es incorrecta"
                    ));
                }

                return Ok(ApiResponse.SuccessResponse("Contraseña actualizada exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "Error al cambiar contraseña",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener perfil del usuario autenticado
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(ApiResponse<UserResponseDto>.ErrorResponse(
                        "Usuario no autenticado"
                    ));
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(ApiResponse<UserResponseDto>.ErrorResponse(
                        "Usuario no encontrado"
                    ));
                }

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil");
                return StatusCode(500, ApiResponse<UserResponseDto>.ErrorResponse(
                    "Error al obtener perfil",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Verificar si el token es válido (solo para testing)
        /// </summary>
        [Authorize]
        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                userId,
                email,
                name,
                roles
            }, "Token válido"));
        }
    }
}