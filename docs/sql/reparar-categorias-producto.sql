SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.CategoriasProducto', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.CategoriasProducto
        (
            Id uniqueidentifier NOT NULL,
            Nombre nvarchar(80) NOT NULL,
            Descripcion nvarchar(400) NULL,
            CotizarComoPaquete bit NOT NULL,
            Activa bit NOT NULL,
            EmpresaId uniqueidentifier NOT NULL,
            FechaCreacion datetime2 NOT NULL,
            FechaActualizacion datetime2 NULL,
            UsuarioCreacion nvarchar(180) NOT NULL,
            UsuarioActualizacion nvarchar(180) NULL,
            CONSTRAINT PK_CategoriasProducto PRIMARY KEY (Id)
        );
    END;

    UPDATE dbo.CategoriasProducto
    SET
        Nombre = LTRIM(RTRIM(Nombre)),
        FechaActualizacion = DATEADD(HOUR, -5, SYSUTCDATETIME()),
        UsuarioActualizacion = 'reparacion'
    WHERE Nombre <> LTRIM(RTRIM(Nombre));

    ;WITH ranked AS
    (
        SELECT
            Id,
            ROW_NUMBER() OVER
            (
                PARTITION BY EmpresaId, UPPER(LTRIM(RTRIM(Nombre)))
                ORDER BY
                    CotizarComoPaquete DESC,
                    CASE WHEN Descripcion IS NOT NULL THEN 0 ELSE 1 END,
                    FechaCreacion,
                    Id
            ) AS RowNumber
        FROM dbo.CategoriasProducto
        WHERE NULLIF(LTRIM(RTRIM(Nombre)), '') IS NOT NULL
    )
    DELETE FROM ranked
    WHERE RowNumber > 1;

    ;WITH source AS
    (
        SELECT
            EmpresaId,
            NULLIF(LTRIM(RTRIM(Categoria)), '') AS Nombre,
            CAST(NULL AS nvarchar(400)) AS Descripcion,
            CAST(CASE WHEN LTRIM(RTRIM(Categoria)) LIKE '%Electrodom%' THEN 1 ELSE 0 END AS int) AS CotizarComoPaquete
        FROM dbo.Productos
        WHERE NULLIF(LTRIM(RTRIM(Categoria)), '') IS NOT NULL

        UNION ALL

        SELECT
            Id AS EmpresaId,
            'Moto' AS Nombre,
            'Categoria principal para motos y vehiculos.' AS Descripcion,
            0 AS CotizarComoPaquete
        FROM dbo.Empresas

        UNION ALL

        SELECT
            Id AS EmpresaId,
            'Electrodomesticos' AS Nombre,
            'Categoria para cotizar varios articulos como un solo paquete.' AS Descripcion,
            1 AS CotizarComoPaquete
        FROM dbo.Empresas
    ),
    deduplicated AS
    (
        SELECT
            EmpresaId,
            MIN(Nombre) AS Nombre,
            MAX(Descripcion) AS Descripcion,
            CAST(MAX(CotizarComoPaquete) AS bit) AS CotizarComoPaquete
        FROM source
        GROUP BY EmpresaId, UPPER(Nombre)
    )
    INSERT INTO dbo.CategoriasProducto
        (Id, Nombre, Descripcion, CotizarComoPaquete, Activa, EmpresaId, FechaCreacion, UsuarioCreacion)
    SELECT
        NEWID(),
        source.Nombre,
        source.Descripcion,
        source.CotizarComoPaquete,
        1,
        source.EmpresaId,
        DATEADD(HOUR, -5, SYSUTCDATETIME()),
        'reparacion'
    FROM deduplicated source
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.CategoriasProducto existing
        WHERE existing.EmpresaId = source.EmpresaId
          AND UPPER(existing.Nombre) = UPPER(source.Nombre)
    );

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.CategoriasProducto')
          AND name = N'IX_CategoriasProducto_EmpresaId'
    )
    BEGIN
        CREATE INDEX IX_CategoriasProducto_EmpresaId
        ON dbo.CategoriasProducto (EmpresaId);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.CategoriasProducto')
          AND name = N'IX_CategoriasProducto_EmpresaId_Nombre'
    )
    BEGIN
        CREATE UNIQUE INDEX IX_CategoriasProducto_EmpresaId_Nombre
        ON dbo.CategoriasProducto (EmpresaId, Nombre);
    END;

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
       AND NOT EXISTS
       (
            SELECT 1
            FROM dbo.__EFMigrationsHistory
            WHERE MigrationId = N'20260715221022_CrearCategoriasProducto'
       )
    BEGIN
        INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES (N'20260715221022_CrearCategoriasProducto', N'8.0.7');
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
