using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrmSaas.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260525185000_UpdateMotorcycleCommercialStages")]
    public partial class UpdateMotorcycleCommercialStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE EtapasNegocio SET Nombre = 'Cotizado', Orden = 1, ProbabilidadPredeterminada = 10, Activa = 1 WHERE Nombre = 'Nuevo prospecto';
                UPDATE EtapasNegocio SET Nombre = 'Interesado', Orden = 2, ProbabilidadPredeterminada = 20, Activa = 1 WHERE Nombre = 'Contacto inicial';
                UPDATE EtapasNegocio SET Nombre = 'Documentos pendientes', Orden = 3, ProbabilidadPredeterminada = 35, Activa = 1 WHERE Nombre = 'Preaprobacion';
                UPDATE EtapasNegocio SET Nombre = 'Credito en estudio', Orden = 4, ProbabilidadPredeterminada = 65, Activa = 1 WHERE Nombre = 'Estudio de credito';
                UPDATE EtapasNegocio SET Orden = 5, ProbabilidadPredeterminada = 80, Activa = 1 WHERE Nombre = 'Aprobado';
                UPDATE EtapasNegocio SET Nombre = 'Entregado', Orden = 7, ProbabilidadPredeterminada = 100, Activa = 1 WHERE Nombre = 'Entregada';
                UPDATE EtapasNegocio SET Nombre = 'Desistido', Orden = 8, ProbabilidadPredeterminada = 0, Activa = 1 WHERE Nombre = 'Perdido';
                UPDATE EtapasNegocio SET Orden = 98, Activa = 0 WHERE Nombre = 'Documentos recibidos';
                UPDATE EtapasNegocio SET Orden = 99, Activa = 0 WHERE Nombre = 'Separada';

                DECLARE @now datetime2 = SYSDATETIME();

                WITH EmpresasConEtapas AS (
                    SELECT DISTINCT EmpresaId
                    FROM EtapasNegocio
                )
                INSERT INTO EtapasNegocio (
                    Id, Nombre, Orden, ProbabilidadPredeterminada, Activa, EmpresaId,
                    FechaCreacion, FechaActualizacion, UsuarioCreacion, UsuarioActualizacion
                )
                SELECT
                    NEWID(), v.Nombre, v.Orden, v.Probabilidad, CAST(1 AS bit), e.EmpresaId,
                    @now, NULL, 'system', NULL
                FROM EmpresasConEtapas e
                CROSS APPLY (VALUES
                    ('Cotizado', 1, CAST(10 AS decimal(5, 2))),
                    ('Interesado', 2, CAST(20 AS decimal(5, 2))),
                    ('Documentos pendientes', 3, CAST(35 AS decimal(5, 2))),
                    ('Credito en estudio', 4, CAST(65 AS decimal(5, 2))),
                    ('Aprobado', 5, CAST(80 AS decimal(5, 2))),
                    ('Rechazado', 6, CAST(0 AS decimal(5, 2))),
                    ('Entregado', 7, CAST(100 AS decimal(5, 2))),
                    ('Desistido', 8, CAST(0 AS decimal(5, 2)))
                ) v(Nombre, Orden, Probabilidad)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM EtapasNegocio x
                    WHERE x.EmpresaId = e.EmpresaId AND x.Nombre = v.Nombre
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE EtapasNegocio SET Nombre = 'Nuevo prospecto', Orden = 1, ProbabilidadPredeterminada = 10, Activa = 1 WHERE Nombre = 'Cotizado';
                UPDATE EtapasNegocio SET Nombre = 'Contacto inicial', Orden = 2, ProbabilidadPredeterminada = 20, Activa = 1 WHERE Nombre = 'Interesado';
                UPDATE EtapasNegocio SET Nombre = 'Preaprobacion', Orden = 3, ProbabilidadPredeterminada = 35, Activa = 1 WHERE Nombre = 'Documentos pendientes';
                UPDATE EtapasNegocio SET Nombre = 'Estudio de credito', Orden = 5, ProbabilidadPredeterminada = 65, Activa = 1 WHERE Nombre = 'Credito en estudio';
                UPDATE EtapasNegocio SET Nombre = 'Entregada', Orden = 8, ProbabilidadPredeterminada = 100, Activa = 1 WHERE Nombre = 'Entregado';
                UPDATE EtapasNegocio SET Nombre = 'Perdido', Orden = 9, ProbabilidadPredeterminada = 0, Activa = 1 WHERE Nombre = 'Desistido';
                UPDATE EtapasNegocio SET Activa = 1 WHERE Nombre IN ('Documentos recibidos', 'Separada');
                """);
        }
    }
}
