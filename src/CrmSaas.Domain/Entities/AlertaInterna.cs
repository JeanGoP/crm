using CrmSaas.Domain.Common;

namespace CrmSaas.Domain.Entities;

public sealed class AlertaInterna : AuditableTenantEntity
{
    public Guid UsuarioId { get; set; }
    public Guid SolicitudCreditoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime? FechaLectura { get; set; }
}
