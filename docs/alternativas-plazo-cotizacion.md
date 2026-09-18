# Alternativas de plazo en una cotización

- Botones rápidos 6, 12, 18, 24, 36 y 40; electrodomésticos sólo muestran hasta 24.
- «Otro plazo» permite agregar un entero entre 1 y el máximo. Cada selección puede quitarse con un clic. Se requiere al menos una.
- Se conservan una misma inicial, extra, precio, cargos y tasa; cada plazo calcula su propia cuota y total. No se crean cotizaciones separadas.
- Los paquetes guardan alternativas globales. Los comparativos guardan alternativas independientes por artículo.
- El listado y la vista previa muestran las alternativas. El PDF incluye tablas identificadas por paquete o artículo y pagina listas extensas sin truncarlas.
- Los cálculos se guardan en JSON con la cotización y sus artículos: consultar o imprimir no recalcula con tasas actuales.
- Contado ignora los plazos. Los registros anteriores siguen mostrando su plazo original.
- Por compatibilidad, los campos escalares de plazo/cuota representan la primera alternativa ordenada. Al pasar a una solicitud de crédito, confirmar el plazo que finalmente eligió el cliente; las alternativas no son varias deudas.

## Publicación

Actualizar backend y frontend. La migración `AlternativasPlazoCotizacion` agrega columnas opcionales a `Cotizaciones` y `CotizacionItems`; el backend aplica las migraciones al iniciar. Respaldar la base y verificar permisos SQL antes del despliegue.

Backend sin ZIP en `publish/crm-saas-api`; preservar configuración y archivos del servidor.

Pruebas: `node tests/quote-payments.cjs`, `dotnet run --project tests/CreditDocumentChecks --no-restore`, `npm --prefix frontend run build`.
