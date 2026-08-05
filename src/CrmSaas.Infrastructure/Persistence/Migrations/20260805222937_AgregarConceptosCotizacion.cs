using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConceptosCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptosCotizacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GrupoCalculo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FuenteValor = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ValorPredeterminado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosCotizacion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosCotizacion_EmpresaId",
                table: "ConceptosCotizacion",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosCotizacion_EmpresaId_Activo_Orden",
                table: "ConceptosCotizacion",
                columns: new[] { "EmpresaId", "Activo", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosCotizacion_EmpresaId_Codigo",
                table: "ConceptosCotizacion",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptosCotizacion");
        }
    }
}
