using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDesembolso",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvio",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioEstudio",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRechazo",
                table: "SolicitudesCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionDecision",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioDecision",
                table: "SolicitudesCredito",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaDesembolso",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaEnvio",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaInicioEstudio",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FechaRechazo",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "ObservacionDecision",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "UsuarioDecision",
                table: "SolicitudesCredito");
        }
    }
}
