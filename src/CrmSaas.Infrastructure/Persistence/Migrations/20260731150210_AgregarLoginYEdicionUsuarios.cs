using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLoginYEdicionUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmpresaId_Email",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Usuarios",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                ;WITH BaseUsuarios AS (
                    SELECT
                        Id,
                        LEFT(LOWER(LTRIM(RTRIM(CASE WHEN Email IS NULL OR Email = '' THEN CONVERT(nvarchar(36), Id) ELSE Email END))), 70) AS BaseLogin
                    FROM Usuarios
                ),
                UsuariosNumerados AS (
                    SELECT
                        Id,
                        BaseLogin,
                        ROW_NUMBER() OVER (PARTITION BY BaseLogin ORDER BY Id) AS Numero
                    FROM BaseUsuarios
                )
                UPDATE u
                SET Login = CASE
                    WHEN n.Numero = 1 THEN n.BaseLogin
                    ELSE LEFT(CONCAT(n.BaseLogin, '-', n.Numero), 80)
                END
                FROM Usuarios u
                INNER JOIN UsuariosNumerados n ON n.Id = u.Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId_Email",
                table: "Usuarios",
                columns: new[] { "EmpresaId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_EmpresaId_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Login",
                table: "Usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_EmpresaId_Email",
                table: "Usuarios",
                columns: new[] { "EmpresaId", "Email" },
                unique: true);
        }
    }
}
