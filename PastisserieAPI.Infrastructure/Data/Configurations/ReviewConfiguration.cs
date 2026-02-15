using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Calificacion)
                .IsRequired();

            builder.Property(r => r.Comentario)
                .HasMaxLength(1000);

            // Relaciones
            builder.HasOne(r => r.Usuario)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); // Preservar reviews aunque se elimine usuario

            builder.HasOne(r => r.Producto)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina producto, eliminar sus reviews

            // Índice compuesto para búsquedas optimizadas
            builder.HasIndex(r => new { r.ProductoId, r.Aprobada })
                .HasDatabaseName("IX_Reviews_ProductoId_Aprobada");

            // Índice para evitar reviews duplicadas
            builder.HasIndex(r => new { r.UsuarioId, r.ProductoId })
                .IsUnique()
                .HasDatabaseName("IX_Reviews_UsuarioId_ProductoId_Unique");
        }
    }
}