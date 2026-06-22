using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillDefaultSalesPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO PuntosVenta (
                    Id,
                    Nombre,
                    Codigo,
                    Ciudad,
                    Direccion,
                    Telefono,
                    MarcaPrincipal,
                    LogoMarcaDataUrl,
                    TasaFactorMensual,
                    PlazoMaximoMeses,
                    ModalidadEntrega,
                    TiempoSoatDias,
                    TiempoMatriculaDias,
                    ProveedorSoat,
                    TramitadorMatricula,
                    Activa,
                    EmpresaId,
                    FechaCreacion,
                    FechaActualizacion,
                    UsuarioCreacion,
                    UsuarioActualizacion)
                SELECT
                    NEWID(),
                    'Sede principal',
                    'PRINCIPAL',
                    'Montelibano',
                    NULL,
                    NULL,
                    'Honda',
                    NULL,
                    4.5,
                    30,
                    'ConSoat',
                    14,
                    20,
                    NULL,
                    NULL,
                    1,
                    e.Id,
                    DATEADD(HOUR, -5, SYSUTCDATETIME()),
                    NULL,
                    'migration',
                    NULL
                FROM Empresas e
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM PuntosVenta p
                    WHERE p.EmpresaId = e.Id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE p
                FROM PuntosVenta p
                WHERE p.Codigo = 'PRINCIPAL'
                  AND p.Nombre = 'Sede principal'
                  AND p.UsuarioCreacion = 'migration';
                """);
        }
    }
}
