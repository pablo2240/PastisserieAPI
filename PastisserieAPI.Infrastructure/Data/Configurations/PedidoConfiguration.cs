using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("Pedidos");

            builder.HasKey(p => p.Id);

            // Estado del pedido
            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente");

            // Campos de pago Wompi
            builder.Property(p => p.EstadoPago)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente");

            builder.Property(p => p.TransaccionId)
                .HasMaxLength(100);

            builder.Property(p => p.MetodoPago)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Efectivo");

            // Costos
            builder.Property(p => p.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CostoEnvio)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Total)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.NotasCliente)
                .HasMaxLength(1000);

            // Relaciones
            builder.HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar usuario con pedidos

            builder.HasOne(p => p.DireccionEnvio)
                .WithMany(d => d.Pedidos)
                .HasForeignKey(p => p.DireccionEnvioId)
                .OnDelete(DeleteBehavior.SetNull); // Permitir null si se elimina dirección

            // Relación 1:1 con Factura
            builder.HasOne(p => p.Factura)
                .WithOne(f => f.Pedido)
                .HasForeignKey<Factura>(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación 1:0..1 con Envío
            builder.HasOne(p => p.Envio)
                .WithOne(e => e.Pedido)
                .HasForeignKey<Envio>(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para búsquedas frecuentes
            builder.HasIndex(p => p.Estado)
                .HasDatabaseName("IX_Pedidos_Estado");

            builder.HasIndex(p => p.EstadoPago)
                .HasDatabaseName("IX_Pedidos_EstadoPago");

            builder.HasIndex(p => p.FechaPedido)
                .HasDatabaseName("IX_Pedidos_FechaPedido");

            builder.HasIndex(p => new { p.UsuarioId, p.Estado })
                .HasDatabaseName("IX_Pedidos_UsuarioId_Estado");

            builder.HasIndex(p => new { p.UsuarioId, p.FechaPedido })
                .HasDatabaseName("IX_Pedidos_UsuarioId_FechaPedido");
        }
    }
}