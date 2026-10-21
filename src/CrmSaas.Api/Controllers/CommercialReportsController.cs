using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using CrmSaas.Application.Abstractions;
using CrmSaas.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/commercial-reports")]
public sealed class CommercialReportsController(ICommercialReportService service, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CommercialReportsDto>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(from, to,
            CreditRoles.CanManage(User) || string.Equals(User.FindFirstValue("global_admin"), "true", StringComparison.OrdinalIgnoreCase)
                ? null : tenantContext.UsuarioActual, cancellationToken));
}
