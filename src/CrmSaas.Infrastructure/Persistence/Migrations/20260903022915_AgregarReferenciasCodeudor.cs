using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarReferenciasCodeudor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia1Celular",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia1Nombre",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia1Relacion",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia2Celular",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia2Nombre",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeudorReferencia2Relacion",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeudorReferencia1Celular",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorReferencia1Nombre",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorReferencia1Relacion",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorReferencia2Celular",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorReferencia2Nombre",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorReferencia2Relacion",
                table: "SolicitudesCredito");
        }
    }
}
