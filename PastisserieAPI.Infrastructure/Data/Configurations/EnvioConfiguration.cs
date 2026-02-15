using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class EnvioConfiguration : IEntityTypeConfiguration<Envio>
    {
        public void Configure(EntityTypeBuilder<Envio> builder)
        {
            builder.ToTable("Envios");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.NumeroGuia)
                .HasMaxLength(100);

            builder.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pendiente");

            // Relaciones
            builder.HasOne(e => e.Repartidor)
                .WithMany(u => u.EnviosAsignados)
                .HasForeignKey(e => e.RepartidorId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar repartidor con envíos asignados

            // Relación 1:1 con Pedido (ya configurada en PedidoConfiguration)

            // Índices para búsquedas frecuentes
            builder.HasIndex(e => e.Estado)
                .HasDatabaseName("IX_Envios_Estado");

            builder.HasIndex(e => new { e.RepartidorId, e.Estado })
                .HasDatabaseName("IX_Envios_RepartidorId_Estado");

            builder.HasIndex(e => e.PedidoId)
                .IsUnique()
                .HasDatabaseName("IX_Envios_PedidoId");
        }
    }
}