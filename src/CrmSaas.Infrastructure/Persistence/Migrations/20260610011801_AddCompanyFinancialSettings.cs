using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyFinancialSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoCredito",
                table: "Cotizaciones",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsoConfiguracionFinancieraEmpresa",
                table: "Cotizaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesFinancierasEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalarioMinimoVigente = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaConsumoEa = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    TasaBajoMontoEa = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    TasaFactorMensual = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    PlazoMaximoMeses = table.Column<int>(type: "int", nullable: false),
                    RedondeoCuota = table.Column<int>(type: "int", nullable: false),
                    UsarTablaMontelibano = table.Column<bool>(type: "bit", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesFinancierasEmpresa", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesFinancierasEmpresa_EmpresaId",
                table: "ConfiguracionesFinancierasEmpresa",
                column: "EmpresaId",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO ConfiguracionesFinancierasEmpresa
                    (Id, SalarioMinimoVigente, TasaConsumoEa, TasaBajoMontoEa, TasaFactorMensual, PlazoMaximoMeses, RedondeoCuota, UsarTablaMontelibano, Activa, EmpresaId, FechaCreacion, UsuarioCreacion)
                SELECT
                    NEWID(), 1400000, 29.72, 56.33, 4.5, 30, 1000, 1, 1, e.Id, GETDATE(), 'migration'
                FROM Empresas e
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ConfiguracionesFinancierasEmpresa c
                    WHERE c.EmpresaId = e.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesFinancierasEmpresa");

            migrationBuilder.DropColumn(
                name: "TipoCredito",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "UsoConfiguracionFinancieraEmpresa",
                table: "Cotizaciones");
        }
    }
}
