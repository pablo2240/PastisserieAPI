using AutoMapper;
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

        public CarritoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CarritoResponseDto?> GetByUsuarioIdAsync(int usuarioId)
        {
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

            // Verificar que el producto exista
            var producto = await _unitOfWork.Productos.GetByIdAsync(request.ProductoId);

            if (producto == null || !producto.Activo)
                throw new Exception("Producto no encontrado o inactivo");

            if (producto.Stock < request.Cantidad)
                throw new Exception($"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles");

            // Verificar si el producto ya está en el carrito
            var itemExistente = carrito!.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);

            if (itemExistente != null)
            {
                // Actualizar cantidad
                itemExistente.Cantidad += request.Cantidad;

                if (producto.Stock < itemExistente.Cantidad)
                    throw new Exception($"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles");
            }
            else
            {
                // Agregar nuevo item
                var nuevoItem = new CarritoItem
                {
                    CarritoId = carrito.Id,
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad,
                    FechaAgregado = DateTime.UtcNow
                };

                carrito.Items.Add(nuevoItem);
            }

            carrito.FechaActualizacion = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            // Recargar carrito con items actualizados
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

            // Verificar stock
            var producto = await _unitOfWork.Productos.GetByIdAsync(item.ProductoId);

            if (producto == null)
                throw new Exception("Producto no encontrado");

            if (producto.Stock < request.Cantidad)
                throw new Exception($"Stock insuficiente. Solo hay {producto.Stock} unidades disponibles");

            item.Cantidad = request.Cantidad;
            carrito.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

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

            await _unitOfWork.Carritos.ClearCarritoAsync(carrito.Id);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}