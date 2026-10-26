using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnvioCreditosAprobadosErp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ErpCreditExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Intentos = table.Column<int>(type: "int", nullable: false),
                    ProximoIntentoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnviadoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReciboErpId = table.Column<long>(type: "bigint", nullable: true),
                    UltimoError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErpCreditExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErpCreditExports_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCreditExports_EmpresaId",
                table: "ErpCreditExports",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ErpCreditExports_EmpresaId_SolicitudCreditoId",
                table: "ErpCreditExports",
                columns: new[] { "EmpresaId", "SolicitudCreditoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ErpCreditExports_Estado_ProximoIntentoUtc",
                table: "ErpCreditExports",
                columns: new[] { "Estado", "ProximoIntentoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ErpCreditExports_SolicitudCreditoId",
                table: "ErpCreditExports",
                column: "SolicitudCreditoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErpCreditExports");
        }
    }
}
