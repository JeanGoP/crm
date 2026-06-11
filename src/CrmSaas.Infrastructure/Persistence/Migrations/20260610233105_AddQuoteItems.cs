using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Productos_ProductoId",
                table: "Cotizaciones");

            migrationBuilder.CreateTable(
                name: "CotizacionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    PrecioProducto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CuotaInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Seguro = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GastosAdministrativos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlazoMeses = table.Column<int>(type: "int", nullable: false),
                    TasaInteresMensual = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    ValorFinanciado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CuotaMensualEstimada = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPagarEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoCredito = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UsoConfiguracionFinancieraEmpresa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotizacionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CotizacionItems_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CotizacionItems_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_CotizacionId",
                table: "CotizacionItems",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_EmpresaId",
                table: "CotizacionItems",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_EmpresaId_CotizacionId",
                table: "CotizacionItems",
                columns: new[] { "EmpresaId", "CotizacionId" });

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_ProductoId",
                table: "CotizacionItems",
                column: "ProductoId");

            migrationBuilder.Sql("""
                INSERT INTO CotizacionItems
                (
                    Id,
                    CotizacionId,
                    ProductoId,
                    Orden,
                    PrecioProducto,
                    CuotaInicial,
                    Seguro,
                    GastosAdministrativos,
                    PlazoMeses,
                    TasaInteresMensual,
                    ValorFinanciado,
                    CuotaMensualEstimada,
                    TotalPagarEstimado,
                    TipoCredito,
                    UsoConfiguracionFinancieraEmpresa,
                    EmpresaId,
                    FechaCreacion,
                    FechaActualizacion,
                    UsuarioCreacion,
                    UsuarioActualizacion
                )
                SELECT
                    NEWID(),
                    Id,
                    ProductoId,
                    1,
                    PrecioProducto,
                    CuotaInicial,
                    Seguro,
                    GastosAdministrativos,
                    PlazoMeses,
                    TasaInteresMensual,
                    ValorFinanciado,
                    CuotaMensualEstimada,
                    TotalPagarEstimado,
                    TipoCredito,
                    UsoConfiguracionFinancieraEmpresa,
                    EmpresaId,
                    FechaCreacion,
                    FechaActualizacion,
                    UsuarioCreacion,
                    UsuarioActualizacion
                FROM Cotizaciones
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM CotizacionItems
                    WHERE CotizacionItems.CotizacionId = Cotizaciones.Id
                );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Productos_ProductoId",
                table: "Cotizaciones",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Productos_ProductoId",
                table: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "CotizacionItems");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Productos_ProductoId",
                table: "Cotizaciones",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
