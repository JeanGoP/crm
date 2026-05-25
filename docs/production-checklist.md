# Checklist Produccion

- Dominio de API definido, por ejemplo `https://api.tudominio.com`.
- Sitio Netlify creado desde GitHub con base directory `frontend`.
- Variable `VITE_API_URL` configurada en Netlify apuntando a la API HTTPS.
- HTTPS activo en Netlify y API.
- `Jwt__SigningKey` rotado y almacenado fuera de `appsettings`.
- CORS limitado a dominios Netlify reales.
- `Verifik__Token` configurado como variable de entorno, no en archivos.
- Migraciones EF revisadas antes de ejecutar en produccion.
- Backups SQL Server configurados y probados.
- Indices por `EmpresaId` creados por migracion.
- Logs persistentes con retencion.
- Application Pool en `AlwaysRunning` e `Idle Time-out = 0`.
- Health check `/health` monitoreado.
- Semillas iniciales de `Empresas`, `Roles` y usuario Administrador por tenant.
- Politica de contrasenas y MFA planificada.
