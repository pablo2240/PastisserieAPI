using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
    {
        public void Configure(EntityTypeBuilder<PedidoItem> builder)
        {
            builder.ToTable("PedidoItems");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.PrecioUnitario)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(pi => pi.Subtotal)
                .HasColumnType("decimal(18,2)");

            // Relaciones
            builder.HasOne(pi => pi.Pedido)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PedidoId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina pedido, eliminar items

            builder.HasOne(pi => pi.Producto)
                .WithMany(p => p.PedidoItems)
                .HasForeignKey(pi => pi.ProductoId)
                .OnDelete(DeleteBehavior.Restrict); // NO eliminar producto con pedidos históricos

            // Índice para consultas
            builder.HasIndex(pi => pi.PedidoId)
                .HasDatabaseName("IX_PedidoItems_PedidoId");
        }
    }
}