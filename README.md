# CRM SaaS Multi-Tenant

Sistema CRM profesional multiempresa con backend ASP.NET Core .NET 8, SQL Server y frontend React + TypeScript para Netlify.

## Estructura

- `src/CrmSaas.Domain`: entidades de dominio en español y enums del CRM.
- `src/CrmSaas.Application`: DTOs en ingles, validaciones, mapeos y servicios de aplicacion.
- `src/CrmSaas.Infrastructure`: EF Core SQL Server, multi-tenant, JWT, bcrypt y persistencia.
- `src/CrmSaas.Api`: API REST, Swagger, CORS, Serilog, middlewares y controllers.
- `frontend`: React, TypeScript, MUI, Zustand, Axios y React Router.

## Multi-Tenant

Todas las entidades persistentes heredan `AuditableTenantEntity`, que incluye:

- `EmpresaId`
- `FechaCreacion`
- `FechaActualizacion`
- `UsuarioCreacion`
- `UsuarioActualizacion`

`CrmDbContext` aplica filtros globales por `EmpresaId` e indices por tenant. El tenant se resuelve por:

- claim JWT `empresa_id`
- subdominio `empresa.midominio.com`
- header `X-Tenant` para desarrollo y Netlify

## Backend

```powershell
dotnet restore CrmSaas.sln --configfile NuGet.config
dotnet build CrmSaas.sln -v:m
dotnet run --project src\CrmSaas.Api\CrmSaas.Api.csproj
```

Configura secretos con variables de entorno en IIS o Windows Server:

- `ConnectionStrings__DefaultConnection`
- `Jwt__SigningKey`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Cors__AllowedOrigins__0`

## SQL Server

Instala la herramienta EF si no existe:

```powershell
dotnet tool install --global dotnet-ef
```

Crea migraciones y aplica la base:

```powershell
dotnet ef migrations add InitialCreate --project src\CrmSaas.Infrastructure --startup-project src\CrmSaas.Api --output-dir Persistence\Migrations
dotnet ef database update --project src\CrmSaas.Infrastructure --startup-project src\CrmSaas.Api
```

## IIS

Publicacion:

```powershell
dotnet publish src\CrmSaas.Api\CrmSaas.Api.csproj -c Release -o .\publish\api
```

Application Pool recomendado:

- .NET CLR version: No Managed Code
- Managed pipeline mode: Integrated
- Start Mode: AlwaysRunning
- Idle Time-out: 0
- Recycling programado fuera de horario comercial
- Logs de IIS habilitados
- ASP.NET Core stdout logs habilitados solo para diagnostico temporal

El archivo `src/CrmSaas.Api/web.config` queda preparado para IIS con ASP.NET Core Module V2.

## Frontend Netlify

```powershell
cd frontend
npm install
npm run build
```

Variables en Netlify:

- `VITE_API_URL=https://api.midominio.com`
- `VITE_TENANT=demo`

`frontend/netlify.toml` incluye el redirect de SPA hacia `index.html`.

## Modulos

- Clientes: CRUD, estado, etiquetas, notas y actividades relacionadas.
- Prospectos: fuente, calificacion y conversion a cliente.
- Negocios: pipeline con etapas configurables, valor, probabilidad y cierre estimado.
- Actividades: tareas, llamadas, reuniones y recordatorios.
- Usuarios/Roles: base de entidades para Administrador, Vendedor y Supervisor.
- Dashboard: indicadores clave, pipeline y actividad reciente.

## Integraciones Futuras

La arquitectura deja puntos claros para WhatsApp API, email, webhooks, ERP, automatizacion e IA sin romper aislamiento por tenant.
