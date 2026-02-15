using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PastisserieAPI.Core.Entities;

namespace PastisserieAPI.Infrastructure.Data.Configurations
{
    public class UserRolConfiguration : IEntityTypeConfiguration<UserRol>
    {
        public void Configure(EntityTypeBuilder<UserRol> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => ur.Id);

            // Relaciones
            builder.HasOne(ur => ur.Usuario)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Rol)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice compuesto para evitar roles duplicados
            builder.HasIndex(ur => new { ur.UsuarioId, ur.RolId })
                .IsUnique()
                .HasDatabaseName("IX_UserRoles_UsuarioId_RolId");
        }
    }
}