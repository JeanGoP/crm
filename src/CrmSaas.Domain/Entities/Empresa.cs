using CrmSaas.Domain.Common;

namespace CrmSaas.Domain.Entities;

public sealed class Empresa : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Subdominio { get; set; } = string.Empty;
    public string? DominioPersonalizado { get; set; }
    public string? LogoDataUrl { get; set; }
    public string? LogoElectrodomesticosDataUrl { get; set; }
    public string? BaseDatosInventarioExterno { get; set; }
    public long? ErpEmpresaId { get; set; }
    public string? ErpEmpresaNit { get; set; }
    public string? ErpEmpresaNombre { get; set; }
    public bool Activa { get; set; } = true;
}
