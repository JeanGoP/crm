namespace CrmSaas.Domain.Enums;

public enum EstadoCliente { Activo = 1, Inactivo = 2, Suspendido = 3 }
public enum CalificacionProspecto { Frio = 1, Tibio = 2, Caliente = 3 }
public enum EstadoActividad { Pendiente = 1, EnProceso = 2, Completada = 3, Cancelada = 4 }
public enum TipoActividad { Tarea = 1, Llamada = 2, Reunion = 3 }
public enum EstadoNegocio { Abierto = 1, Ganado = 2, Perdido = 3 }
public enum TipoIdentificacionColombia { CedulaCiudadania = 1, CedulaExtranjeria = 2, Nit = 3, Pasaporte = 4, TarjetaIdentidad = 5, PermisoProteccionTemporal = 6 }
public enum EstadoSolicitudCredito { Borrador = 1, DocumentosPendientes = 2, DocumentosRecibidos = 3, EnEstudio = 4, Aprobada = 5, Rechazada = 6, Desembolsada = 7 }
public enum TipoDocumentoCredito { Cedula = 1, SoporteIngresos = 2, ReciboServicio = 3, Referencias = 4, Otro = 5 }
public enum EstadoDocumentoCredito { Pendiente = 1, Recibido = 2, Validado = 3, Rechazado = 4 }
public enum EstadoEntregaMoto { Programada = 1, Entregada = 2, Cancelada = 3 }
