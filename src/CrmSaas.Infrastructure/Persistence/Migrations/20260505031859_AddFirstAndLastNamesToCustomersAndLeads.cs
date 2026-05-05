using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstAndLastNamesToCustomersAndLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Prospectos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE Clientes SET Nombres = Nombre WHERE Nombres = ''");
            migrationBuilder.Sql("UPDATE Prospectos SET Nombres = Nombre WHERE Nombres = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Prospectos");

            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Clientes");
        }
    }
}
