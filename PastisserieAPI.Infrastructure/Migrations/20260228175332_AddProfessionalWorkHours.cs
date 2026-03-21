using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PastisserieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalWorkHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraApertura",
                table: "ConfiguracionTienda",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraCierre",
                table: "ConfiguracionTienda",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "SistemaActivoManual",
                table: "ConfiguracionTienda",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsarControlHorario",
                table: "ConfiguracionTienda",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoraApertura",
                table: "ConfiguracionTienda");

            migrationBuilder.DropColumn(
                name: "HoraCierre",
                table: "ConfiguracionTienda");

            migrationBuilder.DropColumn(
                name: "SistemaActivoManual",
                table: "ConfiguracionTienda");

            migrationBuilder.DropColumn(
                name: "UsarControlHorario",
                table: "ConfiguracionTienda");
        }
    }
}
