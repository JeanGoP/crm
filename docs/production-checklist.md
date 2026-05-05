# Checklist Produccion

- HTTPS activo en Netlify y API.
- `Jwt__SigningKey` rotado y almacenado fuera de `appsettings`.
- CORS limitado a dominios Netlify reales.
- Migraciones EF revisadas antes de ejecutar en produccion.
- Backups SQL Server configurados y probados.
- Indices por `EmpresaId` creados por migracion.
- Logs persistentes con retencion.
- Application Pool en `AlwaysRunning` e `Idle Time-out = 0`.
- Health check `/health` monitoreado.
- Semillas iniciales de `Empresas`, `Roles` y usuario Administrador por tenant.
- Politica de contrasenas y MFA planificada.
