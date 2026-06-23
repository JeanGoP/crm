using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearOrdenesRecaudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenesRecaudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorPagado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesRecaudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesRecaudo_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesRecaudo_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesOrdenRecaudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrdenRecaudoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrdenRecaudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesOrdenRecaudo_OrdenesRecaudo_OrdenRecaudoId",
                        column: x => x.OrdenRecaudoId,
                        principalTable: "OrdenesRecaudo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenRecaudo_EmpresaId",
                table: "DetallesOrdenRecaudo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenRecaudo_EmpresaId_OrdenRecaudoId_Tipo",
                table: "DetallesOrdenRecaudo",
                columns: new[] { "EmpresaId", "OrdenRecaudoId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrdenRecaudo_OrdenRecaudoId",
                table: "DetallesOrdenRecaudo",
                column: "OrdenRecaudoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_ClienteId",
                table: "OrdenesRecaudo",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_EmpresaId",
                table: "OrdenesRecaudo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_EmpresaId_Estado_FechaVencimiento",
                table: "OrdenesRecaudo",
                columns: new[] { "EmpresaId", "Estado", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_EmpresaId_Numero",
                table: "OrdenesRecaudo",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_EmpresaId_SolicitudCreditoId",
                table: "OrdenesRecaudo",
                columns: new[] { "EmpresaId", "SolicitudCreditoId" });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesRecaudo_SolicitudCreditoId",
                table: "OrdenesRecaudo",
                column: "SolicitudCreditoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesOrdenRecaudo");

            migrationBuilder.DropTable(
                name: "OrdenesRecaudo");
        }
    }
}
