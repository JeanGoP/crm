using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCoDebtorReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeudorCelular",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorIdentificacion",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CodeudorIngresosMensuales",
                table: "SolicitudesCredito",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorNombre",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorParentesco",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia1Celular",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia1Nombre",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia1Relacion",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia2Celular",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia2Nombre",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia2Relacion",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeudorCelular",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorIdentificacion",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorIngresosMensuales",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorNombre",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorParentesco",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia1Celular",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia1Nombre",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia1Relacion",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia2Celular",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia2Nombre",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "Referencia2Relacion",
                table: "SolicitudesCredito");
        }
    }
}
