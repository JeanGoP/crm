# Despliegue Frontend Netlify

Esta guia publica el frontend React + TypeScript en Netlify desde GitHub.

## 1. Crear sitio desde GitHub

En Netlify:

1. Crear nuevo sitio.
2. Seleccionar el repositorio `JeanGoP/crm`.
3. Configurar:
   - Base directory: `frontend`
   - Build command: `npm run build`
   - Publish directory: `frontend/dist`

El archivo `frontend/netlify.toml` ya incluye el redirect SPA para que rutas como `/clientes` o `/cotizaciones` funcionen al refrescar.

## 2. Variables de entorno

En Netlify, configurar:

```text
VITE_API_URL=https://api.tudominio.com
VITE_TENANT=demo
```

`VITE_API_URL` debe apuntar al dominio HTTPS donde quedo publicada la API en IIS.

### Opcion temporal si la API aun no tiene HTTPS

Si la API esta publicada solo por HTTP, por ejemplo:

```text
http://stecno.dyndns.org/crmapi/
```

el navegador bloqueara llamadas directas desde Netlify por contenido mixto. Para probar mientras se instala HTTPS en IIS, usar el proxy incluido en `frontend/netlify.toml`:

```text
VITE_API_URL=/crmapi-proxy
VITE_TENANT=demo
```

Con esa configuracion, el frontend llama a Netlify por HTTPS y Netlify reenvia internamente hacia:

```text
http://stecno.dyndns.org/crmapi/
```

Esta opcion sirve para prueba inicial. Para produccion formal se recomienda instalar certificado SSL en IIS y usar `VITE_API_URL=https://...`.

## 3. Deploy

Ejecutar deploy desde Netlify. Al terminar, Netlify entregara un dominio como:

```text
https://nombre-del-sitio.netlify.app
```

Ese dominio debe agregarse en CORS del backend:

```text
Cors__AllowedOrigins__0=https://nombre-del-sitio.netlify.app
```

## 4. Prueba final

Abrir el sitio de Netlify y probar:

1. Login.
2. Dashboard.
3. Cotizaciones.
4. Clientes.
5. Consulta de identidad.
6. Solicitudes de credito.

Si el login falla por CORS, revisar que el origen exacto de Netlify este en las variables del backend y reiniciar el Application Pool.
