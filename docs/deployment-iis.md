# Despliegue IIS Windows Server

Esta guia publica la API ASP.NET Core 8 en IIS como reverse proxy con Kestrel/ASP.NET Core Module.

## 1. Prerrequisitos del servidor

- Windows Server con IIS instalado.
- .NET 8 Hosting Bundle instalado en el servidor.
- Certificado SSL para el dominio de la API, por ejemplo `https://api.tudominio.com`.
- SQL Server accesible desde el servidor.
- Puerto HTTPS `443` abierto en firewall.

Despues de instalar el Hosting Bundle, reiniciar IIS:

```powershell
iisreset
```

## 2. Publicar la API

Desde la carpeta raiz del proyecto:

```powershell
dotnet publish src\CrmSaas.Api\CrmSaas.Api.csproj -c Release -o publish\crm-saas-api
```

Copiar el contenido de `publish\crm-saas-api` al servidor, por ejemplo:

```text
C:\inetpub\crm-saas-api
```

La carpeta debe contener `CrmSaas.Api.dll`, `web.config`, `appsettings.json` y dependencias publicadas.

## 3. Crear Application Pool

En IIS Manager:

- Nombre: `CrmSaasApi`
- `.NET CLR Version`: `No Managed Code`
- `Managed pipeline mode`: `Integrated`
- `Start Mode`: `AlwaysRunning`
- `Idle Time-out (minutes)`: `0`
- `Enable 32-Bit Applications`: `False`

## 4. Crear sitio IIS

- Nombre del sitio: `CrmSaasApi`
- Ruta fisica: `C:\inetpub\crm-saas-api`
- Binding HTTPS:
  - Tipo: `https`
  - Puerto: `443`
  - Host name: `api.tudominio.com`
  - Certificado: el certificado SSL del dominio

Activar redireccion HTTP a HTTPS si tambien se crea binding en puerto `80`.

## 5. Variables de entorno obligatorias

Configurar estas variables en el Application Pool, en el servidor, o con `web.config` transformado fuera del repositorio:

```powershell
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=TU_SERVIDOR_SQL;Database=CrmSaasDb;User Id=USUARIO_SQL;Password=CLAVE_SQL;Encrypt=True;TrustServerCertificate=True
Jwt__SigningKey=SECRETO_LARGO_MINIMO_64_CARACTERES_CAMBIAR_EN_PRODUCCION
Cors__AllowedOrigins__0=https://TU-SITIO.netlify.app
Verifik__Token=TOKEN_VERIFIK
```

Cuando Netlify entregue el dominio final, agregarlo a CORS. Si luego se usa dominio propio, agregar tambien:

```powershell
Cors__AllowedOrigins__1=https://crm.tudominio.com
```

## 6. Base de datos

Antes de apuntar usuarios reales:

```powershell
dotnet ef database update --project src\CrmSaas.Infrastructure --startup-project src\CrmSaas.Api --connection "Server=TU_SERVIDOR_SQL;Database=CrmSaasDb;User Id=USUARIO_SQL;Password=CLAVE_SQL;Encrypt=True;TrustServerCertificate=True"
```

Validar que existan empresas, roles y usuario administrador.

## 7. Pruebas despues de publicar

Abrir:

```text
https://api.tudominio.com/health
https://api.tudominio.com/swagger
```

Resultado esperado en `/health`:

```json
{ "status": "ok", "service": "crm-saas-api" }
```

## 8. Logs

La API escribe logs Serilog en:

```text
C:\inetpub\crm-saas-api\logs
```

El `web.config` tiene `stdoutLogEnabled=true` para diagnostico inicial. En produccion estable se puede cambiar a `false` y dejar Serilog como log principal.

## 9. Escalamiento horizontal

- Usar SQL Server como estado compartido.
- Mantener JWT stateless.
- Guardar archivos en blob storage o storage compartido, no disco local.
- Enviar logs a archivo central, SIEM o proveedor de observabilidad.
- Replicar las mismas variables de entorno en cada nodo.
