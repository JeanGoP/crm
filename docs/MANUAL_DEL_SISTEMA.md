# Manual del Sistema CRM SaaS

Ultima actualizacion: 2026-05-29  
Version del manual: 4.0  
Sistema: CRM SaaS para ventas de motos a credito  
Ambiente documentado: Desarrollo local conectado a SQL Server
Zona horaria operativa: Colombia UTC-5

## 1. Objetivo del sistema

El CRM permite gestionar el proceso comercial completo de una empresa de venta de motos a credito: clientes, productos, cotizaciones, solicitudes de credito, entregas, documentos, prospectos, pipeline, actividades, reportes comerciales, usuarios, roles y empresas.

El sistema esta pensado para operar como SaaS multiempresa. Cada usuario pertenece a una empresa y los datos que registra quedan asociados automaticamente a esa empresa.

Las fechas operativas del CRM se registran con fecha y hora de Colombia, UTC-5. Esto aplica para auditoria, cotizaciones, actividades, solicitudes de credito, documentos y decisiones comerciales.

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
- Cotizaciones
- Clientes
- Solicitudes credito
- Entregas
- Pipeline
- Actividades
- Productos
- Prospectos
- Reportes
- Configuracion

En la barra superior se muestra el usuario autenticado, su rol principal y el boton **Salir**.

### Modo demostracion inicial

El menu lateral muestra todos los modulos principales disponibles para operacion:

- Dashboard
- Cotizaciones
- Clientes
- Solicitudes de credito
- Entregas
- Pipeline
- Actividades
- Productos
- Prospectos
- Reportes
- Configuracion

## 6. Dashboard

![Dashboard](./assets/manual/02-dashboard.png)

### Para que sirve

Resume el estado comercial de la empresa: valor del pipeline, clientes activos, prospectos abiertos, actividades pendientes, notificaciones internas y actividad reciente.

### Indicadores

- **Pipeline abierto:** valor total de negocios abiertos.
- **Pipeline ponderado:** valor estimado segun probabilidad de cierre.
- **Clientes activos:** cantidad de clientes activos.
- **Prospectos abiertos:** cantidad de prospectos sin convertir.
- **Actividades pendientes:** tareas, llamadas o reuniones aun sin completar.
- **Vencidas:** actividades pendientes cuya fecha programada ya paso.
- **Para hoy:** actividades pendientes programadas para el dia actual.
- **Notificaciones internas:** recordatorios accionables para priorizar documentos, creditos, actividades y clientes sin seguimiento.
- **Actividad reciente:** ultimos movimientos o actividades programadas.

### Notificaciones internas

El panel de notificaciones internas ayuda al equipo a saber que debe atender primero. Puede mostrar:

- Actividades vencidas que siguen pendientes o en proceso.
- Seguimientos programados para hoy.
- Solicitudes de credito con documentos pendientes o rechazados, indicando cantidad.
- Solicitudes de credito en estudio por varios dias.
- Clientes activos sin actividades recientes o futuras.
- Cotizaciones que llevan varios dias sin seguimiento.
- Negocios abiertos sin actividad reciente.

Cada notificacion muestra el tipo, el nivel de prioridad, una descripcion y un boton **Abrir** para ir al modulo o cliente relacionado.

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

- Primer nombre
- Segundo nombre
- Primer apellido
- Segundo apellido
- Tipo de identificacion
- Numero de identificacion
- Empresa o razon comercial opcional
- Email
- Indicativo telefonico
- Telefono
- Direccion
- Ciudad
- Fecha de nacimiento
- Ocupacion
- Estado
- Etiquetas
- Observaciones

### Crear cliente

1. Abra **Clientes**.
2. Presione **Nuevo cliente**.
3. Complete tipo y numero de identificacion si ya los tiene.
4. Complete primer nombre, segundo nombre si aplica, primer apellido y segundo apellido si aplica.
5. Registre fecha de nacimiento y ocupacion si aplican.
6. Registre indicativo, telefono o WhatsApp, email, direccion y ciudad.
7. Complete datos comerciales como empresa, estado, etiquetas y observaciones.
8. Guarde el registro.

### Editar cliente

1. Ubique el cliente en la tabla.
2. Presione el icono de editar.
3. Ajuste los datos necesarios.
4. Guarde los cambios.

### Eliminar o inactivar

Segun los permisos del usuario, el sistema permite eliminar o marcar registros para no continuar trabajando con ellos.

### Vista 360 del cliente

Desde el cliente se puede acceder a su resumen completo: cotizaciones, solicitudes de credito, negocios del pipeline, actividades relacionadas e historial completo.

Tambien permite crear un **Nuevo seguimiento** directamente desde la ficha del cliente. La actividad queda relacionada automaticamente con el cliente para que aparezca en su historial y en el modulo de Actividades.

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

### Analizar con IA

La ficha 360 incluye el boton **Analizar con IA** en el bloque **Asistente comercial del cliente**.

Al presionarlo, el sistema analiza la informacion real del cliente y muestra:

- Resumen del caso.
- Pendientes.
- Riesgo o prioridad.
- Siguiente mejor accion.
- Mensaje sugerido para WhatsApp.
- Senales usadas para generar el analisis.

Esta primera version funciona como asistente inteligente interno basado en reglas comerciales del CRM. No depende todavia de un proveedor externo de IA, por lo que puede usarse desde el MVP y evolucionar despues hacia modelos generativos.

## 8. Productos

![Productos](./assets/manual/04-productos.png)

### Para que sirve

Permite registrar el catalogo comercial de la empresa. Una moto es una categoria de producto, pero tambien se pueden manejar accesorios, seguros, tramites, repuestos, servicios, garantias u otros productos.

### Datos principales

- Nombre del producto
- Categoria
- Marca opcional
- Modelo opcional
- Referencia
- Cilindraje
- Ano
- Color
- Descripcion
- Precio
- Estado activo/inactivo

### Crear producto

1. Abra **Productos**.
2. Presione **Nuevo producto**.
3. Complete el nombre, categoria y referencia.
4. Agregue marca, modelo, descripcion, cilindraje, ano y color si aplican.
5. Ingrese el precio.
6. Marque el producto como activo.
7. Guarde.

### Uso comercial

Los productos activos aparecen disponibles para crear cotizaciones y solicitudes de credito. Para motos se pueden diligenciar las caracteristicas tecnicas; para accesorios, seguros, tramites o servicios se usan principalmente nombre, categoria, referencia, descripcion y precio.

## 9. Cotizaciones

![Cotizaciones](./assets/manual/05-cotizaciones.png)

![Simulador de credito en cotizacion](./assets/manual/05-cotizaciones-simulador.png)

### Para que sirve

Permite generar una cotizacion para un cliente con datos minimos. Al crear una cotizacion, el sistema registra o relaciona el cliente para continuar el proceso comercial.

Tambien permite simular la financiacion del producto para entregar al cliente una cuota mensual estimada.

### Datos solicitados

- Tipo de identificacion colombiano
- Numero de identificacion
- Nombres del cliente
- Apellidos del cliente
- Indicativo telefonico, por defecto **+57** para Colombia
- Telefono / WhatsApp
- Producto seleccionado
- Cuota inicial
- Plazo en meses
- Tasa mensual
- Valor financiado calculado
- Cuota mensual estimada
- Total estimado a pagar
- Observaciones

### Consultas externas SIMIT y RUNT

El campo **Numero de identificacion** incluye los botones **Simit** y **Runt**. Al presionar cualquiera, el sistema abre la pagina oficial correspondiente y copia el numero de identificacion al portapapeles para pegarlo en la consulta.

Estas paginas externas no exponen un enlace publico estable para consultar automaticamente con el numero ya diligenciado, por eso el CRM abre el sitio oficial y deja listo el numero para pegar.

### Consulta de identidad con Verifik

En la ventana de **Nueva cotizacion**, despues de escribir la cedula, el boton **Consultar** busca primero si esa identificacion ya existe en el maestro de clientes del CRM, incluso si pertenece a otra empresa. Si no existe en la base de datos, el backend consulta Verifik.

Si el CRM o Verifik encuentran informacion, el sistema completa automaticamente:

- Numero de identificacion normalizado
- Primer nombre
- Segundo nombre
- Primer apellido
- Segundo apellido

La integracion usa el endpoint interno del CRM para no exponer el token de Verifik en el navegador. Verifik solo se consume cuando la cedula no existe previamente en la base de datos. Cuando Verifik encuentra informacion, el CRM guarda automaticamente un cliente minimo con esa identificacion para futuras consultas. Para habilitarla en un ambiente real, el administrador tecnico debe configurar la variable de entorno **Verifik__Token** en el backend.

### Crear cotizacion

1. Abra **Cotizaciones**.
2. Presione **Nueva cotizacion**.
3. Seleccione el tipo de identificacion.
4. Escriba el numero de cedula.
5. Si aplica, presione **Consultar** para completar primer nombre, segundo nombre, primer apellido y segundo apellido.
6. Revise o complete los cuatro campos de nombre manualmente.
7. Confirme el indicativo telefonico. Por defecto el sistema propone **+57**.
8. Escriba el telefono o WhatsApp del cliente.
9. Seleccione el producto.
10. Ingrese la cuota inicial.
11. Defina el plazo en meses.
12. Defina la tasa mensual.
13. Ingrese seguro y gastos administrativos si aplican.
14. Revise el resumen del simulador: valor del producto, total financiado, cuota aproximada y total estimado.
15. Agregue observaciones si aplica.
16. Guarde.

### Simulador de credito

El simulador calcula automaticamente:

- **Total financiado:** precio del producto mas seguro y gastos administrativos, menos cuota inicial.
- **Cuota aproximada:** valor aproximado de la cuota segun plazo, tasa mensual y total financiado.
- **Total estimado a pagar:** cuota inicial mas la suma de las cuotas mensuales.

El calculo es una estimacion comercial. La aprobacion final y las condiciones definitivas dependen del proceso de credito.

### Generar PDF

1. Ubique la cotizacion en la tabla.
2. Presione el icono de descarga en la columna **PDF**.
3. El sistema descarga el archivo PDF de la cotizacion con datos del cliente, producto, simulacion de credito, condiciones comerciales y espacios de firma.

### Analizar cliente desde cotizaciones

En la tabla de cotizaciones, el icono de **IA** permite analizar el cliente asociado a la cotizacion sin entrar primero a la ficha 360. El resultado incluye resumen, pendientes, prioridad, siguiente accion y mensaje sugerido para WhatsApp.

### Relacion con clientes

Cuando se crea una cotizacion, el cliente queda disponible en **Clientes** para completar sus datos si avanza hacia credito o venta.

Adicionalmente, el sistema crea automaticamente una actividad de seguimiento llamada **Llamar al cliente mañana** para el dia siguiente. Esta actividad queda relacionada con el cliente y el negocio del pipeline, para que el vendedor recuerde llamar, resolver dudas y avanzar la venta.

Si esa actividad no se completa, cancela o reprograma despues de vencida, el Dashboard muestra la alerta **Seguimiento de cotizacion vencido**.

## 10. Solicitudes de credito

![Solicitudes credito](./assets/manual/06-solicitudes-credito.png)

### Para que sirve

Permite gestionar el tramite de credito de un producto para un cliente.

### Estados de solicitud

- Cotizado
- Interesado
- Documentos pendientes
- Credito en estudio
- Aprobado
- Rechazado
- Entregado
- Desistido

### Flujo de aprobacion

El sistema permite avanzar la solicitud con acciones controladas:

- **Enviar:** pasa de borrador a documentos pendientes y registra la fecha de envio.
- **Interesado:** marca que el cliente desea continuar despues de la cotizacion.
- **Estudio:** pasa a estudio cuando todos los documentos estan recibidos o validados.
- **Aprobar:** solo se permite desde el estado en estudio.
- **Rechazar:** marca la solicitud como rechazada y mueve el pipeline a perdido si existe negocio relacionado.
- **Entregar:** solo se permite despues de aprobar y marca el negocio como ganado/entregado si esta relacionado.
- **Desistir:** cierra el caso cuando el cliente decide no continuar.

Cada decision guarda:

- Fecha de la decision.
- Usuario que la ejecuto.
- Observacion de decision cuando se envia desde API o procesos internos.

### Crear solicitud

1. Abra **Solicitudes credito**.
2. Presione **Nueva solicitud**.
3. Seleccione el cliente.
4. Seleccione el producto principal.
5. Relacione una cotizacion o negocio del pipeline si aplica.
6. Complete identificacion, fecha de nacimiento, celular, direccion, ciudad y ocupacion.
7. Registre ingresos mensuales, cuota inicial, plazo y valor del producto.
8. Si el credito lo requiere, registre la informacion del codeudor.
9. Registre hasta dos referencias personales.
10. Seleccione el estado inicial.
11. Guarde.

El campo **Numero identificacion** tambien incluye los botones **Simit** y **Runt** para abrir la consulta oficial correspondiente y copiar la cedula registrada en la solicitud.

### Codeudor y referencias

El formulario permite registrar informacion clave para estudio de credito:

- Codeudor: nombre, identificacion, celular, parentesco o relacion e ingresos mensuales.
- Referencia 1: nombre, celular y relacion.
- Referencia 2: nombre, celular y relacion.

Si se registra nombre de codeudor, el celular del codeudor es obligatorio. Esto ayuda a evitar solicitudes incompletas antes de pasar a estudio.

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

### Plantillas PDF

En la columna **Plantillas** se pueden generar documentos operativos de la solicitud:

- **Solicitud:** formulario completo de solicitud de credito con cliente, producto, condiciones, codeudor, referencias y checklist documental.
- **Datos:** autorizacion de tratamiento de datos personales para firma del titular.
- **Aprobacion:** carta de aprobacion con condiciones del credito. Solo esta disponible cuando la solicitud esta aprobada o desembolsada.
- **Entrega:** orden de entrega del producto con checklist y firmas. Solo esta disponible cuando la solicitud esta aprobada o desembolsada.

Estas plantillas se generan desde el backend para conservar el mismo formato y reglas sin depender del navegador del usuario.

### Analizar cliente desde solicitudes

En la columna **Acciones**, el icono de **IA** permite analizar el cliente asociado a la solicitud de credito. Es util para revisar rapidamente documentos pendientes, estado del credito, riesgo comercial y el proximo seguimiento recomendado.

### Validar o rechazar documentos

1. En el selector de estado del documento, cambie a **Validado** o **Rechazado**.
2. Si todos los documentos estan recibidos o validados, la solicitud pasa a **Credito en estudio**.

### Relacion con pipeline

Cuando la solicitud cambia de estado, el negocio relacionado puede moverse automaticamente a la etapa correspondiente del pipeline.

Reglas principales:

- Cotizado: etapa cotizado.
- Interesado: etapa interesado.
- Documentos pendientes: etapa documentos pendientes.
- Credito en estudio: etapa credito en estudio.
- Aprobado: etapa aprobado.
- Rechazado: negocio perdido.
- Entregado: negocio ganado y etapa entregado.
- Desistido: negocio perdido.

## 11. Entregas

### Para que sirve

Permite registrar la entrega fisica de la moto despues de que una solicitud de credito fue aprobada o desembolsada. Esta opcion completa el ciclo comercial desde la cotizacion hasta la entrega al cliente.

### Datos principales

- Solicitud aprobada asociada.
- Fecha y hora de entrega.
- Asesor responsable.
- VIN, numero de chasis, numero de motor y placa.
- Kilometraje al momento de entrega.
- Checklist: casco, SOAT, matricula, manual/garantia y acta de entrega firmada.
- Estado: programada, entregada o cancelada.
- Observaciones.

### Crear entrega

1. Abra **Entregas**.
2. Presione **Nueva entrega**.
3. Seleccione una solicitud aprobada o desembolsada.
4. Complete los datos tecnicos de la moto.
5. Marque los documentos y elementos entregados.
6. Si la entrega ya se realizo, cambie el estado a **Entregada**.
7. Guarde.

Cuando una entrega se marca como **Entregada**, la solicitud relacionada queda como **Entregado**, lo que permite cerrar el proceso comercial.

## 12. Prospectos

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

## 13. Pipeline

![Pipeline](./assets/manual/08-pipeline.png)

### Para que sirve

Permite visualizar y gestionar los negocios de venta a credito por etapas.

### Etapas sugeridas

- Cotizado
- Interesado
- Documentos pendientes
- Credito en estudio
- Aprobado
- Rechazado
- Entregado
- Desistido

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

### Mover negocios entre etapas

En la vista pipeline, arrastre la tarjeta del negocio y sueltela sobre otra columna para cambiar su estado comercial. El CRM actualiza la etapa, la probabilidad por defecto de la nueva columna y el estado del negocio cuando aplica:

- **Entregado:** marca el negocio como ganado.
- **Rechazado, Desistido o Perdido:** marca el negocio como perdido.
- Las demas etapas mantienen el negocio abierto.

### Automatizacion desde credito

Si una solicitud de credito esta relacionada con un negocio, los cambios de estado pueden actualizar la etapa del pipeline.

## 14. Actividades

![Actividades](./assets/manual/09-actividades.png)

### Para que sirve

Permite programar tareas, llamadas y reuniones para hacer seguimiento comercial. El modulo esta pensado como agenda diaria del vendedor y supervisor.

### Tipos

- Tarea
- Llamada
- Reunion

### Estados

- Pendiente
- En proceso
- Completada
- Cancelada

### Indicadores y filtros

La pantalla muestra indicadores de seguimiento:

- **Vencidas:** actividades abiertas con fecha anterior a hoy.
- **Para hoy:** actividades abiertas programadas para el dia actual.
- **Proximas:** actividades abiertas con fecha futura.
- **Completadas:** actividades finalizadas.

Tambien incluye filtros rapidos por estado y vencimiento para trabajar primero lo urgente.

### Crear actividad

1. Abra **Actividades**.
2. Presione **Nueva actividad**.
3. Ingrese titulo y descripcion.
4. Seleccione tipo.
5. Defina fecha y hora programada.
6. Configure recordatorio si aplica.
7. Relacione cliente o negocio si corresponde.
8. Guarde.

### Acciones rapidas

Desde la tabla de actividades se puede:

- Marcar una actividad como **En proceso**.
- Marcarla como **Completada**.
- Reprogramarla para manana.
- Cancelarla.
- Editarla.
- Eliminarla si el rol tiene permisos.

### Buenas practicas

- Crear una actividad despues de cada llamada importante.
- Relacionar la actividad al cliente o negocio correcto.
- Marcar como completada cuando se realice.

## 15. Reportes comerciales

![Reportes comerciales](./assets/manual/11-reportes.png)

### Para que sirve

Permite analizar el rendimiento comercial de la empresa por periodo. El modulo esta pensado para gerencia, administradores y supervisores que necesitan revisar ventas, conversion y productos mas demandados.

### Filtros

La pantalla permite seleccionar:

- **Desde:** fecha inicial del periodo.
- **Hasta:** fecha final del periodo.

Por defecto el sistema muestra el mes actual.

### Indicadores principales

- **Cotizaciones:** total de cotizaciones creadas en el periodo.
- **Convertidas a credito:** cotizaciones que ya tienen una solicitud de credito asociada.
- **Conversion cotizacion:** porcentaje de cotizaciones convertidas a credito.
- **Creditos aprobados:** solicitudes aprobadas o desembolsadas.
- **Creditos rechazados:** solicitudes rechazadas.
- **Tasa aprobacion:** porcentaje de creditos aprobados sobre creditos decididos.
- **Valor aprobado:** suma del valor de los creditos aprobados o desembolsados.

### Tablas del reporte

- **Ventas por vendedor:** muestra cotizaciones, creditos aprobados y valor aprobado por vendedor.
- **Cotizaciones por estado:** agrupa cotizaciones vigentes, vencidas y convertidas a credito.
- **Creditos aprobados/rechazados:** agrupa solicitudes por estado y valor.
- **Motos mas cotizadas:** ranking de productos mas cotizados, cantidad y valor cotizado.

### Paso a paso

1. Abra **Reportes** desde el menu lateral.
2. Seleccione el rango de fechas.
3. Presione **Aplicar** o **Actualizar**.
4. Revise indicadores y tablas.
5. Use los resultados para priorizar vendedores, productos y seguimiento comercial.

## 16. Configuracion

![Configuracion](./assets/manual/10-configuracion.png)

### Para que sirve

Permite administrar empresas y usuarios del sistema.

### Empresas

El usuario administrador puede crear empresas. Cada empresa funciona como tenant del sistema.

Datos principales:

- Logo
- Nombre
- Subdominio
- Dominio personalizado
- Estado activo/inactivo

El logo se carga en formato PNG, JPG o WebP. El sistema lo ajusta automaticamente a una medida estandar de **320 x 160 px** y lo muestra en un espacio fijo para evitar que quede demasiado grande, pequeno o deformado.

### Crear empresa

1. Abra **Configuracion**.
2. En la seccion de empresas, presione **Nueva empresa**.
3. Cargue el logo de la empresa si esta disponible.
4. Complete nombre y subdominio.
5. Agregue dominio personalizado si aplica.
6. Marque la empresa como activa.
7. Guarde.

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

## 17. Manejo de errores

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

## 18. Recomendaciones operativas

- Mantener productos actualizados antes de cotizar.
- Crear cotizacion antes de iniciar credito cuando el cliente aun esta decidiendo.
- Usar prospectos para interesados que no han pedido cotizacion formal.
- Convertir prospectos a clientes cuando el proceso avance.
- Subir documentos reales en la solicitud de credito.
- Registrar la entrega de la moto cuando la solicitud este aprobada y validar datos tecnicos antes de marcarla como entregada.
- Mantener el pipeline actualizado para que el dashboard refleje la realidad comercial.
- Registrar actividades despues de cada interaccion importante.

## 19. Historial del manual

| Fecha | Version | Cambio |
| --- | --- | --- |
| 2026-05-29 | 4.0 | Se agrega movimiento drag and drop en el pipeline para arrastrar negocios entre etapas y actualizar estado/probabilidad automaticamente. |
| 2026-05-27 | 3.9 | La consulta con Verifik ahora guarda automaticamente el cliente cuando la cedula no existia en la base y evita duplicados al crear cotizaciones. |
| 2026-05-26 | 3.8 | Se fortalece el simulador financiero de cotizaciones con seguro, gastos administrativos, total financiado, cuota aproximada y total estimado. |
| 2026-05-26 | 3.7 | Se precisa la agenda automatica al crear cotizaciones: actividad Llamar al cliente mañana y alerta especifica si el seguimiento queda vencido. |
| 2026-05-25 | 3.6 | Se ajustan los estados comerciales de motos a credito: Cotizado, Interesado, Documentos pendientes, Credito en estudio, Aprobado, Rechazado, Entregado y Desistido. |
| 2026-05-25 | 3.5 | Se agrega el modulo Entregas para registrar entrega fisica de motos, datos tecnicos, checklist y cierre de solicitudes aprobadas. |
| 2026-05-25 | 3.4 | Se reorganiza el menu principal de acuerdo con el flujo comercial: dashboard, cotizaciones, clientes, credito, pipeline, actividades, productos, prospectos, reportes y configuracion. |
| 2026-05-25 | 3.3 | Se habilitan todos los menus principales: solicitudes de credito, prospectos, pipeline, actividades y reportes. |
| 2026-05-18 | 3.2 | El boton de consulta de identidad ahora se llama Consultar y busca primero en la base de datos antes de consumir Verifik. |
| 2026-05-18 | 3.1 | Se separan los nombres de clientes, prospectos y cotizaciones en primer nombre, segundo nombre, primer apellido y segundo apellido. |
| 2026-05-15 | 3.0 | Se agrega consulta de identidad con Verifik en la creacion de cotizaciones para completar nombres y apellidos desde la cedula. |
| 2026-05-14 | 2.9 | Se agrega logo al crear o editar empresas, con normalizacion automatica a 320 x 160 px y vista previa en configuracion. |
| 2026-05-13 | 2.8 | Se ajusta el registro de fechas operativas del CRM para usar hora de Colombia UTC-5 en lugar de UTC. |
| 2026-05-13 | 2.7 | Se amplia el maestro de clientes con identificacion, indicativo, telefono, direccion, ciudad, fecha de nacimiento, ocupacion y observaciones. |
| 2026-05-13 | 2.6 | Se agrega indicativo telefonico y telefono/WhatsApp obligatorio en la creacion de cotizaciones, guardandolo en el cliente creado. |
| 2026-05-13 | 2.5 | Se bloquearon visualmente los modulos desde solicitudes de credito hasta reportes para una demostracion inicial enfocada en opciones principales. |
| 2026-05-13 | 2.4 | Se agrega el Asistente comercial del cliente con Analizar con IA desde Cliente 360, cotizaciones y solicitudes de credito. |
| 2026-05-11 | 2.1 | Se actualizan las capturas del manual con una pantalla mas amplia y se agrega captura del modulo de reportes comerciales. |
| 2026-05-08 | 2.0 | Se agrega modulo de reportes comerciales con ventas por vendedor, cotizaciones por estado, tasa de conversion, creditos aprobados/rechazados y motos mas cotizadas. |
| 2026-05-07 | 1.9 | Se agregan notificaciones internas para documentos pendientes, credito en estudio, actividades vencidas y clientes sin seguimiento. |
| 2026-05-07 | 1.8 | Se agregan plantillas PDF para cotizacion completa, solicitud de credito, autorizacion de datos, carta de aprobacion y orden de entrega. |
| 2026-05-07 | 1.7 | Se agregan datos de codeudor y referencias personales en solicitudes de credito. |
| 2026-05-06 | 1.6 | Se generaliza el catalogo de productos para manejar motos, accesorios, seguros, tramites, repuestos, servicios y otros productos. |
| 2026-05-06 | 1.5 | Se agregan alertas comerciales en Dashboard y seguimiento automatico al crear cotizaciones. |
| 2026-05-06 | 1.4 | Se mejora gestion de actividades con indicadores, filtros por vencimiento, nombres de cliente/negocio, acciones rapidas y creacion de seguimientos desde Cliente 360. |
| 2026-05-06 | 1.3 | Se agrega historial completo del cliente en Cliente 360 con timeline cronologico de cotizaciones, solicitudes, documentos, decisiones, pipeline, actividades y notas. |
| 2026-05-06 | 1.2 | Se agrega flujo formal de aprobaciones para solicitudes de credito con fechas, usuario de decision y acciones controladas. |
| 2026-05-06 | 1.1 | Se agrega simulador de credito en cotizaciones, cuota estimada en listado y PDF con informacion financiera. |
| 2026-05-05 | 1.0 | Creacion inicial del manual con capturas reales y documentacion de modulos actuales. |
