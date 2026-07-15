BEGIN TRANSACTION;
GO

CREATE TABLE [ProductoPreciosSede] (
    [Id] uniqueidentifier NOT NULL,
    [ProductoId] uniqueidentifier NOT NULL,
    [PuntoVentaId] uniqueidentifier NOT NULL,
    [Precio] decimal(18,2) NOT NULL,
    [VigenteDesde] datetime2 NULL,
    [Activo] bit NOT NULL,
    [EmpresaId] uniqueidentifier NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    [FechaActualizacion] datetime2 NULL,
    [UsuarioCreacion] nvarchar(180) NOT NULL,
    [UsuarioActualizacion] nvarchar(180) NULL,
    CONSTRAINT [PK_ProductoPreciosSede] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductoPreciosSede_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductoPreciosSede_PuntosVenta_PuntoVentaId] FOREIGN KEY ([PuntoVentaId]) REFERENCES [PuntosVenta] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_ProductoPreciosSede_EmpresaId] ON [ProductoPreciosSede] ([EmpresaId]);
GO

CREATE UNIQUE INDEX [IX_ProductoPreciosSede_EmpresaId_ProductoId_PuntoVentaId] ON [ProductoPreciosSede] ([EmpresaId], [ProductoId], [PuntoVentaId]);
GO

CREATE INDEX [IX_ProductoPreciosSede_EmpresaId_PuntoVentaId] ON [ProductoPreciosSede] ([EmpresaId], [PuntoVentaId]);
GO

CREATE INDEX [IX_ProductoPreciosSede_ProductoId] ON [ProductoPreciosSede] ([ProductoId]);
GO

CREATE INDEX [IX_ProductoPreciosSede_PuntoVentaId] ON [ProductoPreciosSede] ([PuntoVentaId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260715232132_AgregarPreciosProductoPorSede', N'8.0.7');
GO

COMMIT;
GO

