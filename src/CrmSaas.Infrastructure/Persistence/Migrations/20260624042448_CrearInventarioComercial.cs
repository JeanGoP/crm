using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearInventarioComercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventarioComercial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Vin = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NumeroChasis = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NumeroMotor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EsUsada = table.Column<bool>(type: "bit", nullable: false),
                    Kilometraje = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ClienteReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CotizacionReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SolicitudCreditoReservaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimientoReserva = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioComercial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioComercial_Clientes_ClienteReservaId",
                        column: x => x.ClienteReservaId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventarioComercial_Cotizaciones_CotizacionReservaId",
                        column: x => x.CotizacionReservaId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventarioComercial_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventarioComercial_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventarioComercial_SolicitudesCredito_SolicitudCreditoReservaId",
                        column: x => x.SolicitudCreditoReservaId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_ClienteReservaId",
                table: "InventarioComercial",
                column: "ClienteReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_CotizacionReservaId",
                table: "InventarioComercial",
                column: "CotizacionReservaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId",
                table: "InventarioComercial",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId_FechaVencimientoReserva",
                table: "InventarioComercial",
                columns: new[] { "EmpresaId", "FechaVencimientoReserva" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId_NumeroChasis",
                table: "InventarioComercial",
                columns: new[] { "EmpresaId", "NumeroChasis" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId_NumeroMotor",
                table: "InventarioComercial",
                columns: new[] { "EmpresaId", "NumeroMotor" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId_Placa",
                table: "InventarioComercial",
                columns: new[] { "EmpresaId", "Placa" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_EmpresaId_PuntoVentaId_ProductoId_Estado",
                table: "InventarioComercial",
                columns: new[] { "EmpresaId", "PuntoVentaId", "ProductoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_ProductoId",
                table: "InventarioComercial",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_PuntoVentaId",
                table: "InventarioComercial",
                column: "PuntoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioComercial_SolicitudCreditoReservaId",
                table: "InventarioComercial",
                column: "SolicitudCreditoReservaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventarioComercial");
        }
    }
}
