# Codeudores y documentos por persona

En Crear/Editar solicitud, la sección Codeudores permite agregar, editar y retirar varias personas. Cada una conserva su identificación, celular, ingresos y dos referencias. El primer vencimiento se registra en Producto y crédito y aparece en el resumen y en el PDF de solicitud.

En Gestionar > Documentos, el selector «Documentos de» separa los 15 documentos opcionales del cliente y los de cada codeudor. La confirmación «Documentación completa» cubre el expediente completo. Cambiar codeudores o archivos revoca esa confirmación. Al cambiar codeudores también debe confirmarse nuevamente su consulta de Datacrédito; el indicador es conjunto y el puntaje opcional corresponde al primero.

Retirar un codeudor no borra sus archivos: permanece disponible en Documentos como «retirado». No se asignan esos archivos a otra persona. Los registros y consultas mantienen la empresa de la solicitud.

## Publicación

1. Respaldar la base de datos y detener el sitio/pool de IIS.
2. Copiar el contenido de publish/crm-saas-api, sin comprimir, al directorio de la API. Conservar la configuración real del servidor, secretos, cadena de conexión, web.config y App_Data/uploads.
3. Iniciar la API: ejecuta la migración 20260917030750_CodeudoresMultiplesPrimerVencimiento. La cuenta SQL necesita permisos de migración.
4. Publicar el frontend del mismo commit y recargar el navegador.

La migración conserva el codeudor antiguo como primer codeudor. Los archivos anteriores mantienen su asociación al cliente porque no es seguro deducir a quién pertenecen. Para los codeudores migrados se crean las opciones vacías de documentos y se solicita reconfirmar la documentación; no se alteran decisiones de aprobación.

El primer vencimiento permanece vacío en solicitudes históricas hasta que el responsable lo registre. No se calcula ni inventa una fecha.

## Verificación

- Crear una solicitud con dos codeudores y una fecha de primer vencimiento; guardar y reabrir.
- Subir cédulas diferentes para cliente, codeudor 1 y codeudor 2; comprobar descarga independiente.
- Confirmar documentación, editar un codeudor y verificar que se reabre la confirmación.
- Retirar un codeudor y verificar que sus archivos siguen en su grupo histórico.
- Descargar el PDF de solicitud: debe mostrar todos los codeudores activos y el primer vencimiento.
- Pruebas automatizadas: dotnet run --project tests/CreditDocumentChecks; compilación: npm run build en frontend.
