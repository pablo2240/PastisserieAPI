using AutoMapper;
using Microsoft.Extensions.Logging;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CarritoService> _logger;

        public CarritoService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CarritoService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CarritoResponseDto?> GetByUsuarioIdAsync(int usuarioId)
        {
            // Liberar items expirados del usuario antes de retornar el carrito
            await LiberarItemsExpiradosUsuarioAsync(usuarioId);

            var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);

            if (carrito == null)
            {
                // Crear carrito si no existe
                carrito = new CarritoCompra
                {
                    UsuarioId = usuarioId,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Carritos.AddAsync(carrito);
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<CarritoResponseDto>(carrito);
        }

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

                // Recargar con items
                carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
            }

            // Obtener producto
            var producto = await _unitOfWork.Productos.GetByIdAsync(request.ProductoId);

            if (producto == null)
                throw new Exception("Producto no encontrado");

            if (!producto.Activo)
                throw new Exception("El producto no está disponible");

            // ========== RN1: VALIDAR STOCK (Stock = 0 no vender) ==========
            if (producto.Stock <= 0)
                throw new Exception("Producto sin stock disponible");

            if (request.Cantidad > producto.Stock)
                throw new Exception($"Solo hay {producto.Stock} unidades disponibles");

            // ========== RN3: LÍMITE 20 UNIDADES POR PRODUCTO ==========
            if (request.Cantidad > 20)
                throw new Exception("No puedes agregar más de 20 unidades por producto");

            // ========== F3: AUMENTAR CANTIDAD SI YA EXISTE ==========
            var itemExistente = carrito!.Items
                .FirstOrDefault(i => i.ProductoId == request.ProductoId);

            if (itemExistente != null)
            {
                int nuevaCantidad = itemExistente.Cantidad + request.Cantidad;

                // Validar stock para nueva cantidad
                if (nuevaCantidad > producto.Stock)
                    throw new Exception($"Solo hay {producto.Stock} unidades disponibles en total");

                // Validar límite de 20 para la cantidad total
                if (nuevaCantidad > 20)
                    throw new Exception("No puedes tener más de 20 unidades de este producto en tu carrito");

                itemExistente.Cantidad = nuevaCantidad;

                // ========== RN2: RENOVAR RESERVA 10 MINUTOS ==========
                itemExistente.ReservaHasta = DateTime.UtcNow.AddMinutes(10);

                _logger.LogInformation(
                    "Cantidad actualizada para ProductoId {ProductoId}: {Cantidad} unidades. Reserva renovada hasta {ReservaHasta}",
                    request.ProductoId,
                    nuevaCantidad,
                    itemExistente.ReservaHasta
                );
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

                    // ========== RN2: ESTABLECER RESERVA 10 MINUTOS ==========
                    ReservaHasta = DateTime.UtcNow.AddMinutes(10)
                };

                carrito.Items.Add(nuevoItem);

                _logger.LogInformation(
                    "Nuevo item agregado - ProductoId {ProductoId}: {Cantidad} unidades. Reserva hasta {ReservaHasta}",
                    request.ProductoId,
                    request.Cantidad,
                    nuevoItem.ReservaHasta
                );
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // Recargar carrito actualizado
            var carritoActualizado = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
            return _mapper.Map<CarritoResponseDto>(carritoActualizado!);
        }

        public async Task<CarritoResponseDto?> UpdateItemAsync(int usuarioId, int itemId, UpdateCarritoItemRequestDto request)
        {
            var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);

            if (carrito == null)
                return null;

            var item = carrito.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                return null;

            // Verificar stock del producto
            var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);

            if (producto == null)
                throw new Exception("Producto no encontrado");

            // ========== RN1: VALIDAR STOCK ==========
            if (producto.Stock <= 0)
                throw new Exception("Producto sin stock disponible");

            if (request.Cantidad > producto.Stock)
                throw new Exception($"Solo hay {producto.Stock} unidades disponibles");

            // ========== RN3: LÍMITE 20 UNIDADES ==========
            if (request.Cantidad > 20)
                throw new Exception("No puedes tener más de 20 unidades de este producto");

            item.Cantidad = request.Cantidad;

            // ========== RN2: RENOVAR RESERVA AL ACTUALIZAR ==========
            item.ReservaHasta = DateTime.UtcNow.AddMinutes(10);

            carrito.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Item {ItemId} actualizado a {Cantidad} unidades. Reserva renovada hasta {ReservaHasta}",
                itemId,
                request.Cantidad,
                item.ReservaHasta
            );

            var carritoActualizado = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);
            return _mapper.Map<CarritoResponseDto>(carritoActualizado!);
        }

        public async Task<bool> RemoveItemAsync(int usuarioId, int itemId)
        {
            var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);

            if (carrito == null)
                return false;

            var item = carrito.Items.FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                return false;

            _logger.LogInformation(
                "Removiendo item {ItemId} (ProductoId: {ProductoId}, Cantidad: {Cantidad}) del carrito",
                itemId,
                item.ProductoId,
                item.Cantidad
            );

            carrito.Items.Remove(item);
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCarritoAsync(int usuarioId)
        {
            var carrito = await _unitOfWork.Carritos.GetByUsuarioIdAsync(usuarioId);

            if (carrito == null)
                return false;

            _logger.LogInformation("Vaciando carrito del usuario {UsuarioId}", usuarioId);

            await _unitOfWork.Carritos.ClearCarritoAsync(carrito.Id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // ========== MÉTODO AUXILIAR: LIBERAR ITEMS EXPIRADOS DE UN USUARIO ==========
        private async Task LiberarItemsExpiradosUsuarioAsync(int usuarioId)
        {
            try
            {
                var carrito = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(usuarioId);

                if (carrito == null || !carrito.Items.Any())
                    return;

                var itemsExpirados = carrito.Items
                    .Where(i => i.ReservaHasta.HasValue && i.ReservaHasta.Value < DateTime.UtcNow)
                    .ToList();

                if (itemsExpirados.Any())
                {
                    _logger.LogInformation(
                        "Liberando {Count} items expirados del carrito del usuario {UsuarioId}",
                        itemsExpirados.Count,
                        usuarioId
                    );

                    foreach (var item in itemsExpirados)
                    {
                        carrito.Items.Remove(item);
                    }

                    carrito.FechaActualizacion = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar items expirados del usuario {UsuarioId}", usuarioId);
            }
        }
    }
}