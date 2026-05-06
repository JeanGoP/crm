# Manual del Sistema CRM SaaS

Ultima actualizacion: 2026-05-06  
Version del manual: 1.3  
Sistema: CRM SaaS para ventas de motos a credito  
Ambiente documentado: Desarrollo local conectado a SQL Server

## 1. Objetivo del sistema

El CRM permite gestionar el proceso comercial completo de una empresa de venta de motos a credito: clientes, productos, cotizaciones, solicitudes de credito, documentos, prospectos, pipeline, actividades, usuarios, roles y empresas.

El sistema esta pensado para operar como SaaS multiempresa. Cada usuario pertenece a una empresa y los datos que registra quedan asociados automaticamente a esa empresa.

## 2. Politica de actualizacion del manual

Este archivo debe actualizarse cada vez que se agregue, cambie o elimine una funcionalidad visible para el usuario.

Regla de mantenimiento:

1. Si se agrega un modulo nuevo, se debe crear una seccion nueva en este manual.
2. Si cambia un formulario, campo, permiso, estado o flujo, se debe actualizar el paso a paso correspondiente.
3. Si cambia una pantalla principal, se debe reemplazar la captura en `docs/assets/manual`.
4. Si se corrige un comportamiento importante, se debe registrar en el historial del manual.
5. Si se prepara una version para produccion, este manual debe revisarse antes del despliegue.

## 3. Roles del sistema

### Administrador

Puede administrar empresas, usuarios, roles y configuraciones generales. Tambien puede acceder a los modulos comerciales.

### Supervisor

Puede gestionar datos comerciales y supervisar la operacion del equipo. Tiene permisos ampliados sobre registros del negocio.

### Vendedor

Puede trabajar sobre clientes, cotizaciones, solicitudes, actividades y pipeline segun los permisos definidos.

## 4. Acceso al sistema

Pantalla de inicio de sesion:

![Login](./assets/manual/01-login.png)

### Para que sirve

Permite ingresar al CRM usando empresa, correo y contrasena. La empresa puede funcionar como identificador del tenant.

### Paso a paso

1. Escriba el identificador de la empresa en el campo **Empresa**.
2. Escriba el correo del usuario en **Email**.
3. Escriba la contrasena en **Contrasena**.
4. Presione **Ingresar**.

### Resultado esperado

Si las credenciales son correctas, el sistema abre el Dashboard. Si hay un error, se muestra un mensaje claro indicando que no fue posible iniciar sesion.

## 5. Navegacion principal

El menu lateral permite moverse entre:

- Dashboard
- Clientes
- Productos
- Cotizaciones
- Solicitudes credito
- Prospectos
- Pipeline
- Actividades
- Configuracion

En la barra superior se muestra el usuario autenticado, su rol principal y el boton **Salir**.

## 6. Dashboard

![Dashboard](./assets/manual/02-dashboard.png)

### Para que sirve

Resume el estado comercial de la empresa: valor del pipeline, clientes activos, prospectos abiertos, actividades pendientes y actividad reciente.

### Indicadores

- **Pipeline abierto:** valor total de negocios abiertos.
- **Pipeline ponderado:** valor estimado segun probabilidad de cierre.
- **Clientes activos:** cantidad de clientes activos.
- **Prospectos abiertos:** cantidad de prospectos sin convertir.
- **Actividades pendientes:** tareas, llamadas o reuniones aun sin completar.
- **Actividad reciente:** ultimos movimientos o actividades programadas.

### Paso a paso

1. Ingrese al sistema.
2. Abra **Dashboard** desde el menu lateral.
3. Revise los indicadores.
4. Presione **Actualizar** para recargar la informacion.

## 7. Clientes

![Clientes](./assets/manual/03-clientes.png)

![Cliente 360](./assets/manual/03-clientes-360.png)

### Para que sirve

Permite administrar los clientes de la empresa. Un cliente puede venir de una cotizacion, de un prospecto convertido o ser creado manualmente.

### Datos principales

- Nombres
- Apellidos
- Empresa o razon comercial opcional
- Email
- Telefono
- Estado
- Etiquetas

### Crear cliente

1. Abra **Clientes**.
2. Presione **Nuevo cliente**.
3. Complete nombres y apellidos.
4. Registre email, telefono y etiquetas si aplican.
5. Seleccione el estado.
6. Guarde el registro.

### Editar cliente

1. Ubique el cliente en la tabla.
2. Presione el icono de editar.
3. Ajuste los datos necesarios.
4. Guarde los cambios.

### Eliminar o inactivar

Segun los permisos del usuario, el sistema permite eliminar o marcar registros para no continuar trabajando con ellos.

### Vista 360 del cliente

Desde el cliente se puede acceder a su resumen completo: cotizaciones, solicitudes de credito, negocios del pipeline, actividades relacionadas e historial completo.

### Historial del cliente

La vista 360 incluye una linea de tiempo cronologica con los eventos mas importantes del cliente:

- Cotizaciones generadas.
- Solicitudes de credito creadas.
- Documentos cargados, recibidos o validados.
- Decisiones de credito: enviado, en estudio, aprobado, rechazado o desembolsado.
- Negocios del pipeline.
- Actividades programadas.
- Notas registradas.

Esta linea de tiempo sirve para que un vendedor, supervisor o administrador entienda rapidamente que ha pasado con el cliente sin abrir cada modulo por separado.

## 8. Productos

![Productos](./assets/manual/04-productos.png)

### Para que sirve

Permite registrar las motos que se van a cotizar y vender.

### Datos principales

- Marca
- Modelo
- Referencia
- Cilindraje
- Ano
- Color
- Precio
- Estado activo/inactivo

### Crear producto

1. Abra **Productos**.
2. Presione **Nueva moto**.
3. Complete marca, modelo y referencia.
4. Agregue cilindraje, ano y color si estan disponibles.
5. Ingrese el precio.
6. Marque el producto como activo.
7. Guarde.

### Uso comercial

Los productos activos aparecen disponibles para crear cotizaciones y solicitudes de credito.

## 9. Cotizaciones

![Cotizaciones](./assets/manual/05-cotizaciones.png)

![Simulador de credito en cotizacion](./assets/manual/05-cotizaciones-simulador.png)

### Para que sirve

Permite generar una cotizacion de moto para un cliente con datos minimos. Al crear una cotizacion, el sistema registra o relaciona el cliente para continuar el proceso comercial.

Tambien permite simular la financiacion de la moto para entregar al cliente una cuota mensual estimada.

### Datos solicitados

- Tipo de identificacion colombiano
- Numero de identificacion
- Nombres del cliente
- Apellidos del cliente
- Moto seleccionada
- Cuota inicial
- Plazo en meses
- Tasa mensual
- Valor financiado calculado
- Cuota mensual estimada
- Total estimado a pagar
- Observaciones

### Crear cotizacion

1. Abra **Cotizaciones**.
2. Presione **Nueva cotizacion**.
3. Seleccione el tipo de identificacion.
4. Escriba numero de identificacion, nombres y apellidos.
5. Seleccione la moto.
6. Ingrese la cuota inicial.
7. Defina el plazo en meses.
8. Defina la tasa mensual.
9. Revise el resumen del simulador: valor de la moto, valor financiado, cuota estimada y total estimado.
10. Agregue observaciones si aplica.
11. Guarde.

### Simulador de credito

El simulador calcula automaticamente:

- **Valor financiado:** precio de la moto menos cuota inicial.
- **Cuota mensual estimada:** valor aproximado de la cuota segun plazo y tasa mensual.
- **Total estimado a pagar:** cuota inicial mas la suma de las cuotas mensuales.

El calculo es una estimacion comercial. La aprobacion final y las condiciones definitivas dependen del proceso de credito.

### Generar PDF

1. Ubique la cotizacion en la tabla.
2. Presione el icono de descarga en la columna **PDF**.
3. El sistema descarga el archivo PDF de la cotizacion con la simulacion de credito incluida.

### Relacion con clientes

Cuando se crea una cotizacion, el cliente queda disponible en **Clientes** para completar sus datos si avanza hacia credito o venta.

## 10. Solicitudes de credito

![Solicitudes credito](./assets/manual/06-solicitudes-credito.png)

### Para que sirve

Permite gestionar el tramite de credito de una moto para un cliente.

### Estados de solicitud

- Borrador
- Documentos pendientes
- Documentos recibidos
- En estudio
- Aprobada
- Rechazada
- Desembolsada

### Flujo de aprobacion

El sistema permite avanzar la solicitud con acciones controladas:

- **Enviar:** pasa de borrador a documentos pendientes y registra la fecha de envio.
- **Estudio:** pasa a estudio cuando todos los documentos estan recibidos o validados.
- **Aprobar:** solo se permite desde el estado en estudio.
- **Rechazar:** marca la solicitud como rechazada y mueve el pipeline a perdido si existe negocio relacionado.
- **Desembolsar:** solo se permite despues de aprobar y marca el negocio como ganado/entregado si esta relacionado.

Cada decision guarda:

- Fecha de la decision.
- Usuario que la ejecuto.
- Observacion de decision cuando se envia desde API o procesos internos.

### Crear solicitud

1. Abra **Solicitudes credito**.
2. Presione **Nueva solicitud**.
3. Seleccione el cliente.
4. Seleccione la moto.
5. Relacione una cotizacion o negocio del pipeline si aplica.
6. Complete identificacion, fecha de nacimiento, celular, direccion, ciudad y ocupacion.
7. Registre ingresos mensuales, cuota inicial, plazo y valor de la moto.
8. Seleccione el estado inicial.
9. Guarde.

### Documentos requeridos

Cada solicitud crea un checklist inicial:

- Cedula
- Soporte de ingresos
- Recibo de servicio o direccion
- Referencias

### Subir documentos reales

1. Abra **Solicitudes credito**.
2. Ubique la solicitud.
3. En la columna **Documentos**, presione el icono de subir archivo junto al documento.
4. Seleccione un archivo permitido.
5. El sistema guarda el archivo y cambia el estado del documento a **Recibido**.

Formatos permitidos:

- PDF
- JPG
- PNG
- WEBP

Tamano maximo: 10 MB por archivo.

### Descargar documento

1. Ubique el documento cargado.
2. Presione el icono de descarga.
3. El sistema descarga el archivo original.

### Validar o rechazar documentos

1. En el selector de estado del documento, cambie a **Validado** o **Rechazado**.
2. Si todos los documentos estan recibidos o validados, la solicitud pasa a **Documentos recibidos**.

### Relacion con pipeline

Cuando la solicitud cambia de estado, el negocio relacionado puede moverse automaticamente a la etapa correspondiente del pipeline.

Reglas principales:

- Documentos pendientes: etapa de preaprobacion.
- Documentos recibidos: etapa documentos recibidos.
- En estudio: etapa estudio de credito.
- Aprobada: etapa aprobado.
- Rechazada: negocio perdido.
- Desembolsada: negocio ganado y etapa entregada.

## 11. Prospectos

![Prospectos](./assets/manual/07-prospectos.png)

### Para que sirve

Permite registrar personas interesadas que aun no son clientes. Un prospecto representa una oportunidad temprana de venta.

### Datos principales

- Nombres
- Apellidos
- Email
- Telefono
- Fuente
- Calificacion

### Calificaciones

- Frio
- Tibio
- Caliente

### Crear prospecto

1. Abra **Prospectos**.
2. Presione **Nuevo prospecto**.
3. Complete nombres, apellidos, email y telefono.
4. Indique la fuente.
5. Seleccione la calificacion.
6. Guarde.

### Convertir prospecto en cliente

1. Ubique el prospecto.
2. Presione la accion de convertir.
3. El sistema crea el cliente con nombres y apellidos separados.
4. Continue completando la informacion desde **Clientes**.

## 12. Pipeline

![Pipeline](./assets/manual/08-pipeline.png)

### Para que sirve

Permite visualizar y gestionar los negocios de venta de motos a credito por etapas.

### Etapas sugeridas

- Nuevo
- Contacto inicial
- Cotizacion
- Preaprobacion
- Documentos recibidos
- Estudio de credito
- Aprobado
- Entregada
- Perdido

### Crear negocio

1. Abra **Pipeline**.
2. Presione **Nuevo negocio**.
3. Escriba un titulo claro, por ejemplo: `Juan Perez - Honda CB125 a credito`.
4. Seleccione cliente.
5. Seleccione etapa.
6. Ingrese valor, probabilidad y fecha estimada de cierre.
7. Guarde.

### Acciones rapidas

- Ver cliente 360.
- Registrar actividad.
- Abrir WhatsApp cuando el cliente tiene telefono.
- Editar negocio.
- Cambiar estado.

### Automatizacion desde credito

Si una solicitud de credito esta relacionada con un negocio, los cambios de estado pueden actualizar la etapa del pipeline.

## 13. Actividades

![Actividades](./assets/manual/09-actividades.png)

### Para que sirve

Permite programar tareas, llamadas y reuniones para hacer seguimiento comercial.

### Tipos

- Tarea
- Llamada
- Reunion

### Estados

- Pendiente
- En proceso
- Completada
- Cancelada

### Crear actividad

1. Abra **Actividades**.
2. Presione **Nueva actividad**.
3. Ingrese titulo y descripcion.
4. Seleccione tipo.
5. Defina fecha y hora programada.
6. Configure recordatorio si aplica.
7. Relacione cliente o negocio si corresponde.
8. Guarde.

### Buenas practicas

- Crear una actividad despues de cada llamada importante.
- Relacionar la actividad al cliente o negocio correcto.
- Marcar como completada cuando se realice.

## 14. Configuracion

![Configuracion](./assets/manual/10-configuracion.png)

### Para que sirve

Permite administrar empresas y usuarios del sistema.

### Empresas

El usuario administrador puede crear empresas. Cada empresa funciona como tenant del sistema.

Datos principales:

- Nombre
- Subdominio
- Dominio personalizado
- Estado activo/inactivo

### Crear empresa

1. Abra **Configuracion**.
2. En la seccion de empresas, presione **Nueva empresa**.
3. Complete nombre y subdominio.
4. Agregue dominio personalizado si aplica.
5. Marque la empresa como activa.
6. Guarde.

### Usuarios

Al crear un usuario se debe seleccionar la empresa a la que pertenece. Esto define donde se guardaran sus datos y que informacion puede consultar.

Datos principales:

- Nombre completo
- Email
- Contrasena inicial
- Empresa
- Roles

### Crear usuario

1. Abra **Configuracion**.
2. En la seccion de usuarios, presione **Nuevo usuario**.
3. Complete nombre, email y contrasena.
4. Seleccione la empresa.
5. Asigne uno o varios roles.
6. Guarde.

## 15. Manejo de errores

El sistema muestra mensajes cuando una accion no se puede completar.

Errores comunes:

- Credenciales incorrectas.
- Campos obligatorios vacios.
- Usuario sin permisos.
- Archivo no permitido.
- Archivo mayor a 10 MB.
- Problemas de conexion con el servidor.

Recomendacion:

Si aparece un error, revise primero los datos ingresados. Si el error persiste, contacte al administrador tecnico con la fecha, usuario y pantalla donde ocurrio.

## 16. Recomendaciones operativas

- Mantener productos actualizados antes de cotizar.
- Crear cotizacion antes de iniciar credito cuando el cliente aun esta decidiendo.
- Usar prospectos para interesados que no han pedido cotizacion formal.
- Convertir prospectos a clientes cuando el proceso avance.
- Subir documentos reales en la solicitud de credito.
- Mantener el pipeline actualizado para que el dashboard refleje la realidad comercial.
- Registrar actividades despues de cada interaccion importante.

## 17. Historial del manual

| Fecha | Version | Cambio |
| --- | --- | --- |
| 2026-05-06 | 1.3 | Se agrega historial completo del cliente en Cliente 360 con timeline cronologico de cotizaciones, solicitudes, documentos, decisiones, pipeline, actividades y notas. |
| 2026-05-06 | 1.2 | Se agrega flujo formal de aprobaciones para solicitudes de credito con fechas, usuario de decision y acciones controladas. |
| 2026-05-06 | 1.1 | Se agrega simulador de credito en cotizaciones, cuota estimada en listado y PDF con informacion financiera. |
| 2026-05-05 | 1.0 | Creacion inicial del manual con capturas reales y documentacion de modulos actuales. |
