namespace CrmSaas.Application.Abstractions;

public interface ITenantContext
{
    Guid? EmpresaId { get; }
    string? Subdominio { get; }
    string UsuarioActual { get; }
    void SetTenant(Guid empresaId, string? subdominio = null);
}
