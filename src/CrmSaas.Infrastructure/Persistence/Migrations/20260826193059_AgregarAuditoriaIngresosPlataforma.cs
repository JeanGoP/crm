using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoriaIngresosPlataforma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngresosPlataforma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreUsuario = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false),
                    MotivoFallo = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    DireccionIp = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioCreacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    UsuarioActualizacion = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresosPlataforma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngresosPlataforma_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosPlataforma_EmpresaId",
                table: "IngresosPlataforma",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_IngresosPlataforma_EmpresaId_FechaIngreso",
                table: "IngresosPlataforma",
                columns: new[] { "EmpresaId", "FechaIngreso" });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosPlataforma_EmpresaId_UsuarioId_FechaIngreso",
                table: "IngresosPlataforma",
                columns: new[] { "EmpresaId", "UsuarioId", "FechaIngreso" });

            migrationBuilder.CreateIndex(
                name: "IX_IngresosPlataforma_UsuarioId",
                table: "IngresosPlataforma",
                column: "UsuarioId");

            migrationBuilder.Sql("""
                INSERT INTO IngresosPlataforma (
                    Id,
                    UsuarioId,
                    NombreUsuario,
                    Login,
                    Email,
                    FechaIngreso,
                    Exitoso,
                    MotivoFallo,
                    DireccionIp,
                    UserAgent,
                    EmpresaId,
                    FechaCreacion,
                    FechaActualizacion,
                    UsuarioCreacion,
                    UsuarioActualizacion
                )
                SELECT
                    NEWID(),
                    u.Id,
                    u.NombreCompleto,
                    u.Login,
                    u.Email,
                    rt.FechaCreacion,
                    CAST(1 AS bit),
                    N'Historico de sesion existente',
                    NULL,
                    NULL,
                    rt.EmpresaId,
                    rt.FechaCreacion,
                    NULL,
                    N'system',
                    NULL
                FROM RefreshTokens rt
                INNER JOIN Usuarios u ON u.Id = rt.UsuarioId
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM IngresosPlataforma ip
                    WHERE ip.UsuarioId = rt.UsuarioId
                      AND ip.FechaIngreso = rt.FechaCreacion
                      AND ip.EmpresaId = rt.EmpresaId
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngresosPlataforma");
        }
    }
}
