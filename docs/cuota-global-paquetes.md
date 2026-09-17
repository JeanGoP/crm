# Cuota inicial global por paquete

Las categorías activas con «Cotizar como paquete» muestran condiciones globales desde el primer artículo. Los artículos de categorías individuales o combinaciones de categorías mantienen el comparativo por artículo.

- El formulario guarda por separado inicial, extra, plazo y un único plan del paquete.
- Inicial completa = inicial + extra. Se descuenta una sola vez del total de artículos después de promociones, más los cargos aplicables.
- El plan debe cubrir exactamente la extra. La inicial completa no puede superar el valor total del paquete.
- Los artículos conservan precio, descuentos y cargos, pero no duplican inicial ni plan.
- Contado no aplica inicial ni financiación.
- El PDF distingue paquetes de comparativos y muestra sus condiciones globales.

## Despliegue

Actualizar backend y frontend juntos. La migración `CuotaGlobalPaquete` agrega `Cotizaciones.EsPaquete`; el backend ejecuta las migraciones al iniciar. Respaldar la base antes del despliegue y comprobar que el usuario SQL tiene permisos de migración.

La publicación sin comprimir está en `publish/crm-saas-api`. Al copiarla al servidor, conservar su configuración, secretos y archivos subidos. Las cotizaciones anteriores no se recalculan ni reclasifican automáticamente.

## Verificación

`node tests/quote-payments.cjs`

`dotnet run --project tests/CreditDocumentChecks --no-restore`

`npm --prefix frontend run build`

Caso de regresión: paquete de $2.000.000 con primer artículo de $400.000, inicial de $1.000.000 y extra de $500.000: financiado $500.000; plan por $500.000, sin limitar la inicial al primer artículo.
