# Documentación opcional de solicitudes

La pestaña Documentos ofrece 15 tipos de soporte, agrupados por uso. Ninguno es obligatorio por sí mismo; no se solicitan fechas de vencimiento. Los perfiles no cambian este catálogo.

Después de cargar los soportes necesarios para el caso, use **Documentación completa** y confirme. Se registra fecha y usuario. Puede confirmar incluso sin archivos, bajo responsabilidad del usuario, porque los soportes son opcionales. El botón no marca documentos individuales como recibidos o validados.

**Reabrir documentación** permite retirar la confirmación. Cambiar el estado de un soporte, cargar/reemplazar o borrar su archivo también retira la confirmación. Esto no borra otros archivos ni revoca decisiones de crédito ya tomadas. Para enviar a estudio se exige esta confirmación, además de las verificaciones existentes de identidad y centrales.

Los archivos históricos se conservan. Los que no pertenecen al nuevo catálogo aparecen en “Documentos anteriores conservados” si tienen archivo; no se eliminan registros ni archivos existentes.

## Instalación

1. Respalde la base de datos y la carpeta de archivos del servidor.
2. Suba los binarios de `publish/crm-saas-api`, sin comprimir, conservando la configuración del servidor y `App_Data/uploads`.
3. Reinicie la aplicación. El backend aplica la migración `DocumentacionOpcionalSolicitud` al iniciar: añade los campos de confirmación y las opciones faltantes en solicitudes existentes, sin borrar archivos ni fechas históricas (estas ya no se muestran ni se utilizan).
4. Despliegue el frontend correspondiente. Las solicitudes existentes requieren confirmación explícita; no se inventa una confirmación retroactiva.

## Verificación local

`dotnet run --project tests/CreditDocumentChecks`

`npm run build` desde `frontend`.
