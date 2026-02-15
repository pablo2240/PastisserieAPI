using Microsoft.EntityFrameworkCore.Storage;
using PastisserieAPI.Core.Entities;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Data;

namespace PastisserieAPI.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositorios específicos
        private IUserRepository? _users;
        private IProductoRepository? _productos;
        private IPedidoRepository? _pedidos;
        private ICarritoRepository? _carritos;
        private IEnvioRepository? _envios;
        private IReviewRepository? _reviews;
        private IRepository<UserRol>? _userRoles;

        // Repositorios genéricos
        private IRepository<Rol>? _roles;
        private IRepository<CategoriaProducto>? _categorias;
        private IRepository<TipoMetodoPago>? _tiposMetodoPago;
        private IRepository<Notificacion>? _notificaciones;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // Implementación de propiedades - Repositorios específicos
        public IUserRepository Users
        {
            get { return _users ??= new UserRepository(_context); }
        }

        public IProductoRepository Productos
        {
            get { return _productos ??= new ProductoRepository(_context); }
        }

        public IPedidoRepository Pedidos
        {
            get { return _pedidos ??= new PedidoRepository(_context); }
        }

        public ICarritoRepository Carritos
        {
            get { return _carritos ??= new CarritoRepository(_context); }
        }

        public IEnvioRepository Envios
        {
            get { return _envios ??= new EnvioRepository(_context); }
        }

        public IReviewRepository Reviews
        {
            get { return _reviews ??= new ReviewRepository(_context); }
        }

        // Implementación de propiedades - Repositorios genéricos
        public IRepository<Rol> Roles
        {
            get { return _roles ??= new Repository<Rol>(_context); }
        }

        public IRepository<UserRol> UserRoles
        {
            get { return _userRoles ??= new Repository<UserRol>(_context); }
        }

        public IRepository<CategoriaProducto> Categorias
        {
            get { return _categorias ??= new Repository<CategoriaProducto>(_context); }
        }

        public IRepository<TipoMetodoPago> TiposMetodoPago
        {
            get { return _tiposMetodoPago ??= new Repository<TipoMetodoPago>(_context); }
        }

        public IRepository<Notificacion> Notificaciones
        {
            get { return _notificaciones ??= new Repository<Notificacion>(_context); }
        }

        // Métodos de persistencia
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Métodos de transacciones
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Dispose
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}