using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CodeudoresMultiplesPrimerVencimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPrimerVencimiento",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodeudorId",
                table: "DocumentosSolicitudCredito",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodeudoresSolicitudCredito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SolicitudCreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identificacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Celular = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parentesco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngresosMensuales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Referencia1Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia1Celular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia1Relacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia2Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia2Celular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referencia2Relacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeudoresSolicitudCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeudoresSolicitudCredito_SolicitudesCredito_SolicitudCreditoId",
                        column: x => x.SolicitudCreditoId,
                        principalTable: "SolicitudesCredito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosSolicitudCredito_CodeudorId",
                table: "DocumentosSolicitudCredito",
                column: "CodeudorId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeudoresSolicitudCredito_EmpresaId",
                table: "CodeudoresSolicitudCredito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeudoresSolicitudCredito_SolicitudCreditoId",
                table: "CodeudoresSolicitudCredito",
                column: "SolicitudCreditoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosSolicitudCredito_CodeudoresSolicitudCredito_CodeudorId",
                table: "DocumentosSolicitudCredito",
                column: "CodeudorId",
                principalTable: "CodeudoresSolicitudCredito",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Existing uploads keep their original client ownership. Never guess the owner of a file.
            migrationBuilder.Sql("""
                INSERT INTO CodeudoresSolicitudCredito
                    (Id, SolicitudCreditoId, EmpresaId, Nombre, Identificacion, Celular, Parentesco,
                     IngresosMensuales, Referencia1Nombre, Referencia1Celular, Referencia1Relacion,
                     Referencia2Nombre, Referencia2Celular, Referencia2Relacion, Activo, Orden,
                     FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), Id, EmpresaId, CodeudorNombre, COALESCE(CodeudorIdentificacion, N''),
                    COALESCE(CodeudorCelular, N''), CodeudorParentesco, COALESCE(CodeudorIngresosMensuales, 0),
                    CodeudorReferencia1Nombre, CodeudorReferencia1Celular, CodeudorReferencia1Relacion,
                    CodeudorReferencia2Nombre, CodeudorReferencia2Celular, CodeudorReferencia2Relacion,
                    1, 0, FechaCreacion, UsuarioCreacion
                FROM SolicitudesCredito WHERE NULLIF(LTRIM(RTRIM(CodeudorNombre)), N'') IS NOT NULL;

                INSERT INTO DocumentosSolicitudCredito
                    (Id, EmpresaId, SolicitudCreditoId, CodeudorId, Tipo, Nombre, Estado, FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), p.EmpresaId, p.SolicitudCreditoId, p.Id, c.Tipo, c.Nombre, 1, GETUTCDATE(), N'migration'
                FROM CodeudoresSolicitudCredito p
                CROSS JOIN (VALUES
                    (N'Cédula de ciudadanía', 1), (N'Recibo de servicio público', 3),
                    (N'Carta laboral', 5), (N'Desprendibles de pago', 5),
                    (N'Certificado de libertad y tradición', 5), (N'Compraventa o escritura', 5),
                    (N'Tarjeta de propiedad', 5), (N'Sana posesión', 5),
                    (N'Extractos bancarios', 5), (N'Declaración de renta', 5), (N'RUT', 5),
                    (N'Certificado de vacunación de ganado', 5), (N'Cámara de comercio', 5),
                    (N'Registro de hierro', 5), (N'Otros', 5)
                ) c(Nombre, Tipo);

                UPDATE s SET DocumentacionCompleta = 0, FechaDocumentacionCompleta = NULL,
                    UsuarioDocumentacionCompleta = NULL, Estado = CASE WHEN Estado = 3 THEN 2 ELSE Estado END
                FROM SolicitudesCredito s
                WHERE EXISTS (SELECT 1 FROM CodeudoresSolicitudCredito c WHERE c.SolicitudCreditoId = s.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosSolicitudCredito_CodeudoresSolicitudCredito_CodeudorId",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropTable(
                name: "CodeudoresSolicitudCredito");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosSolicitudCredito_CodeudorId",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "FechaPrimerVencimiento",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "CodeudorId",
                table: "DocumentosSolicitudCredito");
        }
    }
}
