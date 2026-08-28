using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PermitirPromocionesEnVariasSedes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromocionesPuntosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromocionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocionesPuntosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromocionesPuntosVenta_Promociones_PromocionId",
                        column: x => x.PromocionId,
                        principalTable: "Promociones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromocionesPuntosVenta_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesPuntosVenta_EmpresaId",
                table: "PromocionesPuntosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesPuntosVenta_EmpresaId_PromocionId_PuntoVentaId",
                table: "PromocionesPuntosVenta",
                columns: new[] { "EmpresaId", "PromocionId", "PuntoVentaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesPuntosVenta_PromocionId",
                table: "PromocionesPuntosVenta",
                column: "PromocionId");

            migrationBuilder.CreateIndex(
                name: "IX_PromocionesPuntosVenta_PuntoVentaId",
                table: "PromocionesPuntosVenta",
                column: "PuntoVentaId");

            migrationBuilder.Sql("""
                INSERT INTO PromocionesPuntosVenta
                    (Id, PromocionId, PuntoVentaId, EmpresaId, FechaCreacion, FechaActualizacion, UsuarioCreacion, UsuarioActualizacion)
                SELECT
                    NEWID(), p.Id, p.PuntoVentaId, p.EmpresaId, p.FechaCreacion, p.FechaActualizacion, p.UsuarioCreacion, p.UsuarioActualizacion
                FROM Promociones p
                WHERE p.PuntoVentaId IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromocionesPuntosVenta");
        }
    }
}
