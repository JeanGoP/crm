using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeleccionEmpresaErp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ErpEmpresaId",
                table: "Empresas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpEmpresaNit",
                table: "Empresas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpEmpresaNombre",
                table: "Empresas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_ErpEmpresaId",
                table: "Empresas",
                column: "ErpEmpresaId",
                unique: true,
                filter: "[ErpEmpresaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Empresas_ErpEmpresaId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ErpEmpresaId",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ErpEmpresaNit",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ErpEmpresaNombre",
                table: "Empresas");
        }
    }
}
