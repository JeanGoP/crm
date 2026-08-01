using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SepararBaseInventarioEmpresaYBodegasSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseDatosInventarioExterno",
                table: "Empresas",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE e
                SET BaseDatosInventarioExterno = pv.BaseDatosInventarioExterno
                FROM Empresas e
                INNER JOIN (
                    SELECT EmpresaId, MAX(BaseDatosInventarioExterno) AS BaseDatosInventarioExterno
                    FROM PuntosVenta
                    WHERE BaseDatosInventarioExterno IS NOT NULL AND BaseDatosInventarioExterno <> ''
                    GROUP BY EmpresaId
                ) pv ON pv.EmpresaId = e.Id
                WHERE e.BaseDatosInventarioExterno IS NULL OR e.BaseDatosInventarioExterno = '';
                """);

            migrationBuilder.DropColumn(
                name: "BaseDatosInventarioExterno",
                table: "PuntosVenta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseDatosInventarioExterno",
                table: "PuntosVenta",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE pv
                SET BaseDatosInventarioExterno = e.BaseDatosInventarioExterno
                FROM PuntosVenta pv
                INNER JOIN Empresas e ON e.Id = pv.EmpresaId
                WHERE (pv.BaseDatosInventarioExterno IS NULL OR pv.BaseDatosInventarioExterno = '')
                  AND e.BaseDatosInventarioExterno IS NOT NULL
                  AND e.BaseDatosInventarioExterno <> '';
                """);

            migrationBuilder.DropColumn(
                name: "BaseDatosInventarioExterno",
                table: "Empresas");
        }
    }
}
