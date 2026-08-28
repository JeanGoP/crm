using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearTasasNombradasPorSede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreTasaSede",
                table: "Cotizaciones",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TasaPuntoVentaId",
                table: "Cotizaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TasasPuntosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TasaFactorMensual = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    PlazoMaximoMeses = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TasasPuntosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TasasPuntosVenta_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_TasaPuntoVentaId",
                table: "Cotizaciones",
                column: "TasaPuntoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_TasasPuntosVenta_EmpresaId",
                table: "TasasPuntosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TasasPuntosVenta_EmpresaId_PuntoVentaId_Nombre",
                table: "TasasPuntosVenta",
                columns: new[] { "EmpresaId", "PuntoVentaId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TasasPuntosVenta_PuntoVentaId",
                table: "TasasPuntosVenta",
                column: "PuntoVentaId");

            migrationBuilder.Sql("""
                INSERT INTO TasasPuntosVenta
                    (Id, PuntoVentaId, Nombre, TasaFactorMensual, PlazoMaximoMeses, Activa, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT
                    NEWID(), p.Id, 'Tasa general', p.TasaFactorMensual, p.PlazoMaximoMeses, 1, p.EmpresaId, p.FechaCreacion, p.UsuarioCreacion
                FROM PuntosVenta p;

                UPDATE c
                SET c.TasaPuntoVentaId = t.Id,
                    c.NombreTasaSede = t.Nombre
                FROM Cotizaciones c
                INNER JOIN TasasPuntosVenta t
                    ON t.PuntoVentaId = c.PuntoVentaId
                   AND t.Nombre = 'Tasa general';
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_TasasPuntosVenta_TasaPuntoVentaId",
                table: "Cotizaciones",
                column: "TasaPuntoVentaId",
                principalTable: "TasasPuntosVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_TasasPuntosVenta_TasaPuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "TasasPuntosVenta");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_TasaPuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NombreTasaSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TasaPuntoVentaId",
                table: "Cotizaciones");
        }
    }
}
