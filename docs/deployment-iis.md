# Despliegue IIS Windows Server

1. Instalar .NET 8 Hosting Bundle en el servidor.
2. Publicar la API:

```powershell
dotnet publish src\CrmSaas.Api\CrmSaas.Api.csproj -c Release -o C:\inetpub\crm-saas-api
```

3. Crear sitio en IIS con HTTPS obligatorio.
4. Configurar Application Pool:

- `.NET CLR Version`: No Managed Code
- `Start Mode`: AlwaysRunning
- `Idle Time-out`: 0
- `Enable 32-Bit Applications`: False

5. Variables de entorno del sitio:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=Server=...;Database=CrmSaas;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=False`
- `Jwt__SigningKey=<secreto largo>`
- `Cors__AllowedOrigins__0=https://crm.netlify.app`

6. Escalamiento horizontal:

- Usar SQL Server como estado compartido.
- Mantener JWT stateless.
- Guardar archivos en blob storage o storage compartido, no disco local.
- Enviar logs a archivo central, SIEM o proveedor de observabilidad.
