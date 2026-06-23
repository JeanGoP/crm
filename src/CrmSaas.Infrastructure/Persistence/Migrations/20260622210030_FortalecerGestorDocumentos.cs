using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FortalecerGestorDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClienteId",
                table: "DocumentosSolicitudCredito",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRechazo",
                table: "DocumentosSolicitudCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaValidacion",
                table: "DocumentosSolicitudCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "DocumentosSolicitudCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRechazo",
                table: "DocumentosSolicitudCredito",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioValidacion",
                table: "DocumentosSolicitudCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE d
                SET ClienteId = s.ClienteId
                FROM DocumentosSolicitudCredito d
                INNER JOIN SolicitudesCredito s ON s.Id = d.SolicitudCreditoId
                WHERE d.ClienteId IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE DocumentosSolicitudCredito
                SET FechaVencimiento = DATEADD(day,
                    CASE
                        WHEN Tipo = 1 THEN 365
                        WHEN Tipo = 2 THEN 30
                        WHEN Tipo = 3 THEN 60
                        ELSE 0
                    END,
                    CAST(COALESCE(FechaRecepcion, FechaCarga, FechaCreacion, GETDATE()) AS date))
                WHERE FechaVencimiento IS NULL
                    AND Tipo IN (1, 2, 3);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_ClienteId",
                table: "DocumentosSolicitudCredito",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_EmpresaId_ClienteId_Tipo",
                table: "DocumentosSolicitudCredito",
                columns: new[] { "EmpresaId", "ClienteId", "Tipo" });

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosSolicitudCredito_Clientes_ClienteId",
                table: "DocumentosSolicitudCredito",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosSolicitudCredito_Clientes_ClienteId",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosSolicitudCredito_ClienteId",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosSolicitudCredito_EmpresaId_ClienteId_Tipo",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "FechaRechazo",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "FechaValidacion",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "MotivoRechazo",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioValidacion",
                table: "DocumentosSolicitudCredito");
        }
    }
}
