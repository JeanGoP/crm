# Roadmap de IA para el CRM

Ultima actualizacion: 2026-05-13

Este documento guarda las ideas de inteligencia artificial que se implementaran en futuras fases del CRM SaaS. El objetivo es que la IA ayude a vender mejor, hacer seguimiento comercial, reducir reprocesos y apoyar la gestion de credito.

## 1. Resumen automatico del cliente

En la vista 360 del cliente, la IA debe generar un resumen claro del estado comercial:

- Cotizaciones recientes.
- Producto de interes.
- Solicitud de credito asociada.
- Documentos pendientes.
- Ultima actividad o seguimiento.
- Riesgos o bloqueos actuales.

Ejemplo esperado:

> Cliente cotizo una moto Boxer CT 100, tiene solicitud en estudio, falta soporte de ingresos y no recibe seguimiento hace 5 dias.

## 2. Siguiente mejor accion

La IA debe recomendar que debe hacer el vendedor con cada cliente o negocio:

- Llamar hoy.
- Pedir documento pendiente.
- Enviar cotizacion actualizada.
- Pasar a solicitud de credito.
- Programar visita.
- Escalar a supervisor.
- Cerrar o descartar oportunidad.

## 3. Priorizacion de clientes

Clasificar clientes segun probabilidad o urgencia comercial:

- Caliente.
- Tibio.
- Frio.
- Urgente.
- Sin seguimiento.
- En riesgo de perderse.

## 4. Alertas inteligentes

Crear alertas mas contextuales que las reglas tradicionales:

- Cliente cotizo hace varios dias y no tiene seguimiento.
- Cliente tiene documentos pendientes y credito en estudio.
- Cliente tiene alta probabilidad de compra pero no tiene actividad programada.
- Negocio lleva demasiado tiempo en la misma etapa.

## 5. Asistente de seguimiento comercial

Generar textos listos para enviar por WhatsApp o email:

- Seguimiento posterior a cotizacion.
- Recordatorio de documentos pendientes.
- Confirmacion de cita.
- Mensaje de aprobacion.
- Mensaje de rechazo cordial.
- Recuperacion de cliente sin seguimiento.

## 6. Analisis de cotizaciones perdidas

Detectar patrones comerciales:

- Productos muy cotizados pero poco vendidos.
- Estados donde mas se pierden negocios.
- Vendedores con menor conversion.
- Cotizaciones vencidas sin seguimiento.
- Montos o cuotas que reducen conversion.

## 7. Score de credito preliminar

Calcular una orientacion inicial de riesgo comercial y financiero, sin reemplazar centrales de riesgo:

- Ingresos.
- Cuota estimada.
- Plazo.
- Cuota inicial.
- Codeudor.
- Referencias.
- Documentos completos.

Resultado esperado:

- Riesgo bajo.
- Riesgo medio.
- Riesgo alto.
- Motivos principales.
- Recomendaciones antes de enviar a estudio.

## 8. Extraccion de datos desde documentos

Leer documentos cargados y proponer datos para completar formularios:

- Cedula.
- Soportes de ingresos.
- Recibos de servicio.
- Formularios PDF.
- Imagenes o fotos enviadas por el cliente.

## 9. Validacion inteligente de documentos

Apoyar la revision documental:

- Documento borroso.
- Documento incompleto.
- Documento vencido.
- Datos que no coinciden con el cliente.
- Tipo de documento incorrecto.
- Archivo ilegible.

## 10. Redaccion automatica de documentos

Mejorar textos y observaciones en documentos:

- Cotizacion.
- Solicitud de credito.
- Autorizacion de tratamiento de datos.
- Carta de aprobacion.
- Carta de rechazo.
- Orden de entrega.
- Observaciones internas.

## 11. Chat interno del CRM

Permitir que el usuario consulte informacion del sistema en lenguaje natural:

- Clientes sin seguimiento esta semana.
- Vendedor con mas creditos aprobados.
- Motos mas cotizadas del mes.
- Solicitudes pendientes por documentos.
- Negocios que llevan mas tiempo detenidos.

## 12. Prediccion de cierre

Estimar probabilidad de cierre por negocio en el pipeline:

- Historial del cliente.
- Actividades realizadas.
- Tiempo en etapa.
- Producto cotizado.
- Estado de credito.
- Documentos pendientes.
- Respuesta del cliente.

## Primera implementacion recomendada

La primera fase de IA deberia ser el **Asistente comercial del cliente**.

En clientes, cotizaciones y solicitudes de credito se agregaria un boton **Analizar con IA** que entregue:

- Resumen del caso.
- Pendientes.
- Riesgo o prioridad.
- Siguiente mejor accion.
- Mensaje sugerido para WhatsApp.

Esta fase entregaria valor rapido sin cambiar demasiado la arquitectura actual.

Estado: primera version implementada.

La version inicial ya permite analizar clientes desde Cliente 360, cotizaciones y solicitudes de credito usando reglas comerciales internas. En una fase posterior se podra conectar un modelo generativo para mejorar redaccion, razonamiento y personalizacion de recomendaciones.
