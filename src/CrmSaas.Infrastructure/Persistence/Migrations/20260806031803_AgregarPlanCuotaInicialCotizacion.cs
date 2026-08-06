using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPlanCuotaInicialCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicialPagadaHoy",
                table: "CotizacionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioCreditoEstimada",
                table: "CotizacionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanCuotaInicialJson",
                table: "CotizacionItems",
                type: "nvarchar(1600)",
                maxLength: 1600,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicialPagadaHoy",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioCreditoEstimada",
                table: "Cotizaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanCuotaInicialJson",
                table: "Cotizaciones",
                type: "nvarchar(1600)",
                maxLength: 1600,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuotaInicialPagadaHoy",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "FechaInicioCreditoEstimada",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "PlanCuotaInicialJson",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "CuotaInicialPagadaHoy",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "FechaInicioCreditoEstimada",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PlanCuotaInicialJson",
                table: "Cotizaciones");
        }
    }
}
