SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.ConceptosCotizacion', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ConceptosCotizacion] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(80) NOT NULL,
        [Codigo] nvarchar(40) NOT NULL,
        [GrupoCalculo] nvarchar(20) NOT NULL,
        [FuenteValor] nvarchar(40) NOT NULL,
        [ValorPredeterminado] decimal(18,2) NOT NULL,
        [Orden] int NOT NULL,
        [Activo] bit NOT NULL,
        [EmpresaId] uniqueidentifier NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        [UsuarioCreacion] nvarchar(180) NOT NULL,
        [UsuarioActualizacion] nvarchar(180) NULL,
        CONSTRAINT [PK_ConceptosCotizacion] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConceptosCotizacion_EmpresaId' AND object_id = OBJECT_ID(N'dbo.ConceptosCotizacion'))
    CREATE INDEX [IX_ConceptosCotizacion_EmpresaId] ON [dbo].[ConceptosCotizacion] ([EmpresaId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConceptosCotizacion_EmpresaId_Activo_Orden' AND object_id = OBJECT_ID(N'dbo.ConceptosCotizacion'))
    CREATE INDEX [IX_ConceptosCotizacion_EmpresaId_Activo_Orden] ON [dbo].[ConceptosCotizacion] ([EmpresaId], [Activo], [Orden]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ConceptosCotizacion_EmpresaId_Codigo' AND object_id = OBJECT_ID(N'dbo.ConceptosCotizacion'))
    CREATE UNIQUE INDEX [IX_ConceptosCotizacion_EmpresaId_Codigo] ON [dbo].[ConceptosCotizacion] ([EmpresaId], [Codigo]);

INSERT INTO [dbo].[ConceptosCotizacion] (
    [Id], [Nombre], [Codigo], [GrupoCalculo], [FuenteValor], [ValorPredeterminado],
    [Orden], [Activo], [EmpresaId], [FechaCreacion], [UsuarioCreacion]
)
SELECT NEWID(), defaults.Nombre, defaults.Codigo, defaults.GrupoCalculo, defaults.FuenteValor, 0,
       defaults.Orden, 1, e.Id, DATEADD(HOUR, -5, SYSUTCDATETIME()), N'sistema'
FROM [dbo].[Empresas] e
CROSS APPLY (VALUES
    (N'Seguro', N'SEGURO', N'Seguro', N'SoatProducto', 1),
    (N'Matricula', N'MATRICULA', N'Gasto', N'MatriculaProducto', 2),
    (N'Impuestos', N'IMPUESTOS', N'Gasto', N'ImpuestosProducto', 3)
) defaults([Nombre], [Codigo], [GrupoCalculo], [FuenteValor], [Orden])
WHERE NOT EXISTS (
    SELECT 1
    FROM [dbo].[ConceptosCotizacion] existing
    WHERE existing.EmpresaId = e.Id
      AND existing.Codigo = defaults.Codigo
);

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260805222937_AgregarConceptosCotizacion')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260805222937_AgregarConceptosCotizacion', N'8.0.7');
END;

COMMIT TRANSACTION;

SELECT EmpresaId, COUNT(*) AS Conceptos
FROM [dbo].[ConceptosCotizacion]
GROUP BY EmpresaId
ORDER BY EmpresaId;
