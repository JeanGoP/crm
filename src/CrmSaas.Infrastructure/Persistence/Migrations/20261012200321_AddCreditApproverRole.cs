using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditApproverRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO [Roles] ([Id], [Nombre], [Descripcion], [EmpresaId], [FechaCreacion], [FechaActualizacion], [UsuarioCreacion], [UsuarioActualizacion])
                SELECT NEWID(), N'Aprobador de créditos', N'Gestiona estudios y decisiones de crédito', e.[Id], GETDATE(), NULL, N'migration:AddCreditApproverRole', NULL
                FROM [Empresas] AS e
                WHERE NOT EXISTS (
                    SELECT 1 FROM [Roles] AS r
                    WHERE r.[EmpresaId] = e.[Id] AND r.[Nombre] = N'Aprobador de créditos'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE r FROM [Roles] AS r
                WHERE r.[Nombre] = N'Aprobador de créditos'
                  AND r.[UsuarioCreacion] = N'migration:AddCreditApproverRole'
                  AND NOT EXISTS (SELECT 1 FROM [UsuarioRoles] AS ur WHERE ur.[RolId] = r.[Id]);
                """);
        }
    }
}
