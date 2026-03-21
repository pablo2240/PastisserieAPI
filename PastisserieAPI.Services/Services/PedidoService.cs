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
        private readonly IEmailService _emailService;
        private readonly IInvoiceService _invoiceService;
        private readonly INotificacionService _notificacionService;
        private readonly ITiendaService _tiendaService;

        public PedidoService(IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, IInvoiceService invoiceService, INotificacionService notificacionService, ITiendaService tiendaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _invoiceService = invoiceService;
            _notificacionService = notificacionService;
            _tiendaService = tiendaService;
        }

        public async Task<PedidoResponseDto> CreateAsync(int userId, CreatePedidoRequestDto request)
        {
            // 0. Validar horario laboral
            await ValidarHorarioLaboralAsync();

            // 1. Obtener el carrito
            var carritoActual = await _unitOfWork.Carritos.GetByUsuarioIdWithItemsAsync(userId);

            if (carritoActual == null || !carritoActual.Items.Any())
            {
                throw new Exception("El carrito está vacío o no existe.");
            }

            // 2. Mapeo inicial
            var pedido = _mapper.Map<Pedido>(request);
            pedido.UsuarioId = userId;
            pedido.FechaPedido = DateTime.UtcNow;
            pedido.FechaCreacion = DateTime.UtcNow;
            pedido.Estado = "Pendiente";

            // 1. Inyectar datos simulados en Notas (para que el Admin lo vea claro)
            var infoEnvio = string.Empty;
            if (!string.IsNullOrEmpty(request.Direccion)) infoEnvio += $"📍 Dirección: {request.Direccion}";
            if (!string.IsNullOrEmpty(request.MetodoPago)) infoEnvio += $"\n💳 Pago: {request.MetodoPago}";
            
            pedido.NotasCliente = string.IsNullOrEmpty(pedido.NotasCliente) 
                ? infoEnvio 
                : $"{pedido.NotasCliente}\n---\n{infoEnvio}";

            // 2. Determinar Método de Pago en BD
            var todosMetodos = await _unitOfWork.MetodosPagoUsuario.GetAllAsync();
            MetodoPagoUsuario? metodoSeleccionado = null;

            // A) Si es Tarjeta Simulada, intentamos crear/usar un método de Tarjeta
            if (!string.IsNullOrEmpty(request.MetodoPago) && request.MetodoPago.Contains("Tarjeta", StringComparison.OrdinalIgnoreCase))
            {
                var todosTipos = await _unitOfWork.TiposMetodoPago.GetAllAsync();
                var tipoTarjeta = todosTipos.FirstOrDefault(t => t.Id == 2) ?? todosTipos.FirstOrDefault(t => t.Nombre.Contains("Tarjeta"));

                if (tipoTarjeta != null)
                {
                    metodoSeleccionado = new MetodoPagoUsuario
                    {
                        UsuarioId = userId,
                        TipoMetodoPagoId = tipoTarjeta.Id,
                        UltimosDigitos = "4242",
                        TokenPago = $"SIM_{Guid.NewGuid().ToString().Substring(0, 8)}",
                        EsPredeterminado = false,
                        FechaCreacion = DateTime.UtcNow
                    };
                    await _unitOfWork.MetodosPagoUsuario.AddAsync(metodoSeleccionado);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            // B) Si no se seleccionó arriba (o es Efectivo/Otro), usamos lógica por defecto (Existente o Efectivo)
            if (metodoSeleccionado == null)
            {
                metodoSeleccionado = todosMetodos.FirstOrDefault(m => m.UsuarioId == userId);

                if (metodoSeleccionado == null)
                {
                    var todosTipos = await _unitOfWork.TiposMetodoPago.GetAllAsync();
                    var tipo = todosTipos.FirstOrDefault(); // Por defecto el primero (Efectivo)

                    if (tipo == null)
                    {
                        tipo = new TipoMetodoPago { Nombre = "Efectivo", Activo = true };
                        await _unitOfWork.TiposMetodoPago.AddAsync(tipo);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    metodoSeleccionado = new MetodoPagoUsuario
                    {
                        UsuarioId = userId,
                        TipoMetodoPagoId = tipo.Id,
                        UltimosDigitos = "0000",
                        TokenPago = "PAGO_EFECTIVO",
                        EsPredeterminado = true,
                        FechaCreacion = DateTime.UtcNow
                    };

                    await _unitOfWork.MetodosPagoUsuario.AddAsync(metodoSeleccionado);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            pedido.MetodoPagoId = metodoSeleccionado.Id;

            decimal subtotal = 0;
            var pedidoItems = new List<PedidoItem>();

            // 3. Validar Stock de TODOS los items antes de proceder
            foreach (var itemCart in carritoActual.Items)
            {
                var producto = itemCart.Producto;
                if (producto == null) continue;

                if (producto.Stock < itemCart.Cantidad)
                {
                    throw new Exception($"No hay suficiente stock para {producto.Nombre}. Disponible: {producto.Stock}, Solicitado: {itemCart.Cantidad}");
                }
            }
            foreach (var itemCart in carritoActual.Items)
            {
                var producto = itemCart.Producto;
                if (producto == null) continue;

                var pedidoItem = new PedidoItem
                {
                    ProductoId = itemCart.ProductoId,
                    Cantidad = itemCart.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = producto.Precio * itemCart.Cantidad
                };

                subtotal += pedidoItem.Subtotal;
                pedidoItems.Add(pedidoItem);

                // Descontar Stock
                if (producto.Stock >= itemCart.Cantidad)
                {
                    producto.Stock -= itemCart.Cantidad;
                    await _unitOfWork.Productos.UpdateAsync(producto);
                }
            }

            // 4. Totales (IVA REMOVIDO, DOMICILIO AÑADIDO)
            pedido.CostoEnvio = 5000; // Valor del domicilio
            pedido.Total = subtotal + pedido.CostoEnvio; 
            pedido.IVA = 0; 
            pedido.Subtotal = subtotal; 

            // 5. Guardar Pedido (Cabecera)
            await _unitOfWork.Pedidos.AddAsync(pedido);
            await _unitOfWork.SaveChangesAsync();

            // 6. Relacionar Items con el ID del pedido creado
            foreach (var item in pedidoItems)
            {
                item.PedidoId = pedido.Id;
                pedido.Items.Add(item);
            }
            // Actualizamos el pedido con sus items
            await _unitOfWork.Pedidos.UpdateAsync(pedido);

            // 7. Vaciar Carrito (Usando el ID del carrito, NO del usuario)
            await _unitOfWork.Carritos.ClearCarritoAsync(carritoActual.Id);

            // 8. Historial
            var historial = new PedidoHistorial
            {
                PedidoId = pedido.Id,
                EstadoAnterior = "",
                EstadoNuevo = "Pendiente",
                FechaCambio = DateTime.UtcNow,
                CambiadoPor = userId,
                Notas = "Pedido creado exitosamente"
            };
            // Nota: Si PedidoHistorial es una tabla aparte, agrégala al DbSet, si es colección:
            // pedido.Historial.Add(historial); 
            // await _unitOfWork.SaveChangesAsync();

            // Guardamos cambios finales
            await _unitOfWork.SaveChangesAsync();

            // 9. Retorno con datos completos (Include)
            var pedidoCompleto = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(pedido.Id)
                                 ?? await _unitOfWork.Pedidos.GetByIdAsync(pedido.Id);

            // 10. Notificación al usuario
            try
            {
                await _notificacionService.CrearNotificacionAsync(
                    userId,
                    "Pedido Recibido 🍰",
                    $"Tu pedido #{pedido.Id} ha sido creado exitosamente. Total: ${pedido.Total:N0} COP.",
                    "Pedido",
                    "/history"
                );
            }
            catch { }

            // 11. Enviar Correo de Confirmación y Factura (Fire and forget, or awaited)
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    // Send order confirmation along with invoice
                    byte[]? pdfBytes = null;
                    if (pedidoCompleto != null)
                    {
                        pdfBytes = _invoiceService.GenerateInvoicePdf(pedidoCompleto, user);
                    }
                    
                    await _emailService.SendOrderConfirmationEmailAsync(user.Email, user.Nombre, pedido.Id, pedido.Total, pdfBytes);
                }
            }
            catch { /* Ignorar errores de correo para no fallar el pedido */ }

            return _mapper.Map<PedidoResponseDto>(pedidoCompleto);
        }

        // --- MÉTODOS DE LECTURA ---

        // 👇 ESTE ES EL QUE NECESITA EL DASHBOARD
        public async Task<List<PedidoResponseDto>> GetAllAsync()
        {
            // Llama al repositorio que modificamos anteriormente (con los Includes de Usuario)
            var pedidos = await _unitOfWork.Pedidos.GetAllAsync();
            return _mapper.Map<List<PedidoResponseDto>>(pedidos);
        }

        public async Task<PedidoResponseDto?> GetByIdAsync(int id)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(id) ?? await _unitOfWork.Pedidos.GetByIdAsync(id);
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

        public async Task<List<PedidoResponseDto>> GetPedidosPendientesAsync()
        {
            var pedidos = await _unitOfWork.Pedidos.GetPedidosPendientesAsync();
            return _mapper.Map<List<PedidoResponseDto>>(pedidos);
        }

        public async Task<PedidoResponseDto?> UpdateEstadoAsync(int id, UpdatePedidoEstadoRequestDto request)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(id);
            if (pedido == null) return null;

            // Validar horario para acciones de cocina/preparación (excepto si es una re-asignación de un pedido fallido)
            var estadosRestringidos = new[] { "EnProceso" };
            if (estadosRestringidos.Contains(request.Estado, StringComparer.OrdinalIgnoreCase) && pedido.Estado != "NoEntregado")
            {
                await ValidarHorarioLaboralAsync();
            }

            pedido.Estado = request.Estado;
            pedido.FechaActualizacion = DateTime.UtcNow;

            // Al entregar, registrar la fecha real de entrega
            if (request.Estado == "Entregado")
            {
                pedido.FechaEntrega = DateTime.UtcNow;
                pedido.MotivoNoEntrega = null; // Limpiar si antes falló
            }
            
            // Si el estado es NoEntregado, registrar motivo y fecha de falla
            if (request.Estado == "NoEntregado")
            {
                if (string.IsNullOrWhiteSpace(request.MotivoNoEntrega))
                {
                    throw new Exception("El motivo de no entrega es obligatorio.");
                }

                pedido.FechaNoEntrega = DateTime.UtcNow;
                pedido.MotivoNoEntrega = request.MotivoNoEntrega;
                pedido.FechaEntrega = null; 
            }

            // Si se pasa de NoEntregado a Pendiente o Confirmado (reintento)
            if (request.Estado == "Pendiente" && pedido.Estado == "NoEntregado")
            {
                pedido.FechaNoEntrega = null;
                pedido.RepartidorId = null; // LIMPIAR REPARTIDOR PARA REASIGNACIÓN
            }
            else if ((request.Estado == "Pendiente" || request.Estado == "Confirmado") && pedido.Estado == "NoEntregado")
            {
                pedido.FechaNoEntrega = null;
            }

            await _unitOfWork.Pedidos.UpdateAsync(pedido);
            await _unitOfWork.SaveChangesAsync();

            // Notificar al usuario del cambio de estado
            try
            {
                var estadoMensaje = request.Estado switch
                {
                    "EnProceso"    => "está siendo preparado 👨‍🍳",
                    "EnCamino"     => "está en camino 🚚",
                    "Entregado"    => "fue entregado exitosamente ✅",
                    "NoEntregado"  => "no pudo ser entregado. Por favor contáctanos. ❌",
                    "Cancelado"    => "fue cancelado 🚫",
                    _              => $"cambió a {request.Estado}"
                };
                await _notificacionService.CrearNotificacionAsync(
                    pedido.UsuarioId,
                    $"Pedido #{pedido.Id} - {request.Estado}",
                    $"Tu pedido #{pedido.Id} {estadoMensaje}.",
                    "Pedido",
                    "/history"
                );

                if (request.Estado == "NoEntregado")
                {
                    var admins = (await _unitOfWork.Users.GetAllAsync()).Where(u => u.Id == 1 || (u.Email != null && u.Email.Contains("admin")));
                    foreach (var admin in admins)
                    {
                        await _notificacionService.CrearNotificacionAsync(
                            admin.Id,
                            "Alerta: Pedido Fallido ❌",
                            $"El pedido #{pedido.Id} no pudo ser entregado. Motivo: {request.MotivoNoEntrega}.",
                            "Pedido",
                            "/admin/pedidos"
                        );
                    }
                }
            }
            catch { }

            // Enviar correo de cambio de estado
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(pedido.UsuarioId);
                if (user != null)
                {
                    if (request.Estado == "Entregado")
                    {
                        var pedidoCompleto = await _unitOfWork.Pedidos.GetByIdWithDetailsAsync(pedido.Id) ?? pedido;
                        var pdfBytes = _invoiceService.GenerateInvoicePdf(pedidoCompleto, user);
                        await _emailService.SendInvoiceEmailAsync(user.Email, user.Nombre, pedido.Id, pdfBytes);
                    }
                    else
                    {
                        await _emailService.SendOrderStatusUpdateEmailAsync(user.Email, user.Nombre, pedido.Id, request.Estado);
                    }
                }
            }
            catch { }

            return _mapper.Map<PedidoResponseDto>(pedido);
        }

        public async Task<bool> AprobarPedidoAsync(int id, int aprobadoPor)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(id);
            if (pedido == null) return false;

            pedido.Aprobado = true;
            pedido.FechaAprobacion = DateTime.UtcNow;
            pedido.Estado = "Confirmado";
            await _unitOfWork.Pedidos.UpdateAsync(pedido);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<PedidoResponseDto?> AsignarRepartidorAsync(int pedidoId, int repartidorId)
        {
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(pedidoId);
            if (pedido == null) return null;

            // CONTROL DE CARGA: Máximo 5 pedidos activos por repartidor
            var pedidosActivos = await _unitOfWork.Pedidos.GetAllAsync();
            var cargaActual = pedidosActivos.Count(p => p.RepartidorId == repartidorId && (p.Estado == "EnCamino" || p.Estado == "Confirmado"));
            
            if (cargaActual >= 5)
            {
                throw new Exception("El repartidor ya tiene demasiados pedidos asignados (máximo 5).");
            }

            pedido.RepartidorId = repartidorId;
            pedido.Estado = "EnCamino"; // Al asignar repartidor, pasa a EnCamino
            pedido.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Pedidos.UpdateAsync(pedido);
            await _unitOfWork.SaveChangesAsync();

            // Notificar al usuario y al repartidor
            try
            {
                await _notificacionService.CrearNotificacionAsync(
                    pedido.UsuarioId,
                    $"Pedido #{pedido.Id} - En Camino 🚚",
                    $"Tu pedido #{pedido.Id} ha sido asignado a un repartidor y está en camino.",
                    "Pedido",
                    "/history"
                );
                
                var repartidor = await _unitOfWork.Users.GetByIdAsync(repartidorId);
                var cliente = await _unitOfWork.Users.GetByIdAsync(pedido.UsuarioId);
                
                if (repartidor != null)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        repartidorId,
                        $"Nuevo Pedido Asignado #{pedido.Id} 📦",
                        $"Se te ha asignado el Pedido #{pedido.Id}. Revisa tu panel de entregas.",
                        "Asignacion",
                        "/repartidor/pedidos"
                    );

                    // Enviar Correo al Repartidor
                    await _emailService.SendRepartidorAssignmentEmailAsync(
                        repartidor.Email, 
                        repartidor.Nombre, 
                        pedido.Id, 
                        cliente?.Nombre ?? "Cliente", 
                        pedido.DireccionEnvio?.Direccion ?? "Ver en app"
                    );
                }
            }
            catch { }

            return _mapper.Map<PedidoResponseDto>(pedido);
        }

        public async Task<List<PedidoResponseDto>> GetByRepartidorIdAsync(int repartidorId)
        {
            var pedidos = await _unitOfWork.Pedidos.GetAllAsync();
            var filtrados = pedidos.Where(p => p.RepartidorId == repartidorId).ToList();
            return _mapper.Map<List<PedidoResponseDto>>(filtrados);
        }

        // ============ HELPER PRIVADO: Validar Horario Laboral ============
        private async Task ValidarHorarioLaboralAsync()
        {
            var config = await _tiendaService.GetConfiguracionAsync();
            if (config == null) return;

            if (!_tiendaService.EstaAbierto(config))
            {
                throw new Exception("La tienda está cerrada actualmente.");
            }
        }
    }
}
