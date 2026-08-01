CREATE OR ALTER VIEW dbo.INVENTARIOCRM AS
SELECT
    CONVERT(bigint, a.IdArticulo) AS IdArticulo,
    a.Codigo,
    a.Nombre,
    a.Presentacion,
    e.Bodega,
    b.Nombre AS NombreBodega,
    es.NumerodeSerie,
    CONVERT(decimal(18, 6),
        CASE
            WHEN ISNULL(a.ExigeSerie, 0) = 1 THEN ISNULL(es.Existencias, 0)
            ELSE ISNULL(e.Existencias, 0)
        END
    ) AS Existencias
FROM dbo.Existencia e
INNER JOIN dbo.Articulo a
    ON a.IdArticulo = e.Articulo
LEFT JOIN dbo.ExistenciaSeries es
    ON es.Articulo = e.Articulo
    AND es.Bodega = e.Bodega
    AND ISNULL(es.Lote, '') = ISNULL(e.Lote, '')
    AND ISNULL(es.Clasificacion, -1) = ISNULL(e.Clasificacion, -1)
    AND ISNULL(es.Existencias, 0) > 0
LEFT JOIN dbo.Bodega b
    ON b.Codigo = e.Bodega
WHERE
    (
        ISNULL(a.ExigeSerie, 0) = 1
        AND ISNULL(es.Existencias, 0) > 0
    )
    OR
    (
        ISNULL(a.ExigeSerie, 0) = 0
        AND ISNULL(e.Existencias, 0) > 0
    );
