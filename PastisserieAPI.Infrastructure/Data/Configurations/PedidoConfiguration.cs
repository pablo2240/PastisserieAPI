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

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente");

            builder.Property(p => p.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CostoEnvio)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Total)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.NotasCliente)
                .HasMaxLength(1000);

            // Relación con Factura (1:1)
            builder.HasOne(p => p.Factura)
                .WithOne(f => f.Pedido)
                .HasForeignKey<Factura>(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con Envio (1:0..1)
            builder.HasOne(p => p.Envio)
                .WithOne(e => e.Pedido)
                .HasForeignKey<Envio>(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(p => p.Estado)
                .HasDatabaseName("IX_Pedidos_Estado");

            builder.HasIndex(p => p.FechaPedido)
                .HasDatabaseName("IX_Pedidos_FechaPedido");

            builder.HasIndex(p => p.UsuarioId)
                .HasDatabaseName("IX_Pedidos_UsuarioId");
        }
    }
}