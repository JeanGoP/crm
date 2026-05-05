using CrmSaas.Domain.Common;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Domain.Entities;

public sealed class Cliente : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? EmpresaCliente { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public EstadoCliente Estado { get; set; } = EstadoCliente.Activo;
    public string? Etiquetas { get; set; }
    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
    public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
}

public sealed class Prospecto : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
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
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public int? Cilindraje { get; set; }
    public int? Anio { get; set; }
    public string? Color { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
}

public sealed class Cotizacion : AuditableTenantEntity
{
    public string Numero { get; set; } = string.Empty;
    public TipoIdentificacionColombia TipoIdentificacion { get; set; }
    public string? NumeroIdentificacion { get; set; }
    public string NombresCliente { get; set; } = string.Empty;
    public string ApellidosCliente { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public decimal PrecioProducto { get; set; }
    public DateTime FechaCotizacion { get; set; } = DateTime.UtcNow;
    public DateTime ValidaHasta { get; set; } = DateTime.UtcNow.AddDays(7);
    public string? Observaciones { get; set; }
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
    public EstadoSolicitudCredito Estado { get; set; } = EstadoSolicitudCredito.Borrador;
    public string? Observaciones { get; set; }
    public ICollection<DocumentoSolicitudCredito> Documentos { get; set; } = new List<DocumentoSolicitudCredito>();
}

public sealed class DocumentoSolicitudCredito : AuditableTenantEntity
{
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public TipoDocumentoCredito Tipo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public EstadoDocumentoCredito Estado { get; set; } = EstadoDocumentoCredito.Pendiente;
    public DateTime? FechaRecepcion { get; set; }
    public string? Observaciones { get; set; }
    public string? NombreArchivo { get; set; }
    public string? RutaArchivo { get; set; }
    public string? ContentType { get; set; }
    public long? TamanoBytes { get; set; }
    public DateTime? FechaCarga { get; set; }
}
