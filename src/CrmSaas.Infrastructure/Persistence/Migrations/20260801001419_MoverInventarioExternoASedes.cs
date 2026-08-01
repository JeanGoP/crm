using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoverInventarioExternoASedes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseDatosInventarioExterno",
                table: "PuntosVenta",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodegasInventarioExterno",
                table: "PuntosVenta",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE pv
                SET
                    BaseDatosInventarioExterno = e.BaseDatosInventarioExterno,
                    BodegasInventarioExterno = e.BodegasInventarioExterno
                FROM PuntosVenta pv
                INNER JOIN Empresas e ON e.Id = pv.EmpresaId
                WHERE
                    (pv.BaseDatosInventarioExterno IS NULL OR pv.BaseDatosInventarioExterno = '')
                    AND (e.BaseDatosInventarioExterno IS NOT NULL OR e.BodegasInventarioExterno IS NOT NULL);
                """);

            migrationBuilder.DropColumn(
                name: "BaseDatosInventarioExterno",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "BodegasInventarioExterno",
                table: "Empresas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseDatosInventarioExterno",
                table: "Empresas",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodegasInventarioExterno",
                table: "Empresas",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE e
                SET
                    BaseDatosInventarioExterno = pv.BaseDatosInventarioExterno,
                    BodegasInventarioExterno = pv.BodegasInventarioExterno
                FROM Empresas e
                INNER JOIN (
                    SELECT
                        EmpresaId,
                        MAX(BaseDatosInventarioExterno) AS BaseDatosInventarioExterno,
                        MAX(BodegasInventarioExterno) AS BodegasInventarioExterno
                    FROM PuntosVenta
                    GROUP BY EmpresaId
                ) pv ON pv.EmpresaId = e.Id;
                """);

            migrationBuilder.DropColumn(
                name: "BaseDatosInventarioExterno",
                table: "PuntosVenta");

            migrationBuilder.DropColumn(
                name: "BodegasInventarioExterno",
                table: "PuntosVenta");
        }
    }
}
