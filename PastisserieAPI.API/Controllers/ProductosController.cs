using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.DTOs.Common;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(
            IProductoService productoService,
            ILogger<ProductosController> logger)
        {
            _productoService = productoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los productos con paginación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination)
        {
            try
            {
                var result = await _productoService.GetAllAsync(pagination);

                return Ok(ApiResponse<PagedResult<ProductoResponseDto>>.SuccessResponse(
                    result,
                    $"Página {result.PageNumber} de {result.TotalPages}"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, ApiResponse<PagedResult<ProductoResponseDto>>.ErrorResponse(
                    "Error al obtener productos",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener producto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var producto = await _productoService.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<ProductoResponseDto>.ErrorResponse(
                        $"Producto con ID {id} no encontrado"
                    ));
                }

                return Ok(ApiResponse<ProductoResponseDto>.SuccessResponse(producto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {ProductoId}", id);
                return StatusCode(500, ApiResponse<ProductoResponseDto>.ErrorResponse(
                    "Error al obtener producto",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener productos por categoría
        /// </summary>
        [HttpGet("categoria/{categoriaId}")]
        public async Task<IActionResult> GetByCategoria(int categoriaId)
        {
            try
            {
                var productos = await _productoService.GetByCategoriaIdAsync(categoriaId);

                return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
                    productos,
                    $"Se encontraron {productos.Count} productos en esta categoría"
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

        /// <summary>
        /// Obtener productos activos
        /// </summary>
        [HttpGet("activos")]
        public async Task<IActionResult> GetActivos()
        {
            try
            {
                var productos = await _productoService.GetActivosAsync();

                return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
                    productos,
                    $"Se encontraron {productos.Count} productos activos"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos activos");
                return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
                    "Error al obtener productos activos",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Obtener productos con bajo stock (solo Admin)
        /// </summary>
        [Authorize(Roles = "Admin,Gerente")]
        [HttpGet("bajo-stock")]
        public async Task<IActionResult> GetBajoStock()
        {
            try
            {
                var productos = await _productoService.GetProductosBajoStockAsync();

                return Ok(ApiResponse<List<ProductoResponseDto>>.SuccessResponse(
                    productos,
                    $"Se encontraron {productos.Count} productos con bajo stock"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con bajo stock");
                return StatusCode(500, ApiResponse<List<ProductoResponseDto>>.ErrorResponse(
                    "Error al obtener productos",
                    new List<string> { ex.Message }
                ));
            }
        }

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

        /// <summary>
        /// Crear producto (solo Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductoRequestDto request)
        {
            try
            {
                var producto = await _productoService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = producto.Id },
                    ApiResponse<ProductoResponseDto>.SuccessResponse(
                        producto,
                        "Producto creado exitosamente"
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                return StatusCode(500, ApiResponse<ProductoResponseDto>.ErrorResponse(
                    "Error al crear producto",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Actualizar producto (solo Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequestDto request)
        {
            try
            {
                var producto = await _productoService.UpdateAsync(id, request);

                if (producto == null)
                {
                    return NotFound(ApiResponse<ProductoResponseDto>.ErrorResponse(
                        $"Producto con ID {id} no encontrado"
                    ));
                }

                return Ok(ApiResponse<ProductoResponseDto>.SuccessResponse(
                    producto,
                    "Producto actualizado exitosamente"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {ProductoId}", id);
                return StatusCode(500, ApiResponse<ProductoResponseDto>.ErrorResponse(
                    "Error al actualizar producto",
                    new List<string> { ex.Message }
                ));
            }
        }

        /// <summary>
        /// Eliminar (desactivar) producto (solo Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productoService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse(
                        $"Producto con ID {id} no encontrado"
                    ));
                }

                return Ok(ApiResponse.SuccessResponse("Producto desactivado exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {ProductoId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "Error al eliminar producto",
                    new List<string> { ex.Message }
                ));
            }
        }
    }
}