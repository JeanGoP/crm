SET NOCOUNT ON;

UPDATE dbo.Productos
SET Activo = 1
WHERE ISNULL(Activo, 0) = 0;

SELECT
    COUNT(*) AS TotalProductos,
    SUM(CASE WHEN Activo = 1 THEN 1 ELSE 0 END) AS ProductosActivos,
    SUM(CASE WHEN Activo = 0 THEN 1 ELSE 0 END) AS ProductosInactivos
FROM dbo.Productos;
