using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FortalecerEntregas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActividadPrimeraRevisionId",
                table: "EntregasMoto",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChecklistPreEntregaCompletado",
                table: "EntregasMoto",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FotoEntregaDataUrl",
                table: "EntregasMoto",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoEntregaNombre",
                table: "EntregasMoto",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrimeraRevisionProgramadaEn",
                table: "EntregasMoto",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProtocoloEntrega",
                table: "EntregasMoto",
                type: "nvarchar(800)",
                maxLength: 800,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntregasMoto_EmpresaId_PrimeraRevisionProgramadaEn",
                table: "EntregasMoto",
                columns: new[] { "EmpresaId", "PrimeraRevisionProgramadaEn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EntregasMoto_EmpresaId_PrimeraRevisionProgramadaEn",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "ActividadPrimeraRevisionId",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "ChecklistPreEntregaCompletado",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "FotoEntregaDataUrl",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "FotoEntregaNombre",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "PrimeraRevisionProgramadaEn",
                table: "EntregasMoto");

            migrationBuilder.DropColumn(
                name: "ProtocoloEntrega",
                table: "EntregasMoto");
        }
    }
}
