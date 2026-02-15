using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class DireccionEnvioConfiguration : IEntityTypeConfiguration<DireccionEnvio>
    {
        public void Configure(EntityTypeBuilder<DireccionEnvio> builder)
        {
            builder.ToTable("DireccionesEnvio");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.NombreCompleto)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Direccion)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.Barrio)
                .HasMaxLength(100);

            builder.Property(d => d.Referencia)
                .HasMaxLength(500);

            builder.Property(d => d.Telefono)
                .IsRequired()
                .HasMaxLength(20);

            // Relación
            builder.HasOne(d => d.Usuario)
                .WithMany(u => u.Direcciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para consultas por usuario
            builder.HasIndex(d => d.UsuarioId)
                .HasDatabaseName("IX_DireccionesEnvio_UsuarioId");

            // Índice para direcciones predeterminadas
            builder.HasIndex(d => new { d.UsuarioId, d.EsPredeterminada })
                .HasDatabaseName("IX_DireccionesEnvio_UsuarioId_EsPredeterminada");
        }
    }
}