using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarMaestroProductos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FichaTecnica",
                table: "Productos",
                type: "nvarchar(1600)",
                maxLength: 1600,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Impuestos",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Linea",
                table: "Productos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Matricula",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Soat",
                table: "Productos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Productos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VigenteDesde",
                table: "Productos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FichaTecnica",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Impuestos",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Linea",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Matricula",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Soat",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "VigenteDesde",
                table: "Productos");
        }
    }
}
