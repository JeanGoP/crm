using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitCustomerNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimerApellido",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimerNombre",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SegundoApellido",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SegundoNombre",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimerApellidoCliente",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimerNombreCliente",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SegundoApellidoCliente",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SegundoNombreCliente",
                table: "Cotizaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimerApellido",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimerNombre",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SegundoApellido",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SegundoNombre",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Clientes
                SET
                    PrimerNombre = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Nombres))) > 0 THEN LEFT(LTRIM(RTRIM(Nombres)), CHARINDEX(' ', LTRIM(RTRIM(Nombres))) - 1) ELSE LTRIM(RTRIM(Nombres)) END,
                    SegundoNombre = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Nombres))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(Nombres)), CHARINDEX(' ', LTRIM(RTRIM(Nombres))) + 1, 4000)) ELSE '' END, ''),
                    PrimerApellido = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) > 0 THEN LEFT(LTRIM(RTRIM(Apellidos)), CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) - 1) ELSE LTRIM(RTRIM(Apellidos)) END,
                    SegundoApellido = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(Apellidos)), CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) + 1, 4000)) ELSE '' END, '')
                WHERE (PrimerNombre = '' OR PrimerApellido = '');
                """);

            migrationBuilder.Sql("""
                UPDATE Prospectos
                SET
                    PrimerNombre = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Nombres))) > 0 THEN LEFT(LTRIM(RTRIM(Nombres)), CHARINDEX(' ', LTRIM(RTRIM(Nombres))) - 1) ELSE LTRIM(RTRIM(Nombres)) END,
                    SegundoNombre = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Nombres))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(Nombres)), CHARINDEX(' ', LTRIM(RTRIM(Nombres))) + 1, 4000)) ELSE '' END, ''),
                    PrimerApellido = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) > 0 THEN LEFT(LTRIM(RTRIM(Apellidos)), CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) - 1) ELSE LTRIM(RTRIM(Apellidos)) END,
                    SegundoApellido = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(Apellidos)), CHARINDEX(' ', LTRIM(RTRIM(Apellidos))) + 1, 4000)) ELSE '' END, '')
                WHERE (PrimerNombre = '' OR PrimerApellido = '');
                """);

            migrationBuilder.Sql("""
                UPDATE Cotizaciones
                SET
                    PrimerNombreCliente = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(NombresCliente))) > 0 THEN LEFT(LTRIM(RTRIM(NombresCliente)), CHARINDEX(' ', LTRIM(RTRIM(NombresCliente))) - 1) ELSE LTRIM(RTRIM(NombresCliente)) END,
                    SegundoNombreCliente = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(NombresCliente))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(NombresCliente)), CHARINDEX(' ', LTRIM(RTRIM(NombresCliente))) + 1, 4000)) ELSE '' END, ''),
                    PrimerApellidoCliente = CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(ApellidosCliente))) > 0 THEN LEFT(LTRIM(RTRIM(ApellidosCliente)), CHARINDEX(' ', LTRIM(RTRIM(ApellidosCliente))) - 1) ELSE LTRIM(RTRIM(ApellidosCliente)) END,
                    SegundoApellidoCliente = NULLIF(CASE WHEN CHARINDEX(' ', LTRIM(RTRIM(ApellidosCliente))) > 0 THEN LTRIM(SUBSTRING(LTRIM(RTRIM(ApellidosCliente)), CHARINDEX(' ', LTRIM(RTRIM(ApellidosCliente))) + 1, 4000)) ELSE '' END, '')
                WHERE (PrimerNombreCliente = '' OR PrimerApellidoCliente = '');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimerApellido",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "PrimerNombre",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "SegundoApellido",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "SegundoNombre",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "PrimerApellidoCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PrimerNombreCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "SegundoApellidoCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "SegundoNombreCliente",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "PrimerApellido",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "PrimerNombre",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "SegundoApellido",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "SegundoNombre",
                table: "Clientes");
        }
    }
}
