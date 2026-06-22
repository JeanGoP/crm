using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuntosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarcaPrincipal = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LogoMarcaDataUrl = table.Column<string>(type: "nvarchar(max)", maxLength: 300000, nullable: true),
                    TasaFactorMensual = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    PlazoMaximoMeses = table.Column<int>(type: "int", nullable: false),
                    ModalidadEntrega = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TiempoSoatDias = table.Column<int>(type: "int", nullable: false),
                    TiempoMatriculaDias = table.Column<int>(type: "int", nullable: false),
                    ProveedorSoat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TramitadorMatricula = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuntosVenta", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuntosVenta_EmpresaId",
                table: "PuntosVenta",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_PuntosVenta_EmpresaId_Ciudad",
                table: "PuntosVenta",
                columns: new[] { "EmpresaId", "Ciudad" });

            migrationBuilder.CreateIndex(
                name: "IX_PuntosVenta_EmpresaId_Codigo",
                table: "PuntosVenta",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuntosVenta");
        }
    }
}
