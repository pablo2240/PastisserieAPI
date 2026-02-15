using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
    {
        public void Configure(EntityTypeBuilder<Notificacion> builder)
        {
            builder.ToTable("Notificaciones");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(n => n.Mensaje)
                .IsRequired()
                .HasMaxLength(1000);

            // Relación
            builder.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificaciones)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para consultas frecuentes
            builder.HasIndex(n => new { n.UsuarioId, n.Leida })
                .HasDatabaseName("IX_Notificaciones_UsuarioId_Leida");

            builder.HasIndex(n => new { n.UsuarioId, n.Fecha })
                .HasDatabaseName("IX_Notificaciones_UsuarioId_Fecha");
        }
    }
}