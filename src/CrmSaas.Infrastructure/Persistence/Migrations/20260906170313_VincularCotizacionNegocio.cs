using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VincularCotizacionNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NegocioId",
                table: "Cotizaciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_NegocioId",
                table: "Cotizaciones",
                column: "NegocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizaciones_Negocios_NegocioId",
                table: "Cotizaciones",
                column: "NegocioId",
                principalTable: "Negocios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizaciones_Negocios_NegocioId",
                table: "Cotizaciones");

            migrationBuilder.DropIndex(
                name: "IX_Cotizaciones_NegocioId",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "NegocioId",
                table: "Cotizaciones");
        }
    }
}
