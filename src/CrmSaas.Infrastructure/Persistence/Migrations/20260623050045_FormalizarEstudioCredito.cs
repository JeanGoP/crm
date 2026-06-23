using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FormalizarEstudioCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CondicionesFinales",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicialAprobada",
                table: "SolicitudesCredito",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaMensualAprobada",
                table: "SolicitudesCredito",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionPaso0",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IdentidadValidada",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPaso0",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlazoAprobadoMeses",
                table: "SolicitudesCredito",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereCodeudorParaAprobar",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResultadoEstudio",
                table: "SolicitudesCredito",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RuntConsultado",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SimitConsultado",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioPaso0",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorAprobadoAnalista",
                table: "SolicitudesCredito",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CondicionesFinales",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CuotaInicialAprobada",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CuotaMensualAprobada",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaRevisionPaso0",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "IdentidadValidada",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionPaso0",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "PlazoAprobadoMeses",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "RequiereCodeudorParaAprobar",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ResultadoEstudio",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "RuntConsultado",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "SimitConsultado",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioPaso0",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ValorAprobadoAnalista",
                table: "SolicitudesCredito");
        }
    }
}
