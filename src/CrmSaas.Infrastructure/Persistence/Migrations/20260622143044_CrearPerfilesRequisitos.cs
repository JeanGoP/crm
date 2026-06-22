using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearPerfilesRequisitos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PerfilRequisitoId",
                table: "SolicitudesCredito",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerfilRequisitoId",
                table: "Cotizaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PerfilesRequisito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsContado = table.Column<bool>(type: "bit", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesRequisito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosPerfilRequisito",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerfilRequisitoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Obligatorio = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosPerfilRequisito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosPerfilRequisito_PerfilesRequisito_PerfilRequisitoId",
                        column: x => x.PerfilRequisitoId,
                        principalTable: "PerfilesRequisito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_EmpresaId_PerfilRequisitoId",
                table: "SolicitudesCredito",
                columns: new[] { "EmpresaId", "PerfilRequisitoId" });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCredito_PerfilRequisitoId",
                table: "SolicitudesCredito",
                column: "PerfilRequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_EmpresaId_PerfilRequisitoId",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "PerfilRequisitoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PerfilRequisitoId",
                table: "Cotizaciones",
                column: "PerfilRequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPerfilRequisito_EmpresaId",
                table: "DocumentosPerfilRequisito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPerfilRequisito_EmpresaId_PerfilRequisitoId_Orden",
                table: "DocumentosPerfilRequisito",
                columns: new[] { "EmpresaId", "PerfilRequisitoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPerfilRequisito_PerfilRequisitoId",
                table: "DocumentosPerfilRequisito",
                column: "PerfilRequisitoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesRequisito_EmpresaId",
                table: "PerfilesRequisito",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesRequisito_EmpresaId_Codigo",
                table: "PerfilesRequisito",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.Sql("""
                DECLARE @now datetime2 = SYSUTCDATETIME();

                INSERT INTO PerfilesRequisito (Id, Nombre, Codigo, Descripcion, EsContado, Activo, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), v.Nombre, v.Codigo, v.Descripcion, v.EsContado, 1, e.Id, @now, 'migracion'
                FROM Empresas e
                CROSS JOIN (VALUES
                    ('Empleado', 'EMPLEADO', 'Cliente asalariado con soporte laboral e ingresos.', CAST(0 AS bit)),
                    ('Independiente', 'INDEPENDIENTE', 'Cliente independiente o comerciante.', CAST(0 AS bit)),
                    ('Pensionado', 'PENSIONADO', 'Cliente pensionado.', CAST(0 AS bit)),
                    ('Contado', 'CONTADO', 'Compra de contado con documentos minimos.', CAST(1 AS bit))
                ) v(Nombre, Codigo, Descripcion, EsContado)
                WHERE NOT EXISTS (
                    SELECT 1 FROM PerfilesRequisito p WHERE p.EmpresaId = e.Id AND p.Codigo = v.Codigo
                );

                INSERT INTO DocumentosPerfilRequisito (Id, PerfilRequisitoId, Tipo, Nombre, Descripcion, Obligatorio, Orden, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT NEWID(), p.Id, d.Tipo, d.Nombre, NULL, 1, d.Orden, p.EmpresaId, @now, 'migracion'
                FROM PerfilesRequisito p
                CROSS APPLY (VALUES
                    (1, 'Fotocopia de cedula', 1, 'EMPLEADO'),
                    (2, 'Carta laboral o dos ultimas colillas de pago', 2, 'EMPLEADO'),
                    (3, 'Recibo de servicio publico', 3, 'EMPLEADO'),
                    (4, 'Referencias personales', 4, 'EMPLEADO'),
                    (1, 'Fotocopia de cedula', 1, 'INDEPENDIENTE'),
                    (2, 'Certificado de ingresos o camara de comercio', 2, 'INDEPENDIENTE'),
                    (2, 'Extractos bancarios', 3, 'INDEPENDIENTE'),
                    (3, 'Recibo de servicio publico', 4, 'INDEPENDIENTE'),
                    (4, 'Referencias comerciales o personales', 5, 'INDEPENDIENTE'),
                    (1, 'Fotocopia de cedula', 1, 'PENSIONADO'),
                    (2, 'Dos ultimas colillas de pension', 2, 'PENSIONADO'),
                    (3, 'Recibo de servicio publico', 3, 'PENSIONADO'),
                    (4, 'Referencias personales', 4, 'PENSIONADO'),
                    (1, 'Fotocopia de cedula', 1, 'CONTADO'),
                    (5, 'Soporte de pago', 2, 'CONTADO')
                ) d(Tipo, Nombre, Orden, Codigo)
                WHERE p.Codigo = d.Codigo
                  AND NOT EXISTS (
                    SELECT 1 FROM DocumentosPerfilRequisito existing
                    WHERE existing.PerfilRequisitoId = p.Id AND existing.Nombre = d.Nombre
                  );
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_PerfilesRequisito_PerfilRequisitoId",
                table: "Cotizaciones",
                column: "PerfilRequisitoId",
                principalTable: "PerfilesRequisito",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesCredito_PerfilesRequisito_PerfilRequisitoId",
                table: "SolicitudesCredito",
                column: "PerfilRequisitoId",
                principalTable: "PerfilesRequisito",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_PerfilesRequisito_PerfilRequisitoId",
                table: "Cotizaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesCredito_PerfilesRequisito_PerfilRequisitoId",
                table: "SolicitudesCredito");

            migrationBuilder.DropTable(
                name: "DocumentosPerfilRequisito");

            migrationBuilder.DropTable(
                name: "PerfilesRequisito");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCredito_EmpresaId_PerfilRequisitoId",
                table: "SolicitudesCredito");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesCredito_PerfilRequisitoId",
                table: "SolicitudesCredito");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_EmpresaId_PerfilRequisitoId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_PerfilRequisitoId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PerfilRequisitoId",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "PerfilRequisitoId",
                table: "Cotizaciones");
        }
    }
}
