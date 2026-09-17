using CrmSaas.Domain.Common;

namespace CrmSaas.Domain.Entities;

public sealed class CodeudorSolicitudCredito : AuditableTenantEntity
{
    public Guid SolicitudCreditoId { get; set; }
    public SolicitudCredito? SolicitudCredito { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string? Parentesco { get; set; }
    public decimal IngresosMensuales { get; set; }
    public string? Referencia1Nombre { get; set; }
    public string? Referencia1Celular { get; set; }
    public string? Referencia1Relacion { get; set; }
    public string? Referencia2Nombre { get; set; }
    public string? Referencia2Celular { get; set; }
    public string? Referencia2Relacion { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
