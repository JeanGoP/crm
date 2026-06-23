using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearModuloTramites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tramites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEstimada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Responsable = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Tercero = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    NotificarCliente = table.Column<bool>(type: "bit", nullable: false),
                    FechaNotificacionCliente = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tramites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tramites_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tramites_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tramites_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tramites_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_ClienteId",
                table: "Tramites",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_EmpresaId",
                table: "Tramites",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_EmpresaId_Estado_FechaEstimada",
                table: "Tramites",
                columns: new[] { "EmpresaId", "Estado", "FechaEstimada" });

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_EmpresaId_Numero",
                table: "Tramites",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_EmpresaId_PuntoVentaId_Tipo",
                table: "Tramites",
                columns: new[] { "EmpresaId", "PuntoVentaId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_ProductoId",
                table: "Tramites",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_PuntoVentaId",
                table: "Tramites",
                column: "PuntoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Tramites_SolicitudCreditoId",
                table: "Tramites",
                column: "SolicitudCreditoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tramites");
        }
    }
}
