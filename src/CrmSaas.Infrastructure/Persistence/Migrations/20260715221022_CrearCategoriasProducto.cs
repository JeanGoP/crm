using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearCategoriasProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasProducto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CotizarComoPaquete = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProducto", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT INTO CategoriasProducto
                    (Id, Nombre, Descripcion, CotizarComoPaquete, Activa, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT
                    NEWID(),
                    source.Nombre,
                    source.Descripcion,
                    source.CotizarComoPaquete,
                    1,
                    source.EmpresaId,
                    DATEADD(HOUR, -5, SYSUTCDATETIME()),
                    'migracion'
                FROM (
                    SELECT DISTINCT
                        EmpresaId,
                        NULLIF(LTRIM(RTRIM(Categoria)), '') AS Nombre,
                        CAST(NULL AS nvarchar(400)) AS Descripcion,
                        CAST(CASE WHEN LTRIM(RTRIM(Categoria)) LIKE '%Electrodom%' THEN 1 ELSE 0 END AS bit) AS CotizarComoPaquete
                    FROM Productos
                    WHERE NULLIF(LTRIM(RTRIM(Categoria)), '') IS NOT NULL

                    UNION

                    SELECT
                        Id AS EmpresaId,
                        'Moto' AS Nombre,
                        'Categoria principal para motos y vehiculos.' AS Descripcion,
                        CAST(0 AS bit) AS CotizarComoPaquete
                    FROM Empresas

                    UNION

                    SELECT
                        Id AS EmpresaId,
                        'Electrodomesticos' AS Nombre,
                        'Categoria para cotizar varios articulos como un solo paquete.' AS Descripcion,
                        CAST(1 AS bit) AS CotizarComoPaquete
                    FROM Empresas
                ) source
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM CategoriasProducto existing
                    WHERE existing.EmpresaId = source.EmpresaId
                      AND existing.Nombre = source.Nombre
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProducto_EmpresaId",
                table: "CategoriasProducto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProducto_EmpresaId_Nombre",
                table: "CategoriasProducto",
                columns: new[] { "EmpresaId", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriasProducto");
        }
    }
}
