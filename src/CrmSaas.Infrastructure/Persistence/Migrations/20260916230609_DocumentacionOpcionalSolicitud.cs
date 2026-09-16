using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentacionOpcionalSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DocumentacionCompleta",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDocumentacionCompleta",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioDocumentacionCompleta",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            // Preserve all existing documents and files; only add missing catalog entries.
            migrationBuilder.Sql("""
                INSERT INTO DocumentosSolicitudCredito
                    (Id, EmpresaId, SolicitudCreditoId, ClienteId, Tipo, Nombre, Estado, FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), s.EmpresaId, s.Id, s.ClienteId, c.Tipo, c.Nombre, 1, GETUTCDATE(), N'migration'
                FROM SolicitudesCredito s
                CROSS JOIN (VALUES
                    (N'Cédula de ciudadanía', 1), (N'Recibo de servicio público', 3),
                    (N'Carta laboral', 5), (N'Desprendibles de pago', 5),
                    (N'Certificado de libertad y tradición', 5), (N'Compraventa o escritura', 5),
                    (N'Tarjeta de propiedad', 5), (N'Sana posesión', 5),
                    (N'Extractos bancarios', 5), (N'Declaración de renta', 5), (N'RUT', 5),
                    (N'Certificado de vacunación de ganado', 5), (N'Cámara de comercio', 5),
                    (N'Registro de hierro', 5), (N'Otros', 5)
                ) c(Nombre, Tipo)
                WHERE NOT EXISTS (
                    SELECT 1 FROM DocumentosSolicitudCredito d
                    WHERE d.SolicitudCreditoId = s.Id AND d.EmpresaId = s.EmpresaId
                      AND d.Nombre COLLATE Latin1_General_100_BIN2 = c.Nombre COLLATE Latin1_General_100_BIN2
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentacionCompleta",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaDocumentacionCompleta",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioDocumentacionCompleta",
                table: "SolicitudesCredito");
        }
    }
}
