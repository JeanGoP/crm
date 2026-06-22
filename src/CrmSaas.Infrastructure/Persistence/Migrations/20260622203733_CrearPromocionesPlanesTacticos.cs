using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearPromocionesPlanesTacticos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPromocion",
                table: "CotizacionItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NombrePromocion",
                table: "CotizacionItems",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromocionId",
                table: "CotizacionItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescuentoPromocion",
                table: "Cotizaciones",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NombrePromocion",
                table: "Cotizaciones",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromocionId",
                table: "Cotizaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TipoDescuento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ValorDescuento = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promociones_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Promociones_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CotizacionItems_PromocionId",
                table: "CotizacionItems",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_EmpresaId_PromocionId",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "PromocionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PromocionId",
                table: "Cotizaciones",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_EmpresaId",
                table: "Promociones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_EmpresaId_Activa_VigenteDesde_VigenteHasta",
                table: "Promociones",
                columns: new[] { "EmpresaId", "Activa", "VigenteDesde", "VigenteHasta" });

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_EmpresaId_Codigo",
                table: "Promociones",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_ProductoId",
                table: "Promociones",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_PuntoVentaId",
                table: "Promociones",
                column: "PuntoVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Promociones_PromocionId",
                table: "Cotizaciones",
                column: "PromocionId",
                principalTable: "Promociones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CotizacionItems_Promociones_PromocionId",
                table: "CotizacionItems",
                column: "PromocionId",
                principalTable: "Promociones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Promociones_PromocionId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_CotizacionItems_Promociones_PromocionId",
                table: "CotizacionItems");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropIndex(
                name: "IX_CotizacionItems_PromocionId",
                table: "CotizacionItems");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_EmpresaId_PromocionId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_PromocionId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "DescuentoPromocion",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "NombrePromocion",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "PromocionId",
                table: "CotizacionItems");

            migrationBuilder.DropColumn(
                name: "DescuentoPromocion",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NombrePromocion",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PromocionId",
                table: "Cotizaciones");
        }
    }
}
