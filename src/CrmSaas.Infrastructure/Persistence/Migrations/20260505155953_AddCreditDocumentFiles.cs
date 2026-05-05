using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditDocumentFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "DocumentosSolicitudCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCarga",
                table: "DocumentosSolicitudCredito",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreArchivo",
                table: "DocumentosSolicitudCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaArchivo",
                table: "DocumentosSolicitudCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TamanoBytes",
                table: "DocumentosSolicitudCredito",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "FechaCarga",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "NombreArchivo",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "RutaArchivo",
                table: "DocumentosSolicitudCredito");

            migrationBuilder.DropColumn(
                name: "TamanoBytes",
                table: "DocumentosSolicitudCredito");
        }
    }
}
