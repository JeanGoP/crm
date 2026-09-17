# Plazos y presentación de cotizaciones

- Crédito general: selector de 1 a 40 cuotas.
- Electrodomésticos: selector de 1 a 24 cuotas, individual o paquete. La categoría se reconoce por `electrodom`, igual que la regla de precios por sede existente.
- El backend valida la categoría real de los productos al simular y guardar. Rechaza valores fuera del rango. Contado no solicita ni valida un plazo de crédito.
- Estos límites sustituyen el recorte automático al plazo máximo antiguo de la tasa/sede/configuración. La tasa de interés elegida se conserva; el plazo seleccionado ya no cambia silenciosamente.
- Al cambiar de producto, un plazo que exceda el nuevo máximo queda marcado para seleccionar uno válido. No se modifica en silencio.
- Los cuatro campos de nombres y apellidos se convierten a mayúsculas al escribir, consultar identificación y guardar la cotización. El backend también normaliza los nombres; conserva tildes y ñ. No se ejecuta una actualización masiva de clientes históricos.
- Los campos monetarios pendientes en solicitudes, productos, precios por sede, recaudos, promociones de valor fijo, conceptos y configuración usan puntos de miles y presentan cero como vacío. Los datos enviados siguen siendo números; porcentajes, documentos y teléfonos no reciben formato monetario.

## Verificación y despliegue

Ejecutar `node tests/quote-payments.cjs`, `dotnet run --project tests/CreditDocumentChecks --no-restore` y `npm --prefix frontend run build`.

Actualizar frontend y backend. Publicación sin ZIP en `publish/crm-saas-api`; conservar configuración y archivos del servidor. Este cambio no agrega migraciones.
