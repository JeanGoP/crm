using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMotorcycleDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EntregasMoto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AsesorResponsable = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Vin = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NumeroChasis = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NumeroMotor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KilometrajeEntrega = table.Column<int>(type: "int", nullable: true),
                    CascoEntregado = table.Column<bool>(type: "bit", nullable: false),
                    SoatEntregado = table.Column<bool>(type: "bit", nullable: false),
                    MatriculaEntregada = table.Column<bool>(type: "bit", nullable: false),
                    ManualGarantiaEntregado = table.Column<bool>(type: "bit", nullable: false),
                    ActaEntregaFirmada = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_EntregasMoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntregasMoto_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregasMoto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntregasMoto_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_ClienteId",
                table: "EntregasMoto",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_EmpresaId",
                table: "EntregasMoto",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_EmpresaId_Numero",
                table: "EntregasMoto",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_EmpresaId_SolicitudCreditoId",
                table: "EntregasMoto",
                columns: new[] { "EmpresaId", "SolicitudCreditoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_ProductoId",
                table: "EntregasMoto",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_SolicitudCreditoId",
                table: "EntregasMoto",
                column: "SolicitudCreditoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntregasMoto");
        }
    }
}
