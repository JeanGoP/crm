using System.Security.Claims;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Common;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Supervisor")]
[Route("api/access-logs")]
public sealed class AccessLogsController(CrmDbContext db) : ControllerBase
{
    private const string GlobalAdminEmail = "admin@demo.com";

    [HttpGet]
    public async Task<ActionResult<LoginAccessReportDto>> Get(CancellationToken cancellationToken)
    {
        var query = IsGlobalAdmin()
            ? db.IngresosPlataforma.IgnoreQueryFilters()
            : db.IngresosPlataforma.AsQueryable();

        var today = ColombiaTime.Today;
        var total = await query.CountAsync(cancellationToken);
        var successful = await query.CountAsync(x => x.Exitoso, cancellationToken);
        var failed = total - successful;
        var todayAccesses = await query.CountAsync(x => x.FechaIngreso >= today, cancellationToken);
        var lastAccessAt = await query
            .OrderByDescending(x => x.FechaIngreso)
            .Select(x => (DateTime?)x.FechaIngreso)
            .FirstOrDefaultAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.FechaIngreso)
            .Take(200)
            .Select(x => new LoginAccessDto(
                x.Id,
                x.UsuarioId,
                x.Usuario != null ? x.Usuario.NombreCompleto : x.NombreUsuario,
                x.Login,
                x.Email,
                x.EmpresaId,
                db.Empresas.IgnoreQueryFilters()
                    .Where(e => e.Id == x.EmpresaId)
                    .Select(e => e.Nombre)
                    .FirstOrDefault() ?? "Empresa",
                x.FechaIngreso,
                x.Exitoso,
                x.MotivoFallo,
                x.DireccionIp,
                x.UserAgent))
            .ToListAsync(cancellationToken);

        return Ok(new LoginAccessReportDto(total, successful, failed, todayAccesses, lastAccessAt, items));
    }

    private bool IsGlobalAdmin() =>
        string.Equals(User.FindFirstValue(ClaimTypes.Email), GlobalAdminEmail, StringComparison.OrdinalIgnoreCase);
}
