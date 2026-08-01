using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GuardarUnidadInventarioCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoBodegaInventario",
                table: "CotizacionItems",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreBodegaInventario",
                table: "CotizacionItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroChasisInventario",
                table: "CotizacionItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroMotorInventario",
                table: "CotizacionItems",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroSerieInventario",
                table: "CotizacionItems",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentacionInventario",
                table: "CotizacionItems",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoBodegaInventario",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "NombreBodegaInventario",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "NumeroChasisInventario",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "NumeroMotorInventario",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "NumeroSerieInventario",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "PresentacionInventario",
                table: "CotizacionItems");
        }
    }
}
