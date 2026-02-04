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

            builder.Property(p => p.Categoria)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.ImagenUrl)
                .HasMaxLength(500);

            // Índices
            builder.HasIndex(p => p.Categoria)
                .HasDatabaseName("IX_Productos_Categoria");

            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Productos_Nombre");
        }
    }
}