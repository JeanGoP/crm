using CrmSaas.Domain.Common;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Domain.Entities;

public sealed class Cliente : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public TipoIdentificacionColombia? TipoIdentificacion { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string? EmpresaCliente { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IndicativoTelefono { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Ocupacion { get; set; }
    public EstadoCliente Estado { get; set; } = EstadoCliente.Activo;
    public string? Etiquetas { get; set; }
    public string? Observaciones { get; set; }
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
    public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
}

public sealed class Prospecto : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Fuente { get; set; } = string.Empty;
    public CalificacionProspecto Calificacion { get; set; } = CalificacionProspecto.Frio;
    public bool Convertido { get; set; }
    public Guid? ClienteId { get; set; }
}

public sealed class EtapaNegocio : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
    public decimal ProbabilidadPredeterminada { get; set; }
    public bool Activa { get; set; } = true;
}

public sealed class Negocio : AuditableTenantEntity
{
    public string Titulo { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid EtapaNegocioId { get; set; }
    public EtapaNegocio? EtapaNegocio { get; set; }
    public decimal Valor { get; set; }
    public decimal ProbabilidadCierre { get; set; }
    public DateTime FechaEstimadaCierre { get; set; }
    public EstadoNegocio Estado { get; set; } = EstadoNegocio.Abierto;
}

public sealed class Actividad : AuditableTenantEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoActividad Tipo { get; set; } = TipoActividad.Tarea;
    public EstadoActividad Estado { get; set; } = EstadoActividad.Pendiente;
    public DateTime FechaProgramada { get; set; }
    public DateTime? RecordatorioEn { get; set; }
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid? NegocioId { get; set; }
    public Negocio? Negocio { get; set; }
    public Guid? UsuarioAsignadoId { get; set; }
}

public sealed class Nota : AuditableTenantEntity
{
    public string Contenido { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid? ProspectoId { get; set; }
    public Guid? NegocioId { get; set; }
}

public sealed class Archivo : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public Guid? ClienteId { get; set; }
    public Guid? ProspectoId { get; set; }
    public Guid? NegocioId { get; set; }
}

public sealed class Producto : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = "Moto";
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string? Linea { get; set; }
    public string? Version { get; set; }
    public string Referencia { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? Cilindraje { get; set; }
    public int? Anio { get; set; }
    public string? Color { get; set; }
    public decimal Precio { get; set; }
    public decimal Soat { get; set; }
    public decimal Matricula { get; set; }
    public decimal Impuestos { get; set; }
    public string? FichaTecnica { get; set; }
    public DateTime? VigenteDesde { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<ProductoFoto> Fotos { get; set; } = [];
    public ICollection<ProductoPrecioSede> PreciosPorSede { get; set; } = [];
}

public sealed class CategoriaProducto : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool CotizarComoPaquete { get; set; }
    public bool Activa { get; set; } = true;
}

public sealed class ProductoFoto : AuditableTenantEntity
{
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public byte[] Datos { get; set; } = [];
    public bool EsPrincipalCotizacion { get; set; }
    public int Orden { get; set; }
}

public sealed class ProductoPrecioSede : AuditableTenantEntity
{
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public decimal Precio { get; set; }
    public DateTime? VigenteDesde { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class InventarioComercial : AuditableTenantEntity
{
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public string? Vin { get; set; }
    public string? NumeroChasis { get; set; }
    public string? NumeroMotor { get; set; }
    public string? Placa { get; set; }
    public string? Color { get; set; }
    public bool EsUsada { get; set; }
    public int? Kilometraje { get; set; }
    public EstadoInventarioComercial Estado { get; set; } = EstadoInventarioComercial.Disponible;
    public Guid? ClienteReservaId { get; set; }
    public Cliente? ClienteReserva { get; set; }
    public Guid? CotizacionReservaId { get; set; }
    public Cotizacion? CotizacionReserva { get; set; }
    public Guid? SolicitudCreditoReservaId { get; set; }
    public SolicitudCredito? SolicitudCreditoReserva { get; set; }
    public DateTime? FechaReserva { get; set; }
    public DateTime? FechaVencimientoReserva { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class ConfiguracionFinancieraEmpresa : AuditableTenantEntity
{
    public decimal SalarioMinimoVigente { get; set; } = 1400000;
    public decimal TasaConsumoEa { get; set; } = 29.72m;
    public decimal TasaBajoMontoEa { get; set; } = 56.33m;
    public decimal TasaFactorMensual { get; set; } = 4.5m;
    public int PlazoMaximoMeses { get; set; } = 30;
    public int RedondeoCuota { get; set; } = 1000;
    public bool UsarTablaMontelibano { get; set; } = true;
    public bool Activa { get; set; } = true;
}

public sealed class PuntoVenta : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string MarcaPrincipal { get; set; } = string.Empty;
    public string? LogoMarcaDataUrl { get; set; }
    public decimal TasaFactorMensual { get; set; } = 4.5m;
    public int PlazoMaximoMeses { get; set; } = 30;
    public int VigenciaCotizacionDias { get; set; } = 7;
    public string ModalidadEntrega { get; set; } = "ConSoat";
    public int TiempoSoatDias { get; set; } = 14;
    public int TiempoMatriculaDias { get; set; } = 20;
    public string? ProveedorSoat { get; set; }
    public string? TramitadorMatricula { get; set; }
    public string? CondicionesComerciales { get; set; }
    public string? BodegasInventarioExterno { get; set; }
    public bool Activa { get; set; } = true;
}

public sealed class Cotizacion : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public TipoIdentificacionColombia TipoIdentificacion { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string NombresCliente { get; set; } = string.Empty;
    public string ApellidosCliente { get; set; } = string.Empty;
    public string PrimerNombreCliente { get; set; } = string.Empty;
    public string? SegundoNombreCliente { get; set; }
    public string PrimerApellidoCliente { get; set; } = string.Empty;
    public string? SegundoApellidoCliente { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid? PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public Guid? PerfilRequisitoId { get; set; }
    public PerfilRequisito? PerfilRequisito { get; set; }
    public string? NombreSede { get; set; }
    public string? MarcaSede { get; set; }
    public string? ModalidadEntregaSede { get; set; }
    public decimal? TasaFactorMensualSede { get; set; }
    public int? PlazoMaximoMesesSede { get; set; }
    public int? VigenciaCotizacionDiasSede { get; set; }
    public string? CondicionesSede { get; set; }
    public Guid? PromocionId { get; set; }
    public Promocion? Promocion { get; set; }
    public string? NombrePromocion { get; set; }
    public decimal DescuentoPromocion { get; set; }
    public decimal PrecioProducto { get; set; }
    public decimal CuotaInicial { get; set; }
    public decimal Seguro { get; set; }
    public decimal GastosAdministrativos { get; set; }
    public int PlazoMeses { get; set; } = 24;
    public decimal TasaInteresMensual { get; set; }
    public decimal ValorFinanciado { get; set; }
    public decimal CuotaMensualEstimada { get; set; }
    public decimal TotalPagarEstimado { get; set; }
    public string? TipoCredito { get; set; }
    public bool UsoConfiguracionFinancieraEmpresa { get; set; }
    public DateTime FechaCotizacion { get; set; } = ColombiaTime.Now;
    public DateTime ValidaHasta { get; set; } = ColombiaTime.Now.AddDays(7);
    public string? Observaciones { get; set; }
    public ICollection<CotizacionItem> Items { get; set; } = [];
}

public sealed class PerfilRequisito : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsContado { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<DocumentoPerfilRequisito> Documentos { get; set; } = [];
}

public sealed class DocumentoPerfilRequisito : AuditableTenantEntity
{
    public Guid PerfilRequisitoId { get; set; }
    public PerfilRequisito? PerfilRequisito { get; set; }
    public TipoDocumentoCredito Tipo { get; set; } = TipoDocumentoCredito.Otro;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Obligatorio { get; set; } = true;
    public int Orden { get; set; }
}

public sealed class Promocion : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string TipoDescuento { get; set; } = "Valor";
    public decimal ValorDescuento { get; set; }
    public Guid? ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public string? Marca { get; set; }
    public string? Color { get; set; }
    public Guid? PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public DateTime VigenteDesde { get; set; } = ColombiaTime.Now.Date;
    public DateTime VigenteHasta { get; set; } = ColombiaTime.Now.Date.AddDays(30);
    public bool Activa { get; set; } = true;
}

public sealed class CotizacionItem : AuditableTenantEntity
{
    public Guid CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public int Orden { get; set; }
    public decimal PrecioProducto { get; set; }
    public Guid? PromocionId { get; set; }
    public Promocion? Promocion { get; set; }
    public string? NombrePromocion { get; set; }
    public decimal DescuentoPromocion { get; set; }
    public decimal CuotaInicial { get; set; }
    public decimal Seguro { get; set; }
    public decimal GastosAdministrativos { get; set; }
    public int PlazoMeses { get; set; } = 24;
    public decimal TasaInteresMensual { get; set; }
    public decimal ValorFinanciado { get; set; }
    public decimal CuotaMensualEstimada { get; set; }
    public decimal TotalPagarEstimado { get; set; }
    public string? TipoCredito { get; set; }
    public bool UsoConfiguracionFinancieraEmpresa { get; set; }
}

public sealed class SolicitudCredito : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid? CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }
    public Guid? PerfilRequisitoId { get; set; }
    public PerfilRequisito? PerfilRequisito { get; set; }
    public Guid? NegocioId { get; set; }
    public Negocio? Negocio { get; set; }
    public TipoIdentificacionColombia TipoIdentificacion { get; set; }
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string Celular { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Ocupacion { get; set; }
    public decimal IngresosMensuales { get; set; }
    public decimal CuotaInicial { get; set; }
    public int PlazoMeses { get; set; } = 24;
    public decimal ValorMoto { get; set; }
    public string? CodeudorNombre { get; set; }
    public string? CodeudorIdentificacion { get; set; }
    public string? CodeudorCelular { get; set; }
    public string? CodeudorParentesco { get; set; }
    public decimal? CodeudorIngresosMensuales { get; set; }
    public string? Referencia1Nombre { get; set; }
    public string? Referencia1Celular { get; set; }
    public string? Referencia1Relacion { get; set; }
    public string? Referencia2Nombre { get; set; }
    public string? Referencia2Celular { get; set; }
    public string? Referencia2Relacion { get; set; }
    public EstadoSolicitudCredito Estado { get; set; } = EstadoSolicitudCredito.Borrador;
    public string? Observaciones { get; set; }
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaInicioEstudio { get; set; }
    public DateTime? FechaRevisionPaso0 { get; set; }
    public bool RuntConsultado { get; set; }
    public bool SimitConsultado { get; set; }
    public bool IdentidadValidada { get; set; }
    public string? UsuarioPaso0 { get; set; }
    public string? ObservacionPaso0 { get; set; }
    public decimal? ValorAprobadoAnalista { get; set; }
    public decimal? CuotaInicialAprobada { get; set; }
    public int? PlazoAprobadoMeses { get; set; }
    public decimal? CuotaMensualAprobada { get; set; }
    public bool RequiereCodeudorParaAprobar { get; set; }
    public string? CondicionesFinales { get; set; }
    public string? ResultadoEstudio { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public DateTime? FechaDesembolso { get; set; }
    public string? UsuarioDecision { get; set; }
    public string? ObservacionDecision { get; set; }
    public ICollection<DocumentoSolicitudCredito> Documentos { get; set; } = new List<DocumentoSolicitudCredito>();
}

public sealed class DocumentoSolicitudCredito : AuditableTenantEntity
{
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public TipoDocumentoCredito Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public EstadoDocumentoCredito Estado { get; set; } = EstadoDocumentoCredito.Pendiente;
    public DateTime? FechaRecepcion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaRechazo { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaValidacion { get; set; }
    public string? UsuarioValidacion { get; set; }
    public string? NombreArchivo { get; set; }
    public string? RutaArchivo { get; set; }
    public string? ContentType { get; set; }
    public long? TamanoBytes { get; set; }
    public DateTime? FechaCarga { get; set; }
}

public sealed class EntregaMoto : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public DateTime FechaEntrega { get; set; } = ColombiaTime.Now;
    public string? AsesorResponsable { get; set; }
    public string? Vin { get; set; }
    public string? NumeroChasis { get; set; }
    public string? NumeroMotor { get; set; }
    public string? Placa { get; set; }
    public int? KilometrajeEntrega { get; set; }
    public bool CascoEntregado { get; set; }
    public bool SoatEntregado { get; set; }
    public bool MatriculaEntregada { get; set; }
    public bool ManualGarantiaEntregado { get; set; }
    public bool ActaEntregaFirmada { get; set; }
    public bool ChecklistPreEntregaCompletado { get; set; }
    public string? ProtocoloEntrega { get; set; }
    public string? FotoEntregaDataUrl { get; set; }
    public string? FotoEntregaNombre { get; set; }
    public DateTime? PrimeraRevisionProgramadaEn { get; set; }
    public Guid? ActividadPrimeraRevisionId { get; set; }
    public EstadoEntregaMoto Estado { get; set; } = EstadoEntregaMoto.Programada;
    public string? Observaciones { get; set; }
}

public sealed class OrdenRecaudo : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public DateTime FechaEmision { get; set; } = ColombiaTime.Now;
    public DateTime FechaVencimiento { get; set; } = ColombiaTime.Now.Date.AddDays(3);
    public decimal Total { get; set; }
    public decimal ValorPagado { get; set; }
    public DateTime? FechaPago { get; set; }
    public EstadoOrdenRecaudo Estado { get; set; } = EstadoOrdenRecaudo.Emitida;
    public string? Observaciones { get; set; }
    public ICollection<DetalleOrdenRecaudo> Detalles { get; set; } = new List<DetalleOrdenRecaudo>();
}

public sealed class DetalleOrdenRecaudo : AuditableTenantEntity
{
    public Guid OrdenRecaudoId { get; set; }
    public OrdenRecaudo? OrdenRecaudo { get; set; }
    public TipoConceptoRecaudo Tipo { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public sealed class Tramite : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public Guid? PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public TipoTramite Tipo { get; set; }
    public EstadoTramite Estado { get; set; } = EstadoTramite.Pendiente;
    public DateTime FechaInicio { get; set; } = ColombiaTime.Now;
    public DateTime FechaEstimada { get; set; } = ColombiaTime.Now.Date.AddDays(8);
    public DateTime? FechaFinalizacion { get; set; }
    public string? Responsable { get; set; }
    public string? Tercero { get; set; }
    public bool NotificarCliente { get; set; } = true;
    public DateTime? FechaNotificacionCliente { get; set; }
    public string? Observaciones { get; set; }
}
