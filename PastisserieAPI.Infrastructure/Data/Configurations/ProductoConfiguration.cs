using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Descripcion)
                .HasMaxLength(1000);

            builder.Property(p => p.Precio)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.ImagenUrl)
                .HasMaxLength(500);

            // Relación con Categoría (OBLIGATORIA)
            builder.HasOne(p => p.CategoriaProducto)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaProductoId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar categoría con productos

            // Índices para búsquedas
            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Productos_Nombre");

            builder.HasIndex(p => p.CategoriaProductoId)
                .HasDatabaseName("IX_Productos_CategoriaProductoId");

            builder.HasIndex(p => p.Activo)
                .HasDatabaseName("IX_Productos_Activo");
        }
    }
}