# Roadmap funcional MCH

Este documento guarda los siguientes pasos acordados para adaptar EnMarcha CRM al flujo comercial real de MCH. Debe actualizarse cuando una fase cambie de alcance o quede implementada.

## Fase actual: maestro de sedes / puntos de venta

Objetivo: centralizar la configuracion operativa por sede para que los siguientes modulos no queden quemados por empresa.

Incluye:

- Nombre, codigo, ciudad, direccion y telefono de la sede.
- Marca principal y logo de marca.
- Tasa factor mensual y plazo maximo.
- Modalidad de entrega: con SOAT o completa.
- Tiempos estimados de SOAT y matricula.
- Proveedor SOAT y tramitador de matricula.
- Estado activo/inactivo.

## Siguientes pasos recomendados

1. Conectar sedes con usuarios. Implementado.
   Cada asesor tiene una sede principal para que cotizaciones, tareas y reportes puedan segmentarse correctamente.

2. Conectar sedes con cotizaciones. Implementado.
   La cotizacion usa logo de marca, tasa, plazo, vigencia y condiciones de la sede del asesor.

3. Ampliar maestro de productos. Implementado.
   Agrega SOAT, matricula, impuestos, ficha tecnica estructurada, vigencia de precio y marca/modelo/linea/version/color mas precisos. La cotizacion toma esos cargos como valores sugeridos del producto.

4. Crear perfiles de requisitos. Implementado.
   Empleado, independiente, pensionado, contado y perfiles configurables por empresa. El perfil elegido en la cotizacion genera el checklist de documentos al crear la solicitud de credito.

5. Crear promociones / planes tacticos. Implementado.
   Descuentos por producto, marca, color, sede y vigencia. La cotizacion aplica automaticamente la promocion vigente mas especifica.

6. Fortalecer gestor de documentos. Implementado.
   Documentos ligados al cliente, vigencia por tipo, rechazo con motivo, permiso de validacion y alerta al analista cuando el checklist este completo.

7. Formalizar estudio de credito. Implementado.
   Paso 0 RUNT/SIMIT, validacion de identidad, recalculo por analista, aprobado con ajuste, aprobado con codeudor, negado y carta de condiciones finales.

8. Crear orden de recaudo. Implementado.
   Conceptos separados: vehiculo, documentos y anticipo. Estados: emitida, pagada, parcial, vencida y anulada.

9. Crear modulo de tramites.
   SOAT, matricula, placas, terceros, tiempos por sede, listas de atrasados y notificaciones al cliente.

10. Fortalecer entrega.
    Protocolo digital por marca, acta, foto de entrega, checklist obligatorio y agendamiento automatico de primera revision.

11. Inventario comercial.
    Stock por sede, seriales/chasis/motor, separaciones contra disponibilidad y motos usadas.

12. Integracion Zeus.
    Primero vistas de solo lectura y orden de recaudo en PDF; despues esquema puente `CRM_PUENTE` para pagos, facturacion e inventario.
