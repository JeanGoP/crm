using System.Security.Claims;
using CrmSaas.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CrmSaas.Infrastructure.Tenancy;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private Guid? _empresaId;
    private string? _subdominio;

    public Guid? EmpresaId => _empresaId ?? TryGetCompanyClaim();
    public string? Subdominio => _subdominio;
    public string UsuarioActual => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? "system";

    public void SetTenant(Guid empresaId, string? subdominio = null)
    {
        _empresaId = empresaId;
        _subdominio = subdominio;
    }

    private Guid? TryGetCompanyClaim()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue("empresa_id");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
