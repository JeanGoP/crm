using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPreciosProductoPorSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductoPreciosSede",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoPreciosSede", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoPreciosSede_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoPreciosSede_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPreciosSede_EmpresaId",
                table: "ProductoPreciosSede",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPreciosSede_EmpresaId_ProductoId_PuntoVentaId",
                table: "ProductoPreciosSede",
                columns: new[] { "EmpresaId", "ProductoId", "PuntoVentaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPreciosSede_EmpresaId_PuntoVentaId",
                table: "ProductoPreciosSede",
                columns: new[] { "EmpresaId", "PuntoVentaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPreciosSede_ProductoId",
                table: "ProductoPreciosSede",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPreciosSede_PuntoVentaId",
                table: "ProductoPreciosSede",
                column: "PuntoVentaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductoPreciosSede");
        }
    }
}
