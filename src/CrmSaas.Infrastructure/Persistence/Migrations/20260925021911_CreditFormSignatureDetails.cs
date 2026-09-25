using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreditFormSignatureDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormDetailsJson",
                table: "SolicitudesCredito",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormDetailsJson",
                table: "CodeudoresSolicitudCredito",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormDetailsJson",
                table: "SolicitudesCredito");

            migrationBuilder.DropColumn(
                name: "FormDetailsJson",
                table: "CodeudoresSolicitudCredito");
        }
    }
}
