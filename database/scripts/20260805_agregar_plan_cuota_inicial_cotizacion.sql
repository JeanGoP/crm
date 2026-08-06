SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.Cotizaciones', N'CuotaInicialPagadaHoy') IS NULL
    ALTER TABLE [dbo].[Cotizaciones] ADD [CuotaInicialPagadaHoy] decimal(18,2) NOT NULL CONSTRAINT [DF_Cotizaciones_CuotaInicialPagadaHoy] DEFAULT 0;

IF COL_LENGTH(N'dbo.Cotizaciones', N'FechaInicioCreditoEstimada') IS NULL
    ALTER TABLE [dbo].[Cotizaciones] ADD [FechaInicioCreditoEstimada] datetime2 NULL;

IF COL_LENGTH(N'dbo.Cotizaciones', N'PlanCuotaInicialJson') IS NULL
    ALTER TABLE [dbo].[Cotizaciones] ADD [PlanCuotaInicialJson] nvarchar(1600) NULL;

IF COL_LENGTH(N'dbo.CotizacionItems', N'CuotaInicialPagadaHoy') IS NULL
    ALTER TABLE [dbo].[CotizacionItems] ADD [CuotaInicialPagadaHoy] decimal(18,2) NOT NULL CONSTRAINT [DF_CotizacionItems_CuotaInicialPagadaHoy] DEFAULT 0;

IF COL_LENGTH(N'dbo.CotizacionItems', N'FechaInicioCreditoEstimada') IS NULL
    ALTER TABLE [dbo].[CotizacionItems] ADD [FechaInicioCreditoEstimada] datetime2 NULL;

IF COL_LENGTH(N'dbo.CotizacionItems', N'PlanCuotaInicialJson') IS NULL
    ALTER TABLE [dbo].[CotizacionItems] ADD [PlanCuotaInicialJson] nvarchar(1600) NULL;

EXEC(N'
UPDATE [dbo].[Cotizaciones]
SET
    [CuotaInicialPagadaHoy] = [CuotaInicial],
    [FechaInicioCreditoEstimada] = COALESCE([FechaInicioCreditoEstimada], CONVERT(date, [FechaCotizacion]))
WHERE [CuotaInicialPagadaHoy] = 0
  AND [PlanCuotaInicialJson] IS NULL;
');

EXEC(N'
UPDATE [dbo].[CotizacionItems]
SET
    [CuotaInicialPagadaHoy] = [CuotaInicial],
    [FechaInicioCreditoEstimada] = COALESCE([FechaInicioCreditoEstimada], CONVERT(date, [FechaCreacion]))
WHERE [CuotaInicialPagadaHoy] = 0
  AND [PlanCuotaInicialJson] IS NULL;
');

IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20260806031803_AgregarPlanCuotaInicialCotizacion')
BEGIN
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260806031803_AgregarPlanCuotaInicialCotizacion', N'8.0.7');
END;

COMMIT TRANSACTION;

EXEC(N'
SELECT
    COUNT(*) AS Cotizaciones,
    SUM(CASE WHEN CuotaInicialPagadaHoy > 0 THEN 1 ELSE 0 END) AS CotizacionesConInicialPagada
FROM [dbo].[Cotizaciones];
');
