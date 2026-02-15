using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============ USUARIOS Y ROLES ============
        public DbSet<User> Users { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UserRol> UserRoles { get; set; }

        // ============ PRODUCTOS ============
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // ============ PEDIDOS ============
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoItem> PedidoItems { get; set; }
        public DbSet<PedidoHistorial> PedidoHistoriales { get; set; }
        public DbSet<Factura> Facturas { get; set; }

        // ============ CARRITO ============
        public DbSet<CarritoCompra> CarritosCompra { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }

        // ============ DIRECCIONES Y ENVÍOS ============
        public DbSet<DireccionEnvio> DireccionesEnvio { get; set; }
        public DbSet<Envio> Envios { get; set; }

        // ============ NOTIFICACIONES ============
        public DbSet<Notificacion> Notificaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar TODAS las configuraciones Fluent API automáticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Datos iniciales (seeds)
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // ============ ROLES INICIALES ============
            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = 1, Nombre = "Usuario", Activo = true },
                new Rol { Id = 2, Nombre = "Admin", Activo = true },
                new Rol { Id = 3, Nombre = "Domiciliario", Activo = true },
                new Rol { Id = 4, Nombre = "Gerente", Activo = true }
            );

            // ============ CATEGORÍAS DE PRODUCTOS ============
            modelBuilder.Entity<CategoriaProducto>().HasData(
                new CategoriaProducto { Id = 1, Nombre = "Tortas", Descripcion = "Tortas y pasteles", Activa = true },
                new CategoriaProducto { Id = 2, Nombre = "Panes", Descripcion = "Variedad de panes artesanales", Activa = true },
                new CategoriaProducto { Id = 3, Nombre = "Postres", Descripcion = "Postres y dulces", Activa = true },
                new CategoriaProducto { Id = 4, Nombre = "Galletas", Descripcion = "Galletas caseras", Activa = true }
            );
        }
    }
}