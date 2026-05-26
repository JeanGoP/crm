using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260526090000_BackfillQuoteFollowUps")]
    public partial class BackfillQuoteFollowUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Actividades
                SET Titulo = 'Llamar al cliente mañana',
                    FechaActualizacion = SYSDATETIME(),
                    UsuarioActualizacion = 'system'
                WHERE Titulo LIKE 'Seguimiento cotizacion COT-%';

                INSERT INTO Actividades (
                    Id, Titulo, Descripcion, Tipo, Estado, FechaProgramada, RecordatorioEn,
                    ClienteId, NegocioId, UsuarioAsignadoId, EmpresaId,
                    FechaCreacion, FechaActualizacion, UsuarioCreacion, UsuarioActualizacion
                )
                SELECT
                    NEWID(),
                    'Llamar al cliente mañana',
                    'Cotizacion ' + c.Numero + ': contactar al cliente para resolver dudas y avanzar la venta.',
                    2,
                    1,
                    DATEADD(day, 1, c.FechaCotizacion),
                    DATEADD(hour, 20, c.FechaCotizacion),
                    c.ClienteId,
                    n.Id,
                    NULL,
                    c.EmpresaId,
                    SYSDATETIME(),
                    NULL,
                    'system',
                    NULL
                FROM Cotizaciones c
                OUTER APPLY (
                    SELECT TOP 1 Id
                    FROM Negocios n
                    WHERE n.EmpresaId = c.EmpresaId
                      AND n.ClienteId = c.ClienteId
                      AND n.FechaCreacion >= DATEADD(day, -1, c.FechaCotizacion)
                    ORDER BY n.FechaCreacion
                ) n
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM Actividades a
                    WHERE a.EmpresaId = c.EmpresaId
                      AND a.ClienteId = c.ClienteId
                      AND a.FechaProgramada >= c.FechaCotizacion
                      AND a.Titulo IN ('Llamar al cliente mañana', 'Seguimiento cotizacion ' + c.Numero)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM Actividades
                WHERE Titulo = 'Llamar al cliente mañana'
                  AND UsuarioCreacion = 'system'
                  AND Descripcion LIKE 'Cotizacion COT-%: contactar al cliente para resolver dudas y avanzar la venta.';
                """);
        }
    }
}
