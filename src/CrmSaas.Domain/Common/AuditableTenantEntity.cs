namespace CrmSaas.Domain.Common;

public abstract class AuditableTenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string UsuarioCreacion { get; set; } = "system";
    public string? UsuarioActualizacion { get; set; }
}
