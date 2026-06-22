using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSalesPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PuntoVentaId",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE u
                SET PuntoVentaId = pv.Id,
                    FechaActualizacion = DATEADD(HOUR, -5, SYSUTCDATETIME()),
                    UsuarioActualizacion = 'migration'
                FROM Usuarios u
                OUTER APPLY (
                    SELECT TOP 1 p.Id
                    FROM PuntosVenta p
                    WHERE p.EmpresaId = u.EmpresaId
                      AND p.Activa = 1
                    ORDER BY
                      CASE WHEN p.Codigo = 'PRINCIPAL' THEN 0 ELSE 1 END,
                      p.Nombre
                ) pv
                WHERE u.PuntoVentaId IS NULL
                  AND pv.Id IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId_PuntoVentaId",
                table: "Usuarios",
                columns: new[] { "EmpresaId", "PuntoVentaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PuntoVentaId",
                table: "Usuarios",
                column: "PuntoVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_PuntosVenta_PuntoVentaId",
                table: "Usuarios",
                column: "PuntoVentaId",
                principalTable: "PuntosVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_PuntosVenta_PuntoVentaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmpresaId_PuntoVentaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PuntoVentaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "PuntoVentaId",
                table: "Usuarios");
        }
    }
}
