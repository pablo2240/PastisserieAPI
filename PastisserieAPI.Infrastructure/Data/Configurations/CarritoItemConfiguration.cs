using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class CarritoItemConfiguration : IEntityTypeConfiguration<CarritoItem>
    {
        public void Configure(EntityTypeBuilder<CarritoItem> builder)
        {
            builder.ToTable("CarritoItems");

            builder.HasKey(ci => ci.Id);

            // Relaciones
            builder.HasOne(ci => ci.Carrito)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CarritoId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina carrito, eliminar items

            builder.HasOne(ci => ci.Producto)
                .WithMany(p => p.CarritoItems)
                .HasForeignKey(ci => ci.ProductoId)
                .OnDelete(DeleteBehavior.Restrict); // NO eliminar producto con items en carritos activos

            // Índice para evitar productos duplicados en el mismo carrito
            builder.HasIndex(ci => new { ci.CarritoId, ci.ProductoId })
                .IsUnique()
                .HasDatabaseName("IX_CarritoItems_CarritoId_ProductoId");

            // Configuración para ReservaHasta
            builder.Property(ci => ci.ReservaHasta)
                .IsRequired(false); // Nullable

            // Índice para búsquedas de reservas expiradas
            builder.HasIndex(ci => ci.ReservaHasta)
                .HasDatabaseName("IX_CarritoItems_ReservaHasta");
        }
    }
}