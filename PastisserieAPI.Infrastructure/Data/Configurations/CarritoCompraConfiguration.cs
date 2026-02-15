using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class CarritoCompraConfiguration : IEntityTypeConfiguration<CarritoCompra>
    {
        public void Configure(EntityTypeBuilder<CarritoCompra> builder)
        {
            builder.ToTable("CarritosCompra");

            builder.HasKey(c => c.Id);

            // Relación 1:1 con Usuario
            builder.HasOne(c => c.Usuario)
                .WithOne(u => u.CarritoCompra)
                .HasForeignKey<CarritoCompra>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único por usuario
            builder.HasIndex(c => c.UsuarioId)
                .IsUnique()
                .HasDatabaseName("IX_CarritosCompra_UsuarioId");
        }
    }
}