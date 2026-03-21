using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Infrastructure.Data;
using PastisserieAPI.Services.DTOs.Common;
using System.Security.Claims;

namespace PastisserieAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================================================
        // GET /api/dashboard/admin
        // Solo Admin
        // ======================================================
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var ahora = DateTime.UtcNow.AddHours(-5); // Sincronizado con Medellín
            var hoy       = ahora.Date;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
            var inicioMes  = new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
            var inicioAnio = new DateTime(ahora.Year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            // Base Filter: Pedidos válidos (No Cancelados, Rechazados ni NoEntregados)
            var pedidosVentas = await _context.Pedidos
                .Where(p => p.Estado != "Cancelado" && p.Estado != "Rechazado" && p.Estado != "NoEntregado")
                .Include(p => p.Items)
                .ToListAsync();

            // Ganancias (UNIFICADO: El Total Histórico incluye todo lo NO cancelado/rechazado)
            var gananciasTotales = pedidosVentas.Sum(p => p.Total);
            var ventasHoy        = pedidosVentas.Where(p => p.FechaPedido.AddHours(-5).Date == hoy)
                                    .Sum(p => p.Total);
            var ventasSemana     = pedidosVentas.Where(p => p.FechaPedido.AddHours(-5) >= inicioSemana)
                                    .Sum(p => p.Total);
            var ventasMes        = pedidosVentas.Where(p => p.FechaPedido.AddHours(-5) >= inicioMes)
                                    .Sum(p => p.Total);
            var ventasAnio       = pedidosVentas.Where(p => p.FechaPedido.AddHours(-5) >= inicioAnio)
                                    .Sum(p => p.Total);

            // Pedidos por estado (todos)
            var todosPedidos = await _context.Pedidos.Select(p => p.Estado).ToListAsync();
            var pedidosPorEstado = todosPedidos
                .GroupBy(e => e)
                .ToDictionary(g => g.Key, g => g.Count());

            // Asegurar que estados críticos tengan al menos un 0 si no existen
            if (!pedidosPorEstado.ContainsKey("NoEntregado")) pedidosPorEstado["NoEntregado"] = 0;
            if (!pedidosPorEstado.ContainsKey("Cancelado")) pedidosPorEstado["Cancelado"] = 0;
            if (!pedidosPorEstado.ContainsKey("Rechazado")) pedidosPorEstado["Rechazado"] = 0;
            if (!pedidosPorEstado.ContainsKey("Pendiente")) pedidosPorEstado["Pendiente"] = 0;

            // Top 5 más y menos vendidos (Incluye los que tienen 0 ventas)
            var todosProductos = await _context.Productos.Select(p => new { p.Id, p.Nombre }).ToListAsync();
            var itemsVendidos = await _context.PedidoItems
                .Where(i => _context.Pedidos.Any(p => p.Id == i.PedidoId && p.Estado != "Cancelado" && p.Estado != "Rechazado" && p.Estado != "NoEntregado"))
                .GroupBy(i => i.ProductoId)
                .Select(g => new { ProductoId = g.Key, TotalCantidad = g.Sum(i => i.Cantidad) })
                .ToListAsync();

            var statsProductos = todosProductos.Select(p => new
            {
                p.Id,
                Nombre = p.Nombre,
                TotalCantidad = itemsVendidos.FirstOrDefault(v => v.ProductoId == p.Id)?.TotalCantidad ?? 0
            }).ToList();

            var topMasVendidos   = statsProductos.OrderByDescending(i => i.TotalCantidad).Take(5).ToList();
            var topMenosVendidos = statsProductos.OrderBy(i => i.TotalCantidad).Take(5).ToList();

            // Ventas por día de la semana actual
            var ventasPorDia = new List<object>();
            for (int i = 0; i < 7; i++)
            {
                var dia = inicioSemana.AddDays(i);
                var venta = pedidosVentas
                    .Where(p => p.FechaPedido.AddHours(-5).Date == dia.Date)
                    .Sum(p => p.Total);
                ventasPorDia.Add(new { nombre = dia.ToString("ddd"), fecha = dia.ToString("yyyy-MM-dd"), ventas = venta });
            }

            var limiteRetraso = ahora.AddMinutes(-30);
            var pedidosRetrasados = await _context.Pedidos
                .Where(p => (p.Estado == "Pendiente" || p.Estado == "EnProceso") && p.FechaPedido <= limiteRetraso)
                .OrderBy(p => p.FechaPedido)
                .Select(p => new {
                    p.Id,
                    Cliente = p.Usuario.Nombre,
                    p.Estado,
                    MinutosRetraso = (int)(DateTime.UtcNow - p.FechaPedido).TotalMinutes,
                    p.Total
                })
                .ToListAsync();

            var result = new
            {
                gananciasTotales,
                ventasHoy,
                ventasSemana,
                ventasMes,
                ventasAnio,
                topMasVendidos,
                topMenosVendidos,
                pedidosPorEstado,
                ventasPorDia,
                totalPedidos = todosPedidos.Count,
                pedidosPendientes = pedidosPorEstado.GetValueOrDefault("Pendiente", 0),
                alertasCriticas = new {
                    conteoRetrasados = pedidosRetrasados.Count,
                    pedidosRetrasados = pedidosRetrasados,
                    stockBajo = await _context.Productos.CountAsync(p => p.Stock < 5),
                    reclamacionesPendientes = await _context.Reclamaciones.CountAsync(r => r.Estado == "Pendiente")
                },
                pedidosFallidosDetalle = await _context.Pedidos
                    .Where(p => p.Estado == "NoEntregado" || p.Estado == "Cancelado" || p.Estado == "Rechazado")
                    .OrderByDescending(p => p.FechaNoEntrega ?? p.FechaActualizacion ?? p.FechaPedido)
                    .Take(10)
                    .Select(p => new
                    {
                        p.Id,
                        Cliente = p.Usuario.Nombre,
                        Repartidor = p.Repartidor != null ? p.Repartidor.Nombre : "No asignado",
                        Fecha = (p.FechaNoEntrega ?? p.FechaActualizacion ?? p.FechaPedido).AddHours(-5).ToString("dd/MM/yyyy HH:mm"),
                        p.MotivoNoEntrega,
                        p.Total,
                        p.Estado
                    })
                    .ToListAsync()
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpGet("earnings-history")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetEarningsHistory([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var fechaFin = end ?? DateTime.UtcNow.AddHours(-5);
            var fechaInicio = start ?? fechaFin.AddDays(-30);

            var pedidos = await _context.Pedidos
                .Where(p => p.Estado != "Cancelado" && p.Estado != "Rechazado" && p.Estado != "NoEntregado" && (p.FechaEntrega ?? p.FechaPedido) >= fechaInicio && (p.FechaEntrega ?? p.FechaPedido) <= fechaFin)
                .ToListAsync();

            // Si es un solo día, agrupamos por HORA para mayor detalle
            if (fechaInicio.Date == fechaFin.Date)
            {
                var dataHoras = pedidos
                    .GroupBy(p => (p.FechaEntrega ?? p.FechaPedido).AddHours(-5).Hour) // Ajuste a Colombia
                    .Select(g => new
                    {
                        hora = g.Key,
                        ventas = g.Sum(p => p.Total)
                    })
                    .ToList();

                var hourlyData = new List<object>();
                for (int h = 0; h < 24; h++)
                {
                    var hStr = h.ToString("D2") + ":00";
                    var v = dataHoras.FirstOrDefault(x => x.hora == h)?.ventas ?? 0;
                    hourlyData.Add(new { fecha = hStr, nombre = hStr, ventas = v });
                }

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    dailyData = hourlyData,
                    periodStats = new
                    {
                        totalGanancia = pedidos.Sum(p => p.Total),
                        totalPedidos = pedidos.Count,
                        promedioVenta = pedidos.Count > 0 ? pedidos.Sum(p => p.Total) / pedidos.Count : 0
                    },
                    topProductsInRange = await GetTopProductsInRange(pedidos),
                    ordersInRange = MapOrders(pedidos)
                }));
            }

            var data = pedidos
                .GroupBy(p => (p.FechaEntrega ?? p.FechaPedido).Date)
                .Select(g => new
                {
                    fecha = g.Key.ToString("yyyy-MM-dd"),
                    nombre = g.Key.ToString("dd/MM"),
                    ventas = g.Sum(p => p.Total)
                })
                .OrderBy(x => x.fecha)
                .ToList();

            // Rellenar días sin ventas para la gráfica
            var dailyData = new List<object>();
            for (var dt = fechaInicio.Date; dt <= fechaFin.Date; dt = dt.AddDays(1))
            {
                var diaStr = dt.ToString("yyyy-MM-dd");
                var diaData = data.FirstOrDefault(d => d.fecha == diaStr);
                dailyData.Add(new
                {
                    fecha = diaStr,
                    nombre = dt.ToString("dd/MM"),
                    ventas = diaData?.ventas ?? 0
                });
            }

            // Estadísticas adicionales para el periodo
            var totalGanancia = pedidos.Sum(p => p.Total);
            var totalPedidos  = pedidos.Count;
            
            // Productos más vendidos EN ESTE RANGO
            var pedidosRangeIds = pedidos.Select(p => p.Id).ToList();
            var topProductsInRange = await _context.PedidoItems
                .Where(pi => pedidosRangeIds.Contains(pi.PedidoId))
                .GroupBy(pi => pi.Producto.Nombre)
                .Select(g => new
                {
                    nombre = g.Key,
                    totalCantidad = g.Sum(pi => pi.Cantidad)
                })
                .OrderByDescending(x => x.totalCantidad)
                .Take(5)
                .ToListAsync();

            // Órdenes en este rango (resumen)
            var ordersInRange = pedidos
                .OrderByDescending(p => p.FechaEntrega ?? p.FechaPedido)
                .Select(p => new
                {
                    id = p.Id,
                    total = p.Total,
                    estado = p.Estado,
                    fecha = (p.FechaEntrega ?? p.FechaPedido).ToString("yyyy-MM-dd HH:mm"),
                    cliente = _context.Users.Where(u => u.Id == p.UsuarioId).Select(u => u.Nombre).FirstOrDefault() ?? "Cliente"
                })
                .Take(20) // Limitamos a las 20 más recientes del rango
                .ToList();

            var result = new
            {
                dailyData,
                periodStats = new
                {
                    totalGanancia,
                    totalPedidos,
                    promedioVenta = totalPedidos > 0 ? totalGanancia / totalPedidos : 0
                },
                topProductsInRange,
                ordersInRange
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        // ======================================================
        // GET /api/dashboard/repartidor
        // Solo Repartidor (datos filtrados por su ID)
        // ======================================================
        [HttpGet("repartidor")]
        [Authorize(Roles = "Repartidor")]
        public async Task<IActionResult> GetRepartidorDashboard()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int repartidorId))
                return Unauthorized(ApiResponse<string>.ErrorResponse("No autenticado"));

            var pedidos = await _context.Pedidos
                .Where(p => p.RepartidorId == repartidorId)
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Include(p => p.DireccionEnvio)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();

            var entregados    = pedidos.Where(p => p.Estado == "Entregado").ToList();
            var fallidos      = pedidos.Where(p => p.Estado == "NoEntregado" || p.Estado == "Cancelado" || p.Estado == "Rechazado").ToList();
            var enCamino      = pedidos.Where(p => p.Estado == "EnCamino" || p.Estado == "Pendiente" || p.Estado == "Confirmado").ToList();
            var historial     = pedidos.Where(p => p.Estado == "Entregado" || p.Estado == "NoEntregado").ToList();
            
            // EL REPARTIDOR GANA 5,000 POR DOMICILIO COMPLETADO
            var ganancias     = entregados.Count * 5000;

            var mapPedido = (Core.Entities.Pedido p) => new
            {
                p.Id,
                p.Estado,
                p.Total,
                p.CostoEnvio, // Incluimos el costo de envío explícitamente
                p.FechaPedido,
                p.FechaEntrega,
                usuario = new { p.Usuario?.Nombre, p.Usuario?.Email, p.Usuario?.Telefono },
                direccionEnvio = p.DireccionEnvio == null ? null : new
                {
                    p.DireccionEnvio.Direccion,
                    Ciudad = p.DireccionEnvio.Barrio ?? "Medellín",
                    Notas = p.DireccionEnvio.Referencia ?? ""
                },
                items = p.Items.Select(i => new
                {
                    cantidad = i.Cantidad,
                    precioUnitario = i.PrecioUnitario,
                    producto = new { nombre = i.Producto?.Nombre, categoria = i.Producto?.Categoria }
                }).ToList(),
                itemsCount = p.Items.Count
            };

            var result = new
            {
                resumen = new
                {
                    totalEntregados   = entregados.Count,
                    totalNoEntregados = fallidos.Count,
                    totalEnCamino     = enCamino.Count,
                    gananciasTotales  = ganancias // Ganancias fijas 5mil
                },
                enCamino    = enCamino.Select(mapPedido).ToList(),
                entregados  = entregados.Select(mapPedido).ToList(),
                noEntregados = fallidos.Select(mapPedido).ToList(),
                historial   = historial.Select(mapPedido).ToList()
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        // ================= HELPER METHODS =================

        private async Task<List<object>> GetTopProductsInRange(List<Core.Entities.Pedido> pedidos)
        {
            var pedidosRangeIds = pedidos.Select(p => p.Id).ToList();
            var top = await _context.PedidoItems
                .Where(pi => pedidosRangeIds.Contains(pi.PedidoId))
                .GroupBy(pi => pi.Producto.Nombre)
                .Select(g => new
                {
                    nombre = g.Key,
                    totalCantidad = g.Sum(pi => pi.Cantidad)
                })
                .OrderByDescending(x => x.totalCantidad)
                .Take(5)
                .ToListAsync();

            return top.Cast<object>().ToList();
        }

        private List<object> MapOrders(List<Core.Entities.Pedido> pedidos)
        {
            return pedidos
                .OrderByDescending(p => p.FechaEntrega ?? p.FechaPedido)
                .Select(p => new
                {
                    id = p.Id,
                    total = p.Total,
                    estado = p.Estado,
                    fecha = (p.FechaEntrega ?? p.FechaPedido).AddHours(-5).ToString("yyyy-MM-dd HH:mm"),
                    cliente = _context.Users.Where(u => u.Id == p.UsuarioId).Select(u => u.Nombre).FirstOrDefault() ?? "Cliente"
                })
                .Take(20)
                .Cast<object>()
                .ToList();
        }
    }
}
