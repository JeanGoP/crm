using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeguimientoProcesoCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BienvenidaCompletada",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DataCreditoClienteConsultado",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DataCreditoCodeudorConsultado",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DataCreditoPuntajeCliente",
                table: "SolicitudesCredito",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DataCreditoPuntajeCodeudor",
                table: "SolicitudesCredito",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaBienvenida",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFirmasCompletas",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionDataCredito",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRevisionFinal",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FirmasCompletas",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionBienvenida",
                table: "SolicitudesCredito",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionDataCredito",
                table: "SolicitudesCredito",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionFirmas",
                table: "SolicitudesCredito",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionRevisionFinal",
                table: "SolicitudesCredito",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RevisionFinalAprobada",
                table: "SolicitudesCredito",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioBienvenida",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioDataCredito",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioFirmas",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioRevisionFinal",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BienvenidaCompletada",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "DataCreditoClienteConsultado",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "DataCreditoCodeudorConsultado",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "DataCreditoPuntajeCliente",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "DataCreditoPuntajeCodeudor",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaBienvenida",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaFirmasCompletas",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaRevisionDataCredito",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaRevisionFinal",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FirmasCompletas",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionBienvenida",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionDataCredito",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionFirmas",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionRevisionFinal",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "RevisionFinalAprobada",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioBienvenida",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioDataCredito",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioFirmas",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioRevisionFinal",
                table: "SolicitudesCredito");
        }
    }
}
