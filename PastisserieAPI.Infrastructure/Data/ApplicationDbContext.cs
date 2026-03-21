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

        // ============ PERSONALIZACIÓN ============
        public DbSet<PersonalizadoConfig> PersonalizadoConfigs { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<PersonalizadoConfigIngrediente> PersonalizadoConfigIngredientes { get; set; }

        // ============ CARRITO ============
        public DbSet<CarritoCompra> Carritos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }

        // ============ PROMOCIONES ============
        public DbSet<Promocion> Promociones { get; set; }

        // ============ PAGOS ============
        public DbSet<TipoMetodoPago> TiposMetodoPago { get; set; }
        public DbSet<MetodoPagoUsuario> MetodosPagoUsuario { get; set; }

        // ============ DIRECCIONES Y ENVÍOS ============
        public DbSet<DireccionEnvio> DireccionesEnvio { get; set; }
        public DbSet<Envio> Envios { get; set; }

        // ============ NOTIFICACIONES ============
        public DbSet<Notificacion> Notificaciones { get; set; }

        // ============ CONFIGURACIÓN ============
        public DbSet<ConfiguracionTienda> ConfiguracionTienda { get; set; }
        public DbSet<HorarioDia> HorariosPorDia { get; set; }

        // ============ RECLAMACIONES ============
        public DbSet<Reclamacion> Reclamaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Evitar ciclos de cascada en Reclamación
            modelBuilder.Entity<Reclamacion>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction); // Evita el error de SQL Server 1785

            // Aplicar todas las configuraciones automáticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configuraciones adicionales
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // ============ USER - ENVIO (Repartidor) ============
            modelBuilder.Entity<Envio>()
                .HasOne(e => e.Repartidor)
                .WithMany(u => u.EnviosAsignados)
                .HasForeignKey(e => e.RepartidorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ PEDIDO - USER ============
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ REVIEW - USER ============
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============ ÍNDICES ÚNICOS ============
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Factura>()
                .HasIndex(f => f.NumeroFactura)
                .IsUnique();

            // ============ ÍNDICES DE RENDIMIENTO ============
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Categoria);

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.Estado);

            modelBuilder.Entity<Pedido>()
                .HasIndex(p => p.FechaPedido);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Actualizar automáticamente FechaActualizacion
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("FechaActualizacion") != null)
                {
                    entry.Property("FechaActualizacion").CurrentValue = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}