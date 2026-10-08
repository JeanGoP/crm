using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations;

[DbContext(typeof(CrmDbContext))]
[Migration("20261007195000_LogoElectrodomesticosEmpresa")]
public sealed class LogoElectrodomesticosEmpresa : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<string>(
            name: "LogoElectrodomesticosDataUrl",
            table: "Empresas",
            type: "nvarchar(max)",
            maxLength: 300000,
            nullable: true);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "LogoElectrodomesticosDataUrl", table: "Empresas");
}
