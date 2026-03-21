using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastisserieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSocialMediaNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhatsApp",
                table: "ConfiguracionTienda",
                newName: "WhatsappUrl");

            migrationBuilder.RenameColumn(
                name: "Instagram",
                table: "ConfiguracionTienda",
                newName: "InstagramUrl");

            migrationBuilder.RenameColumn(
                name: "Facebook",
                table: "ConfiguracionTienda",
                newName: "FacebookUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WhatsappUrl",
                table: "ConfiguracionTienda",
                newName: "WhatsApp");

            migrationBuilder.RenameColumn(
                name: "InstagramUrl",
                table: "ConfiguracionTienda",
                newName: "Instagram");

            migrationBuilder.RenameColumn(
                name: "FacebookUrl",
                table: "ConfiguracionTienda",
                newName: "Facebook");
        }
    }
}
