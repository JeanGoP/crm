using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GeneralizeProductsCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Productos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "Moto");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Productos",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Productos
                SET
                    Categoria = CASE WHEN NULLIF(LTRIM(RTRIM(Categoria)), '') IS NULL THEN 'Moto' ELSE Categoria END,
                    Nombre = COALESCE(NULLIF(LTRIM(RTRIM(CONCAT(Marca, ' ', Modelo, ' ', Referencia))), ''), Referencia, 'Producto')
                WHERE NULLIF(LTRIM(RTRIM(Nombre)), '') IS NULL
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EmpresaId_Categoria",
                table: "Productos",
                columns: new[] { "EmpresaId", "Categoria" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Productos_EmpresaId_Categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Productos");
        }
    }
}
