using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteCreditSimulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CuotaInicial",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CuotaMensualEstimada",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PlazoMeses",
                table: "Cotizaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaInteresMensual",
                table: "Cotizaciones",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPagarEstimado",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFinanciado",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuotaInicial",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "CuotaMensualEstimada",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PlazoMeses",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TasaInteresMensual",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TotalPagarEstimado",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "ValorFinanciado",
                table: "Cotizaciones");
        }
    }
}
