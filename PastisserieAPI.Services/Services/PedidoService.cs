using AutoMapper;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Request;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PedidoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PedidoResponseDto> CreateAsync(CreatePedidoRequestDto request)
        {
            // Crear pedido base
            var pedido = _mapper.Map<Pedido>(request);

            // Calcular totales y crear items
            decimal subtotal = 0;
            var pedidoItems = new List<PedidoItem>();

            foreach (var itemRequest in request.Items)
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(itemRequest.ProductoId);

                if (producto == null || !producto.Activo)
                    throw new Exception($"Producto con ID {itemRequest.ProductoId} no encontrado o inactivo");

                if (producto.Stock < itemRequest.Cantidad)
                    throw new Exception($"Stock insuficiente para el producto {producto.Nombre}");

                var pedidoItem = new PedidoItem
                {
                    ProductoId = itemRequest.ProductoId,
                    Cantidad = itemRequest.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = producto.Precio * itemRequest.Cantidad
                };

                subtotal += pedidoItem.Subtotal;
                pedidoItems.Add(pedidoItem);

                // Reducir stock
                producto.Stock -= itemRequest.Cantidad;
                await _unitOfWork.Productos.UpdateAsync(producto);
            }

            // Calcular total
            pedido.Subtotal = subtotal;
            pedido.CostoEnvio = subtotal >= 50000 ? 0 : 5000; // Envío gratis sobre $50,000
            pedido.Total = pedido.Subtotal + pedido.CostoEnvio;

            // Crear pedido
            await _unitOfWork.Pedidos.AddAsync(pedido);
            await _unitOfWork.SaveChangesAsync();

            // Agregar items al pedido
            foreach (var item in pedidoItems)
            {
                item.PedidoId = pedido.Id;
            }
            await _unitOfWork.SaveChangesAsync();


            // Crear historial
            var historial = new PedidoHistorial
            {
                PedidoId = pedido.Id,
                EstadoAnterior = "",
                EstadoNuevo = "Pendiente",
                FechaCambio = DateTime.UtcNow,
                CambiadoPor = request.UsuarioId,
                Notas = "Pedido creado"
            };

            await _unitOfWork.SaveChangesAsync();

            // Obtener pedido completo
            var pedidoCompleto = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(pedido.Id);
            return _mapper.Map<PedidoResponseDto>(pedidoCompleto!);
        }

        public async Task<PedidoResponseDto?> GetByIdAsync(int id)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(id);
            return pedido == null ? null : _mapper.Map<PedidoResponseDto>(pedido);
        }

        public async Task<List<PedidoResponseDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var pedidos = await _unitOfWork.Pedidos.GetByUsuarioIdAsync(usuarioId);
            return _mapper.Map<List<PedidoResponseDto>>(pedidos);
        }

        public async Task<List<PedidoResponseDto>> GetByEstadoAsync(string estado)
        {
            var pedidos = await _unitOfWork.Pedidos.GetByEstadoAsync(estado);
            return _mapper.Map<List<PedidoResponseDto>>(pedidos);
        }

        public async Task<PedidoResponseDto?> UpdateEstadoAsync(int id, UpdatePedidoEstadoRequestDto request)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(id);

            if (pedido == null)
                return null;

            var estadoAnterior = pedido.Estado;
            pedido.Estado = request.Estado;
            pedido.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Pedidos.UpdateAsync(pedido);

            // Crear historial
            var historial = new PedidoHistorial
            {
                PedidoId = pedido.Id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = request.Estado,
                FechaCambio = DateTime.UtcNow,
                Notas = request.Notas
            };

            await _unitOfWork.SaveChangesAsync();

            var pedidoCompleto = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(id);
            return _mapper.Map<PedidoResponseDto>(pedidoCompleto!);
        }

        public async Task<bool> AprobarPedidoAsync(int id, int aprobadoPor)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(id);

            if (pedido == null)
                return false;

            pedido.Aprobado = true;
            pedido.FechaAprobacion = DateTime.UtcNow;
            pedido.Estado = "Confirmado";

            await _unitOfWork.Pedidos.UpdateAsync(pedido);

            // Crear historial
            var historial = new PedidoHistorial
            {
                PedidoId = pedido.Id,
                EstadoAnterior = "Pendiente",
                EstadoNuevo = "Confirmado",
                FechaCambio = DateTime.UtcNow,
                CambiadoPor = aprobadoPor,
                Notas = "Pedido aprobado"
            };

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<PedidoResponseDto>> GetPedidosPendientesAsync()
        {
            var pedidos = await _unitOfWork.Pedidos.GetPedidosPendientesAsync();
            return _mapper.Map<List<PedidoResponseDto>>(pedidos);
        }
    }
}