/*
    Script de reparacion idempotente para bases que quedaron con migraciones de
    inventario aplicadas parcialmente.

    Objetivo actual del modelo:
    - Empresas.BaseDatosInventarioExterno
    - PuntosVenta.BodegasInventarioExterno
    - CotizacionItems conserva la unidad de inventario usada en la cotizacion.

    Puede ejecutarse mas de una vez.
*/

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[__EFMigrationsHistory](
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF COL_LENGTH(N'dbo.Empresas', N'BaseDatosInventarioExterno') IS NULL
BEGIN
    ALTER TABLE [dbo].[Empresas] ADD [BaseDatosInventarioExterno] nvarchar(128) NULL;
END;

IF COL_LENGTH(N'dbo.PuntosVenta', N'BodegasInventarioExterno') IS NULL
BEGIN
    ALTER TABLE [dbo].[PuntosVenta] ADD [BodegasInventarioExterno] nvarchar(800) NULL;
END;

IF COL_LENGTH(N'dbo.PuntosVenta', N'BaseDatosInventarioExterno') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE e
        SET BaseDatosInventarioExterno = pv.BaseDatosInventarioExterno
        FROM dbo.Empresas e
        INNER JOIN (
            SELECT EmpresaId, MAX(BaseDatosInventarioExterno) AS BaseDatosInventarioExterno
            FROM dbo.PuntosVenta
            WHERE BaseDatosInventarioExterno IS NOT NULL AND BaseDatosInventarioExterno <> ''''
            GROUP BY EmpresaId
        ) pv ON pv.EmpresaId = e.Id
        WHERE e.BaseDatosInventarioExterno IS NULL OR e.BaseDatosInventarioExterno = '''';
    ');
END;

IF COL_LENGTH(N'dbo.Empresas', N'BodegasInventarioExterno') IS NOT NULL
BEGIN
    EXEC(N'
        UPDATE pv
        SET BodegasInventarioExterno = e.BodegasInventarioExterno
        FROM dbo.PuntosVenta pv
        INNER JOIN dbo.Empresas e ON e.Id = pv.EmpresaId
        WHERE (pv.BodegasInventarioExterno IS NULL OR pv.BodegasInventarioExterno = '''')
          AND e.BodegasInventarioExterno IS NOT NULL
          AND e.BodegasInventarioExterno <> '''';
    ');
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'CodigoBodegaInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [CodigoBodegaInventario] nvarchar(40) NULL;
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'NombreBodegaInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [NombreBodegaInventario] nvarchar(160) NULL;
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'PresentacionInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [PresentacionInventario] nvarchar(240) NULL;
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'NumeroSerieInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [NumeroSerieInventario] nvarchar(240) NULL;
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'NumeroMotorInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [NumeroMotorInventario] nvarchar(80) NULL;
END;

IF COL_LENGTH(N'dbo.CotizacionItems', N'NumeroChasisInventario') IS NULL
BEGIN
    ALTER TABLE [dbo].[CotizacionItems] ADD [NumeroChasisInventario] nvarchar(80) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260731233537_ConfigurarBodegasInventarioEmpresa')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731233537_ConfigurarBodegasInventarioEmpresa', N'8.0.7');
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260731234454_ConfigurarBaseDatosInventarioEmpresa')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260731234454_ConfigurarBaseDatosInventarioEmpresa', N'8.0.7');
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260801001419_MoverInventarioExternoASedes')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801001419_MoverInventarioExternoASedes', N'8.0.7');
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260801002230_SepararBaseInventarioEmpresaYBodegasSede')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801002230_SepararBaseInventarioEmpresaYBodegasSede', N'8.0.7');
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260801010122_GuardarUnidadInventarioCotizacion')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260801010122_GuardarUnidadInventarioCotizacion', N'8.0.7');
END;

COMMIT TRANSACTION;

SELECT
    'OK' AS Resultado,
    COL_LENGTH(N'dbo.Empresas', N'BaseDatosInventarioExterno') AS EmpresaBaseInventario,
    COL_LENGTH(N'dbo.PuntosVenta', N'BodegasInventarioExterno') AS SedeBodegasInventario,
    COL_LENGTH(N'dbo.CotizacionItems', N'NumeroChasisInventario') AS CotizacionChasisInventario;
