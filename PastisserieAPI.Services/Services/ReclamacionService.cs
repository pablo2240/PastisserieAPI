using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Services.DTOs.Response;
using PastisserieAPI.Services.Services.Interfaces;

namespace PastisserieAPI.Services.Services
{
    public class ReclamacionService : IReclamacionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificacionService _notificacionService;

        public ReclamacionService(IUnitOfWork unitOfWork, INotificacionService notificacionService)
        {
            _unitOfWork = unitOfWork;
            _notificacionService = notificacionService;
        }

        public async Task<ReclamacionResponseDto> CreateAsync(int usuarioId, int pedidoId, string motivo)
        {
            // 1. Verificar que el pedido existe
            var pedido = await _unitOfWork.Pedidos.GetByIdAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado.");

            // 2. Verificar que el pedido pertenece al usuario
            if (pedido.UsuarioId != usuarioId)
                throw new Exception("No tienes permiso para reclamar este pedido.");

            // 3. Verificar que el pedido fue entregado
            if (pedido.Estado != "Entregado")
                throw new Exception("Solo puedes reclamar pedidos que han sido entregados.");

            // 4. Verificar que han pasado menos de 3 días desde la entrega
            var fechaEntrega = pedido.FechaEntrega ?? pedido.FechaActualizacion ?? pedido.FechaPedido;
            var diasTranscurridos = (DateTime.UtcNow - fechaEntrega).TotalDays;
            if (diasTranscurridos > 3)
                throw new Exception($"El plazo para reclamar ha caducado. Solo se permiten reclamaciones dentro de los 3 días posteriores a la entrega. Han pasado {diasTranscurridos:F0} días.");

            // 5. Verificar que no haya ya una reclamación pendiente para el mismo pedido
            var existente = await _unitOfWork.Reclamaciones.FindAsync(r => r.PedidoId == pedidoId && r.UsuarioId == usuarioId && r.Estado == "Pendiente");
            if (existente.Any())
                throw new Exception("Ya tienes una reclamación pendiente para este pedido.");

            // 6. Crear la reclamación
            var reclamacion = new Reclamacion
            {
                PedidoId = pedidoId,
                UsuarioId = usuarioId,
                Fecha = DateTime.UtcNow,
                Motivo = motivo,
                Estado = "Pendiente"
            };

            await _unitOfWork.Reclamaciones.AddAsync(reclamacion);
            await _unitOfWork.SaveChangesAsync();

            // 7. Notificar al usuario
            try
            {
                await _notificacionService.CrearNotificacionAsync(
                    usuarioId,
                    "Reclamación Recibida",
                    $"Tu reclamación para el Pedido #{pedidoId} ha sido registrada y está en revisión.",
                    "Reclamacion",
                    "/reclamaciones"
                );

                // Notificar también al(los) Administrador(es)
                var admins = (await _unitOfWork.Users.GetAllAsync()).Where(u => u.Id == 1 || (u.Email != null && u.Email.Contains("admin")));
                foreach (var admin in admins)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        admin.Id,
                        "Nueva Reclamación ⚠️",
                        $"El usuario #{usuarioId} ha creado una reclamación para el Pedido #{pedidoId}.",
                        "Reclamacion",
                        "/admin/reclamaciones"
                    );
                }
            }
            catch { /* No fallar si la notificación falla */ }

            var usuario = await _unitOfWork.Users.GetByIdAsync(usuarioId);
            return MapToDto(reclamacion, usuario?.Nombre ?? "");
        }

        public async Task<List<ReclamacionResponseDto>> GetByUsuarioIdAsync(int usuarioId)
        {
            var reclamaciones = await _unitOfWork.Reclamaciones.FindAsync(r => r.UsuarioId == usuarioId);
            var lista = reclamaciones.OrderByDescending(r => r.Fecha).ToList();

            var dtos = new List<ReclamacionResponseDto>();
            foreach (var r in lista)
            {
                var usuario = await _unitOfWork.Users.GetByIdAsync(r.UsuarioId);
                dtos.Add(MapToDto(r, usuario?.Nombre ?? ""));
            }
            return dtos;
        }

        public async Task<List<ReclamacionResponseDto>> GetAllAsync()
        {
            var reclamaciones = await _unitOfWork.Reclamaciones.GetAllAsync();
            var lista = reclamaciones.OrderByDescending(r => r.Fecha).ToList();

            var dtos = new List<ReclamacionResponseDto>();
            foreach (var r in lista)
            {
                var usuario = await _unitOfWork.Users.GetByIdAsync(r.UsuarioId);
                dtos.Add(MapToDto(r, usuario?.Nombre ?? ""));
            }
            return dtos;
        }

        public async Task<ReclamacionResponseDto?> UpdateEstadoAsync(int id, string estado)
        {
            var reclamacion = await _unitOfWork.Reclamaciones.GetByIdAsync(id);
            if (reclamacion == null) return null;

            reclamacion.Estado = estado;
            await _unitOfWork.Reclamaciones.UpdateAsync(reclamacion);
            await _unitOfWork.SaveChangesAsync();

            // Notificar al usuario del cambio
            try
            {
                await _notificacionService.CrearNotificacionAsync(
                    reclamacion.UsuarioId,
                    "Actualización de Reclamación",
                    $"Tu reclamación para el Pedido #{reclamacion.PedidoId} ha sido actualizada: {estado}.",
                    "Reclamacion",
                    "/reclamaciones"
                );
            }
            catch { }

            var usuario = await _unitOfWork.Users.GetByIdAsync(reclamacion.UsuarioId);
            return MapToDto(reclamacion, usuario?.Nombre ?? "");
        }

        private ReclamacionResponseDto MapToDto(Reclamacion r, string nombreUsuario)
        {
            return new ReclamacionResponseDto
            {
                Id = r.Id,
                PedidoId = r.PedidoId,
                UsuarioId = r.UsuarioId,
                NombreUsuario = nombreUsuario,
                Fecha = r.Fecha,
                Motivo = r.Motivo,
                Estado = r.Estado
            };
        }
    }
}
