using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Data;

namespace PastisserieAPI.Infrastructure.Repositories
{
    public class PedidoRepository : Repository<Pedido>, IPedidoRepository
    {
        public PedidoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pedido>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(p => p.CategoriaProducto) // Incluir categoría del producto
                .Where(p => p.UsuarioId == usuarioId)
                .OrderByDescending(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetByEstadoAsync(string estado)
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == estado)
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<Pedido?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                        .ThenInclude(prod => prod.CategoriaProducto) // Incluir categoría
                .Include(p => p.Usuario)
                .Include(p => p.DireccionEnvio)
                .Include(p => p.Envio)
                    .ThenInclude(e => e!.Repartidor)
                .Include(p => p.Factura)
                .Include(p => p.Historial)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> GetPedidosPendientesAsync()
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == "Pendiente" || p.Estado == "Confirmado")
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pedido>> GetPedidosEnPreparacionAsync()
        {
            return await _dbSet
                .Include(p => p.Usuario)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Producto)
                .Where(p => p.Estado == "EnPreparacion")
                .OrderBy(p => p.FechaPedido)
                .ToListAsync();
        }
    }
}