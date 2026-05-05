using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitudesCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CotizacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NegocioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoIdentificacion = table.Column<int>(type: "int", nullable: false),
                    NumeroIdentificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Celular = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ciudad = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ocupacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngresosMensuales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CuotaInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlazoMeses = table.Column<int>(type: "int", nullable: false),
                    ValorMoto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_SolicitudesCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCredito_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitudesCredito_Cotizaciones_CotizacionId",
                        column: x => x.CotizacionId,
                        principalTable: "Cotizaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesCredito_Negocios_NegocioId",
                        column: x => x.NegocioId,
                        principalTable: "Negocios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesCredito_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosSolicitudCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosSolicitudCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosSolicitudCredito_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_EmpresaId",
                table: "DocumentosSolicitudCredito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_EmpresaId_SolicitudCreditoId_Tipo",
                table: "DocumentosSolicitudCredito",
                columns: new[] { "EmpresaId", "SolicitudCreditoId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_SolicitudCreditoId",
                table: "DocumentosSolicitudCredito",
                column: "SolicitudCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_ClienteId",
                table: "SolicitudesCredito",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_CotizacionId",
                table: "SolicitudesCredito",
                column: "CotizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_EmpresaId",
                table: "SolicitudesCredito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_EmpresaId_Numero",
                table: "SolicitudesCredito",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_NegocioId",
                table: "SolicitudesCredito",
                column: "NegocioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_ProductoId",
                table: "SolicitudesCredito",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosSolicitudCredito");

            migrationBuilder.DropTable(
                name: "SolicitudesCredito");
        }
    }
}
