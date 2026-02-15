using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class PedidoHistorialConfiguration : IEntityTypeConfiguration<PedidoHistorial>
    {
        public void Configure(EntityTypeBuilder<PedidoHistorial> builder)
        {
            builder.ToTable("PedidoHistoriales");

            builder.HasKey(ph => ph.Id);

            builder.Property(ph => ph.EstadoAnterior)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ph => ph.EstadoNuevo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ph => ph.Notas)
                .HasMaxLength(500);

            // Relación
            builder.HasOne(ph => ph.Pedido)
                .WithMany(p => p.Historial)
                .HasForeignKey(ph => ph.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para consultas por pedido
            builder.HasIndex(ph => ph.PedidoId)
                .HasDatabaseName("IX_PedidoHistoriales_PedidoId");
        }
    }
}