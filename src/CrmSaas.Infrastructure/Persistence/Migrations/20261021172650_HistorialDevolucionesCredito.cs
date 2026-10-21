using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HistorialDevolucionesCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevolucionesSolicitudCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaDevolucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioDevolucion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    FechaReenvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioReenvio = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesSolicitudCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevolucionesSolicitudCredito_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesSolicitudCredito_EmpresaId",
                table: "DevolucionesSolicitudCredito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesSolicitudCredito_SolicitudCreditoId",
                table: "DevolucionesSolicitudCredito",
                column: "SolicitudCreditoId");

            migrationBuilder.Sql("""
                INSERT INTO DevolucionesSolicitudCredito
                    (Id, SolicitudCreditoId, Motivo, FechaDevolucion, UsuarioDevolucion,
                     FechaReenvio, UsuarioReenvio, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), Id, LEFT(MotivoDevolucion, 1000), FechaDevolucion,
                       LEFT(COALESCE(NULLIF(UsuarioDevolucion, ''), 'Sistema'), 180),
                       CASE WHEN FechaDocumentacionCompleta > FechaDevolucion THEN FechaDocumentacionCompleta ELSE NULL END,
                       CASE WHEN FechaDocumentacionCompleta > FechaDevolucion THEN UsuarioDocumentacionCompleta ELSE NULL END,
                       EmpresaId, FechaDevolucion,
                       LEFT(COALESCE(NULLIF(UsuarioDevolucion, ''), 'Sistema'), 180)
                FROM SolicitudesCredito
                WHERE FechaDevolucion IS NOT NULL AND MotivoDevolucion IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucionesSolicitudCredito");
        }
    }
}
