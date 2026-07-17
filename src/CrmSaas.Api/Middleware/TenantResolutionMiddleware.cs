using CrmSaas.Application.Abstractions;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Middleware;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, CrmDbContext db, ITenantContext tenantContext)
    {
        var isGlobalAdmin = string.Equals(context.User.FindFirst("global_admin")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var selectedCompany = context.Request.Headers["X-Company-Id"].FirstOrDefault();
        if (isGlobalAdmin && Guid.TryParse(selectedCompany, out var selectedCompanyId))
        {
            var empresa = await db.Empresas.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == selectedCompanyId && x.Activa, context.RequestAborted);
            if (empresa is not null)
            {
                tenantContext.SetTenant(empresa.Id, empresa.Subdominio);
                await next(context);
                return;
            }
        }

        var claimTenant = context.User.FindFirst("empresa_id")?.Value;
        if (Guid.TryParse(claimTenant, out var claimEmpresaId))
        {
            tenantContext.SetTenant(claimEmpresaId);
            await next(context);
            return;
        }

        var host = context.Request.Host.Host;
        var subdomain = host.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        var headerTenant = context.Request.Headers["X-Tenant"].FirstOrDefault();
        var tenantKey = headerTenant ?? subdomain;

        if (!string.IsNullOrWhiteSpace(tenantKey) && !tenantKey.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            var empresa = await db.Empresas.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Subdominio == tenantKey && x.Activa, context.RequestAborted);
            if (empresa is not null)
            {
                tenantContext.SetTenant(empresa.Id, empresa.Subdominio);
            }
        }

        await next(context);
    }
}
