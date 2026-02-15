using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
    {
        public void Configure(EntityTypeBuilder<Factura> builder)
        {
            builder.ToTable("Facturas");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.NumeroFactura)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(f => f.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(f => f.Total)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(f => f.RutaArchivo)
                .HasMaxLength(500);

            // Índice único para número de factura
            builder.HasIndex(f => f.NumeroFactura)
                .IsUnique()
                .HasDatabaseName("IX_Facturas_NumeroFactura");

            // Relación 1:1 con Pedido (ya configurada en PedidoConfiguration)
        }
    }
}