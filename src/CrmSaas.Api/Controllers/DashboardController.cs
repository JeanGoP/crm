using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken cancellationToken)
    {
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var canSeeAll = User.IsInRole("Administrador") || User.IsInRole("Supervisor")
            || string.Equals(User.FindFirstValue("global_admin"), "true", StringComparison.OrdinalIgnoreCase);
        return Ok(await service.GetAsync(userId, email, canSeeAll, cancellationToken));
    }
}
