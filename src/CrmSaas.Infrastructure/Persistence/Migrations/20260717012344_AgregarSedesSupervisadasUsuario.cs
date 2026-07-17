using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSedesSupervisadasUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosSedesSupervisadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PuntoVentaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSedesSupervisadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSedesSupervisadas_PuntosVenta_PuntoVentaId",
                        column: x => x.PuntoVentaId,
                        principalTable: "PuntosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuariosSedesSupervisadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSedesSupervisadas_EmpresaId",
                table: "UsuariosSedesSupervisadas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSedesSupervisadas_EmpresaId_PuntoVentaId",
                table: "UsuariosSedesSupervisadas",
                columns: new[] { "EmpresaId", "PuntoVentaId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSedesSupervisadas_EmpresaId_UsuarioId_PuntoVentaId",
                table: "UsuariosSedesSupervisadas",
                columns: new[] { "EmpresaId", "UsuarioId", "PuntoVentaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSedesSupervisadas_PuntoVentaId",
                table: "UsuariosSedesSupervisadas",
                column: "PuntoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSedesSupervisadas_UsuarioId",
                table: "UsuariosSedesSupervisadas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosSedesSupervisadas");
        }
    }
}
