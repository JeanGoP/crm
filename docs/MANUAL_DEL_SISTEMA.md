# Manual de usuario - CRM Comercial

Ultima actualizacion: 2026-06-09  
Version del manual: 5.8
Sistema: CRM para gestion comercial y ventas a credito  

## 1. Bienvenida

Este manual explica como usar el CRM en la operacion diaria. Esta dirigido a vendedores, supervisores y administradores que necesitan registrar clientes, crear cotizaciones, hacer seguimiento comercial, gestionar solicitudes de credito, controlar entregas y revisar reportes.

El sistema ayuda a mantener organizada la informacion comercial de la empresa para que cada asesor sepa que cliente atender, que negocio esta pendiente, que documentos faltan y que oportunidades pueden convertirse en venta.

## 2. Conceptos importantes

Antes de usar el sistema, tenga presentes estos conceptos:

- **Cliente:** persona o empresa que ya fue registrada en el sistema. Puede venir desde una cotizacion o ser creado manualmente.
- **Prospecto:** persona interesada que todavia no tiene toda la informacion comercial completa. Puede convertirse en cliente.
- **Cotizacion:** propuesta comercial generada para un cliente, normalmente con producto, precio, cuota inicial y financiacion.
- **Solicitud de credito:** proceso donde se revisan documentos, codeudor, referencias y decision de aprobacion.
- **Pipeline:** tablero visual donde se ve el avance de cada venta por etapas.
- **Actividad:** tarea, llamada o reunion que debe realizarse para hacer seguimiento.
- **Producto:** articulo que se cotiza o vende. Puede ser moto u otro tipo de producto configurado por la empresa.

## 3. Roles de usuario

El acceso a las opciones depende del rol asignado.

### Administrador

Puede administrar empresas, usuarios, productos, configuracion general y consultar toda la informacion comercial.

### Supervisor

Puede revisar la gestion del equipo, consultar reportes, validar seguimiento comercial y apoyar decisiones del proceso.

### Vendedor

Puede crear clientes, cotizaciones, actividades, solicitudes y hacer seguimiento a sus oportunidades de venta.

## 4. Ingreso al sistema

![Login](./assets/manual/01-login.png)

### Para que sirve

La pantalla de ingreso permite entrar al CRM con el usuario asignado por la empresa.

### Paso a paso

1. Escriba la empresa o identificador indicado por el administrador.
2. Escriba su correo electronico.
3. Escriba su contrasena.
4. Presione **Ingresar**.

### Si no puede ingresar

Revise que la empresa, el correo y la contrasena esten escritos correctamente. Si el problema continua, solicite al administrador que valide si su usuario esta activo.

## 5. Menu principal

El menu lateral permite moverse por las opciones principales:

- **Dashboard:** resumen general de la gestion comercial.
- **Cotizaciones:** creacion y consulta de propuestas comerciales.
- **Clientes:** maestro de clientes y vista Cliente 360.
- **Solicitudes de credito:** gestion de documentos, codeudor, referencias y decision.
- **Entregas:** control de entrega del producto al cliente.
- **Pipeline:** seguimiento visual de negocios por etapa.
- **Actividades:** agenda de tareas, llamadas y reuniones.
- **Productos:** catalogo de productos disponibles para cotizar.
- **Prospectos:** interesados que pueden convertirse en clientes.
- **Reportes:** indicadores comerciales.
- **Configuracion:** empresas, usuarios, roles y parametros generales.

En la parte superior se muestra el usuario conectado y la opcion **Salir** para cerrar sesion.

## 6. Dashboard

![Dashboard](./assets/manual/02-dashboard.png)

### Para que sirve

El Dashboard muestra el estado general del negocio. Es la primera pantalla para saber que esta pasando y que requiere atencion.

### Informacion que muestra

- **Pipeline abierto:** valor total de negocios que siguen en proceso.
- **Pipeline ponderado:** valor estimado segun la probabilidad de cierre.
- **Clientes activos:** cantidad de clientes registrados y activos.
- **Prospectos abiertos:** interesados que aun no se han convertido en clientes.
- **Actividades pendientes:** seguimientos que todavia no se han completado.
- **Actividades vencidas:** tareas o llamadas que ya debieron realizarse.
- **Actividades para hoy:** gestiones programadas para el dia actual.
- **Notificaciones internas:** alertas sobre documentos, creditos, actividades vencidas o clientes sin seguimiento.
- **Actividad reciente:** ultimos movimientos registrados.

### Recomendacion de uso

Revise el Dashboard al iniciar el dia. Le ayuda a priorizar llamadas, documentos pendientes y negocios que necesitan seguimiento.

## 7. Clientes

![Clientes](./assets/manual/03-clientes.png)

### Para que sirve

El modulo Clientes guarda la informacion principal de las personas o empresas con las que se tiene relacion comercial.

### Crear un cliente

1. Entre a **Clientes**.
2. Presione **Nuevo cliente**.
3. Seleccione el tipo de identificacion.
4. Escriba el numero de identificacion.
5. Complete nombres y apellidos.
6. Registre telefono, indicativo, correo, direccion y ciudad si los tiene.
7. Seleccione el estado del cliente.
8. Presione **Guardar**.

### Editar un cliente

1. Busque el cliente en la lista.
2. Presione la accion de editar.
3. Actualice los datos necesarios.
4. Guarde los cambios.

### Cliente creado desde cotizacion

Cuando se crea una cotizacion, el sistema tambien puede crear el cliente automaticamente con los datos basicos. Despues, desde Clientes, se pueden completar los demas datos.

### Consulta de identidad

Cuando se digita una identificacion, el boton **Consultar** busca primero si el cliente ya existe en la base de datos. Si no existe y la integracion esta configurada, consulta la informacion externa disponible y ayuda a completar nombres y apellidos.

## 8. Cliente 360

![Cliente 360](./assets/manual/03-clientes-360.png)

### Para que sirve

Cliente 360 muestra toda la informacion importante de un cliente en una sola pantalla.

### Que puede revisar

- Datos personales y de contacto.
- Cotizaciones asociadas.
- Solicitudes de credito.
- Negocios en pipeline.
- Actividades y seguimientos.
- Historial de interacciones.
- Documentos y notas relacionados.

### Recomendacion de uso

Antes de llamar o escribir a un cliente, abra Cliente 360. Asi puede ver que se ha hablado, que documentos faltan y cual es el siguiente paso.

## 9. Productos

![Productos](./assets/manual/04-productos.png)

### Para que sirve

El modulo Productos permite administrar lo que la empresa vende o cotiza. Aunque el sistema inicio enfocado en motos, tambien puede manejar otros productos.

### Crear un producto

1. Entre a **Productos**.
2. Presione **Nuevo producto**.
3. Escriba nombre, referencia, marca y categoria.
4. Complete las caracteristicas principales, como cilindraje, modelo, color u otros datos necesarios.
5. Ingrese precio y estado.
6. Guarde.

### Adjuntar fotos al producto

1. Cree o edite el producto.
2. En la seccion **Fotos del producto**, presione **Adjuntar fotos**.
3. Seleccione una o varias imagenes.
4. Revise las miniaturas cargadas.
5. Presione **Usar en PDF** sobre la foto que desea mostrar en la cotizacion.

### Foto para la cotizacion

La foto marcada como **Foto PDF** sera la imagen comercial que aparecera en el PDF de cotizacion. El sistema admite imagenes **JPG/JPEG** y **PNG compatibles** para imprimirlas en el PDF.

### Recomendacion de uso

Mantenga precios, modelos y estados actualizados. Una cotizacion depende de que el producto tenga informacion correcta.

## 10. Cotizaciones

![Cotizaciones](./assets/manual/05-cotizaciones.png)

### Para que sirve

Cotizaciones permite generar una propuesta comercial para un cliente. Es uno de los puntos principales del proceso de venta.

### Crear una cotizacion

1. Entre a **Cotizaciones**.
2. Presione **Nueva cotizacion**.
3. Seleccione el tipo de identificacion del cliente.
4. Escriba el numero de identificacion.
5. Presione **Consultar** si desea buscar datos existentes o consultar la integracion disponible.
6. Complete primer nombre, segundo nombre, primer apellido y segundo apellido.
7. Ingrese indicativo y telefono. Por defecto se usa el indicativo de Colombia **+57**.
8. Seleccione el producto que desea cotizar.
9. Digite la cuota inicial y el numero de cuotas.
10. Guarde la cotizacion.

### Que ocurre al guardar

- Se crea o actualiza el cliente con los datos correctos.
- Se guarda la cotizacion.
- Se abre una vista previa del PDF en pantalla.
- Desde la vista previa se decide si se descarga o se imprime.
- Si el producto tiene una foto principal compatible, se incluye en el PDF.
- Se puede crear seguimiento automatico para llamar al cliente.
- El negocio puede reflejarse en el pipeline comercial.

### Vista previa del PDF

Despues de guardar una cotizacion, el sistema no imprime ni descarga automaticamente. Primero muestra el PDF en pantalla para que el usuario revise datos del cliente, producto, valores, foto y condiciones. Si todo esta correcto, puede usar **Descargar PDF** o **Imprimir**.

## 11. Simulador financiero

![Simulador de credito en cotizacion](./assets/manual/05-cotizaciones-simulador.png)

### Para que sirve

El simulador ayuda a estimar rapidamente el valor de la cuota cuando el cliente quiere comprar a credito. Se usa dentro de la cotizacion para que el asesor pueda entregar una propuesta clara antes de continuar con la solicitud de credito.

### Como se usa

1. Seleccione el producto que el cliente desea cotizar.
2. Digite la cuota inicial que el cliente va a entregar.
3. Indique el numero de cuotas.
4. Agregue seguro o gastos adicionales si aplican.
5. Revise el total financiado y la cuota aproximada.

### Campos principales

- **Precio del producto:** valor base del producto.
- **Cuota inicial:** dinero que entrega el cliente al inicio.
- **Numero de cuotas:** cantidad de pagos que tendra la financiacion.
- **Seguro:** valor adicional del seguro, si aplica.
- **Gastos administrativos:** cobros asociados al proceso.
- **Total financiado:** valor que queda pendiente despues de la cuota inicial y cargos.
- **Cuota aproximada:** valor estimado de la cuota mensual.

### Recomendacion de uso

Explique al cliente que la cuota es aproximada y puede cambiar segun la aprobacion final, politicas de credito, documentos entregados o condiciones comerciales vigentes.

## 12. Solicitudes de credito

![Solicitudes credito](./assets/manual/06-solicitudes-credito.png)

### Para que sirve

Este modulo permite gestionar el proceso de credito del cliente despues de una cotizacion o negocio interesado.

### Crear o revisar una solicitud

1. Entre a **Solicitudes de credito**.
2. Seleccione o cree la solicitud del cliente.
3. Revise los datos del cliente.
4. Complete informacion laboral, financiera o comercial cuando aplique.
5. Registre codeudor y referencias si son requeridos.
6. Adjunte documentos.
7. Actualice el estado de aprobacion.
8. Guarde los cambios.

### Estados comunes

- **Pendiente:** aun falta informacion o revision.
- **En estudio:** la solicitud esta siendo evaluada.
- **Aprobada:** el credito fue aprobado.
- **Rechazada:** el credito no fue aprobado.

### Documentos

Desde la solicitud se pueden manejar documentos como:

- Cotizacion.
- Solicitud de credito.
- Autorizacion de tratamiento de datos.
- Carta de aprobacion.
- Orden de entrega.

## 13. Codeudor y referencias

### Para que sirve

El codeudor y las referencias ayudan a completar el estudio de credito cuando la politica comercial o financiera lo requiere.

### Cuando se diligencian

Se diligencian dentro de **Solicitudes de credito**, en la solicitud del cliente. Normalmente se completan despues de que el cliente acepta continuar con el proceso y antes de tomar una decision final.

### Recomendacion de uso

Registre datos claros y verificables. Si falta informacion, deje una actividad pendiente para solicitarla al cliente.

## 14. Entregas

### Para que sirve

Entregas permite registrar la entrega final del producto cuando el proceso comercial y de credito ya esta listo.

### Paso a paso

1. Entre a **Entregas**.
2. Seleccione la solicitud o venta aprobada.
3. Verifique datos del cliente y producto.
4. Complete los datos de entrega.
5. Revise el checklist.
6. Guarde la entrega.

### Recomendacion de uso

Use esta opcion solo cuando el producto realmente vaya a entregarse o ya haya sido entregado. Asi los reportes reflejan ventas cerradas correctamente.

## 15. Prospectos

![Prospectos](./assets/manual/07-prospectos.png)

### Para que sirve

Prospectos permite registrar personas interesadas que todavia no son clientes completos.

### Crear un prospecto

1. Entre a **Prospectos**.
2. Presione **Nuevo prospecto**.
3. Complete nombres, apellidos, telefono, correo y fuente del prospecto.
4. Seleccione la calificacion: frio, tibio o caliente.
5. Guarde.

### Convertir prospecto en cliente

Cuando un prospecto muestra interes real, use la opcion de convertir o llevar a cliente. El sistema permite continuar con la creacion del cliente y completar los datos faltantes.

## 16. Pipeline

![Pipeline](./assets/manual/08-pipeline.png)

### Para que sirve

El Pipeline muestra las oportunidades de venta por etapas. Funciona como un tablero para saber en que estado esta cada negocio.

### Estados comerciales sugeridos

- **Cotizado**
- **Interesado**
- **Documentos pendientes**
- **Credito en estudio**
- **Aprobado**
- **Rechazado**
- **Entregado**
- **Desistido**

### Mover una oportunidad

Puede mover una tarjeta arrastrandola de una columna a otra. Al hacerlo, el sistema actualiza la etapa del negocio y ajusta la probabilidad segun la etapa.

### Recomendacion de uso

Mantenga el pipeline actualizado todos los dias. Esto permite que el Dashboard y los reportes muestren informacion real.

## 17. Actividades

![Actividades](./assets/manual/09-actividades.png)

### Para que sirve

Actividades funciona como agenda comercial. Permite programar tareas, llamadas y reuniones para no perder seguimiento.

### Crear una actividad

1. Entre a **Actividades**.
2. Presione **Nueva actividad**.
3. Seleccione el cliente o negocio relacionado.
4. Escoja el tipo: tarea, llamada o reunion.
5. Defina fecha y hora.
6. Escriba la descripcion.
7. Guarde.

### Estados

- **Pendiente**
- **En proceso**
- **Completada**
- **Cancelada**

### Reprogramar una actividad

1. En la lista de actividades, presione el boton **Reprogramar**.
2. Use un acceso rapido como **Hoy**, **Manana**, **En 2 dias** o **Proxima semana**, si aplica.
3. Ajuste la fecha y la hora.
4. Revise el resumen de la nueva programacion.
5. Guarde el cambio.

Si la actividad tenia recordatorio, el sistema conserva la misma anticipacion frente a la nueva fecha programada.

### Seguimiento automatico

Al crear una cotizacion, el sistema puede generar una actividad automatica como **Llamar al cliente manana**. Si no se actualiza, puede aparecer como alerta de seguimiento vencido.

## 18. Reportes comerciales

![Reportes comerciales](./assets/manual/11-reportes.png)

### Para que sirve

Reportes permite revisar resultados comerciales y tomar decisiones con informacion consolidada.

### Indicadores disponibles

- Ventas por vendedor.
- Cotizaciones por estado.
- Tasa de conversion.
- Creditos aprobados y rechazados.
- Productos mas cotizados.

### Recomendacion de uso

El supervisor debe revisar reportes con frecuencia para detectar vendedores con oportunidades vencidas, productos mas consultados y cuellos de botella en credito.

## 19. Configuracion

![Configuracion](./assets/manual/10-configuracion.png)

### Para que sirve

Configuracion permite administrar datos generales del sistema. Esta opcion normalmente es usada por administradores.

### Opciones principales

- **Empresas:** crear y actualizar empresas. Al crear una empresa se puede cargar su logo.
- **Usuarios:** crear usuarios y asignarlos a una empresa.
- **Roles:** administrar permisos segun el perfil del usuario.
- **Etapas del pipeline:** configurar las columnas comerciales.
- **Configuracion financiera:** definir las condiciones usadas por la empresa para calcular cuotas en las cotizaciones.

### Configuracion financiera

1. Entre a **Configuracion**.
2. Busque la tarjeta **Configuracion financiera**.
3. Presione **Editar**.
4. Ajuste los parametros de financiacion autorizados por la empresa.
5. Verifique plazo maximo y redondeo de cuota.
6. Guarde.

Los cambios aplican a las nuevas cotizaciones de la empresa. Las cotizaciones ya creadas conservan los valores calculados al momento de generarse.

### Crear usuario

1. Entre a **Configuracion**.
2. Abra la seccion de usuarios.
3. Presione **Nuevo usuario**.
4. Complete nombres, correo y datos requeridos.
5. Seleccione la empresa a la que pertenece.
6. Asigne el rol.
7. Guarde.

## 20. Manejo de errores

El sistema muestra mensajes cuando algo no puede completarse.

### Que hacer si aparece un error

1. Lea el mensaje mostrado.
2. Revise campos obligatorios.
3. Valide que la informacion tenga el formato correcto.
4. Intente guardar nuevamente.
5. Si el error continua, tome una captura y reporte al administrador.

### Casos frecuentes

- Falta completar un campo obligatorio.
- El usuario no tiene permiso para la accion.
- La sesion expiro y debe iniciar sesion nuevamente.
- No hay conexion con el servidor.
- La integracion externa no esta configurada o no respondio.

## 21. Buenas practicas

- Registre clientes con identificacion correcta.
- No cree clientes duplicados.
- Complete telefono y correo siempre que sea posible.
- Cree actividades despues de cada contacto importante.
- Actualice el pipeline cuando cambie el estado real de la venta.
- Adjunte documentos en la solicitud de credito.
- Revise notificaciones internas al iniciar el dia.
- Use Cliente 360 antes de contactar al cliente.
- Mantenga productos y precios actualizados.
- Cierre sesion al terminar si usa un equipo compartido.

## 22. Flujo recomendado de trabajo

1. Ingresar al CRM.
2. Revisar Dashboard y alertas.
3. Crear o consultar cliente.
4. Crear cotizacion.
5. Hacer seguimiento con actividades.
6. Si el cliente continua, completar solicitud de credito.
7. Adjuntar documentos, codeudor y referencias si aplica.
8. Actualizar pipeline segun avance.
9. Registrar aprobacion o rechazo.
10. Si se concreta la venta, registrar entrega.
11. Revisar reportes para seguimiento.

## 23. Historial del manual

| Fecha | Version | Cambio |
| --- | --- | --- |
| 2026-06-09 | 5.8 | Se actualizan los pantallazos del manual desde el CRM publicado con una vista mas amplia de las pantallas principales. |
| 2026-06-09 | 5.7 | Se limpia el manual para usuario final, se actualiza el simulador de credito y se retiran referencias internas de configuracion financiera. |
| 2026-06-09 | 5.6 | Se mejora la reprogramacion de actividades con accesos rapidos de fecha, selector de hora y resumen antes de guardar. |
| 2026-06-09 | 5.5 | El boton de actividades ahora se llama Reprogramar y permite escoger fecha y hora de reprogramacion. |
| 2026-06-09 | 5.4 | Se aclara que la cotizacion calcula la cuota desde la tabla financiera de la empresa usando valor del producto, cuota inicial y numero de cuotas. |
| 2026-06-09 | 5.3 | Se agrega configuracion financiera por empresa para cotizaciones, con factor mensual, plazo maximo y redondeo de cuota. |
| 2026-06-09 | 5.2 | La cotizacion ya no descarga ni imprime automaticamente; primero abre vista previa del PDF y luego permite descargar o imprimir. |
| 2026-06-09 | 5.1 | Se documenta la carga de varias fotos por producto, seleccion de foto principal compatible para PDF y mejora visual del PDF de cotizacion. |
| 2026-06-09 | 5.0 | Se reconstruye el manual para usuario final, con explicaciones operativas, paso a paso y recomendaciones de uso. |
| 2026-05-29 | 4.0 | Se agrega movimiento drag and drop en el pipeline para arrastrar negocios entre etapas y actualizar estado/probabilidad automaticamente. |
| 2026-05-27 | 3.9 | La consulta con Verifik guarda automaticamente el cliente cuando la cedula no existia y evita duplicados al crear cotizaciones. |
