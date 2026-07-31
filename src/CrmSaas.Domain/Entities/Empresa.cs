using CrmSaas.Domain.Common;

namespace CrmSaas.Domain.Entities;

public sealed class Empresa : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Subdominio { get; set; } = string.Empty;
    public string? DominioPersonalizado { get; set; }
    public string? LogoDataUrl { get; set; }
    public string? BaseDatosInventarioExterno { get; set; }
    public string? BodegasInventarioExterno { get; set; }
    public bool Activa { get; set; } = true;
}
