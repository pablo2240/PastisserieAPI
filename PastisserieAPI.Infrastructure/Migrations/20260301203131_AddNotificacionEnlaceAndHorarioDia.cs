using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastisserieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificacionEnlaceAndHorarioDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Enlace",
                table: "Notificaciones",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HorariosPorDia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfiguracionTiendaId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Abierto = table.Column<bool>(type: "bit", nullable: false),
                    HoraApertura = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraCierre = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosPorDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosPorDia_ConfiguracionTienda_ConfiguracionTiendaId",
                        column: x => x.ConfiguracionTiendaId,
                        principalTable: "ConfiguracionTienda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosPorDia_ConfiguracionTiendaId",
                table: "HorariosPorDia",
                column: "ConfiguracionTiendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosPorDia");

            migrationBuilder.DropColumn(
                name: "Enlace",
                table: "Notificaciones");
        }
    }
}
