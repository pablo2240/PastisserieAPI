using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class CategoriaProductoConfiguration : IEntityTypeConfiguration<CategoriaProducto>
    {
        public void Configure(EntityTypeBuilder<CategoriaProducto> builder)
        {
            builder.ToTable("CategoriasProducto");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Descripcion)
                .HasMaxLength(500);

            // Índice único para nombre de categoría
            builder.HasIndex(c => c.Nombre)
                .IsUnique()
                .HasDatabaseName("IX_CategoriasProducto_Nombre");
        }
    }
}