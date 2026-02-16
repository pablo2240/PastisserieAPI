using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PastisserieAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PastisserieAPI.Services.Services
{
    /// <summary>
    /// Servicio en background para liberar automáticamente reservas de stock expiradas (RN2)
    /// Se ejecuta cada 2 minutos para limpiar items de carrito con reservas vencidas
    /// </summary>
    public class ReservaStockService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservaStockService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(2);

        public ReservaStockService(
            IServiceProvider serviceProvider,
            ILogger<ReservaStockService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ Servicio de liberación de reservas de stock INICIADO");
            _logger.LogInformation("⏱️  Se ejecutará cada {Intervalo} minutos", _intervalo.TotalMinutes);

            // Esperar 30 segundos antes de la primera ejecución
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await LiberarReservasExpiradasAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al liberar reservas expiradas");
                }

                // Esperar intervalo antes de la siguiente ejecución
                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("⛔ Servicio de liberación de reservas DETENIDO");
        }

        private async Task LiberarReservasExpiradasAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                var ahora = DateTime.UtcNow;

                // Buscar items con reservas expiradas
                var itemsExpirados = await context.CarritoItems
                    .Where(ci => ci.ReservaHasta.HasValue && ci.ReservaHasta.Value < ahora)
                    .Include(ci => ci.Producto)
                    .ToListAsync(stoppingToken);

                if (itemsExpirados.Any())
                {
                    _logger.LogWarning(
                        "🧹 Liberando {Count} reservas expiradas de stock",
                        itemsExpirados.Count
                    );

                    // Log detallado de cada item eliminado
                    foreach (var item in itemsExpirados)
                    {
                        _logger.LogInformation(
                            "   📦 ProductoId: {ProductoId} ({Nombre}) - Cantidad: {Cantidad} - Expiró: {Expiracion}",
                            item.ProductoId,
                            item.Producto?.Nombre ?? "Desconocido",
                            item.Cantidad,
                            item.ReservaHasta
                        );
                    }

                    // Eliminar items expirados
                    context.CarritoItems.RemoveRange(itemsExpirados);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("✅ Reservas liberadas exitosamente");
                }
                else
                {
                    _logger.LogDebug("ℹ️  No hay reservas expiradas en este momento");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("⚠️  Operación de liberación cancelada");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "❌ Error al procesar liberación de reservas expiradas"
                );
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("⛔ Deteniendo servicio de reservas...");
            return base.StopAsync(cancellationToken);
        }
    }
}