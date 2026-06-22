using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteSalesPointConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CondicionesComerciales",
                table: "PuntosVenta",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VigenciaCotizacionDias",
                table: "PuntosVenta",
                type: "int",
                nullable: false,
                defaultValue: 7);

            migrationBuilder.AddColumn<string>(
                name: "CondicionesSede",
                table: "Cotizaciones",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarcaSede",
                table: "Cotizaciones",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModalidadEntregaSede",
                table: "Cotizaciones",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreSede",
                table: "Cotizaciones",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlazoMaximoMesesSede",
                table: "Cotizaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PuntoVentaId",
                table: "Cotizaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TasaFactorMensualSede",
                table: "Cotizaciones",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VigenciaCotizacionDiasSede",
                table: "Cotizaciones",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE PuntosVenta
                SET VigenciaCotizacionDias = 7
                WHERE VigenciaCotizacionDias <= 0;

                UPDATE PuntosVenta
                SET CondicionesComerciales = 'Cotizacion sujeta a disponibilidad del producto, validacion comercial y aprobacion final.'
                WHERE CondicionesComerciales IS NULL;

                UPDATE c
                SET PuntoVentaId = pv.Id,
                    NombreSede = pv.Nombre,
                    MarcaSede = pv.MarcaPrincipal,
                    ModalidadEntregaSede = pv.ModalidadEntrega,
                    TasaFactorMensualSede = pv.TasaFactorMensual,
                    PlazoMaximoMesesSede = pv.PlazoMaximoMeses,
                    VigenciaCotizacionDiasSede = pv.VigenciaCotizacionDias,
                    CondicionesSede = pv.CondicionesComerciales,
                    FechaActualizacion = DATEADD(HOUR, -5, SYSUTCDATETIME()),
                    UsuarioActualizacion = 'migration'
                FROM Cotizaciones c
                OUTER APPLY (
                    SELECT TOP 1 p.*
                    FROM PuntosVenta p
                    WHERE p.EmpresaId = c.EmpresaId
                      AND p.Activa = 1
                    ORDER BY
                      CASE WHEN p.Codigo = 'PRINCIPAL' THEN 0 ELSE 1 END,
                      p.Nombre
                ) pv
                WHERE c.PuntoVentaId IS NULL
                  AND pv.Id IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_EmpresaId_PuntoVentaId",
                table: "Cotizaciones",
                columns: new[] { "EmpresaId", "PuntoVentaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_PuntoVentaId",
                table: "Cotizaciones",
                column: "PuntoVentaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_PuntosVenta_PuntoVentaId",
                table: "Cotizaciones",
                column: "PuntoVentaId",
                principalTable: "PuntosVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_PuntosVenta_PuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_EmpresaId_PuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_PuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "CondicionesComerciales",
                table: "PuntosVenta");

            migrationBuilder.DropColumn(
                name: "VigenciaCotizacionDias",
                table: "PuntosVenta");

            migrationBuilder.DropColumn(
                name: "CondicionesSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "MarcaSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "ModalidadEntregaSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NombreSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PlazoMaximoMesesSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PuntoVentaId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "TasaFactorMensualSede",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "VigenciaCotizacionDiasSede",
                table: "Cotizaciones");
        }
    }
}
