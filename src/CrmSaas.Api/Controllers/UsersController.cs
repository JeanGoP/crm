using System.Security.Claims;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Auth;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/users")]
public sealed class UsersController(CrmDbContext db, IPasswordHasher passwordHasher, ITenantContext tenantContext) : ControllerBase
{
    private const string GlobalAdminEmail = "admin@demo.com";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> Get(CancellationToken cancellationToken)
    {
        var useSelectedCompany = IsGlobalAdmin() &&
            Request.Headers.ContainsKey("X-Company-Id") &&
            tenantContext.EmpresaId.HasValue;

        var query = IsGlobalAdmin()
            ? useSelectedCompany
                ? db.Usuarios.IgnoreQueryFilters().Where(x => x.EmpresaId == tenantContext.EmpresaId!.Value)
                : db.Usuarios.IgnoreQueryFilters()
            : db.Usuarios.AsQueryable();

        var users = await query
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.PuntoVenta)
            .Include(x => x.SedesSupervisadas).ThenInclude(x => x.PuntoVenta)
            .OrderBy(x => x.NombreCompleto)
            .Select(x => new UserDto(
                x.Id,
                x.NombreCompleto,
                x.Email,
                x.UsuarioRoles.Select(ur => ur.Rol!.Nombre).ToArray(),
                x.EmpresaId,
                x.PuntoVentaId,
                x.PuntoVenta == null ? null : x.PuntoVenta.Nombre,
                x.SedesSupervisadas.Select(s => s.PuntoVentaId).ToArray(),
                x.SedesSupervisadas.Where(s => s.PuntoVenta != null).Select(s => s.PuntoVenta!.Nombre).ToArray()))
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto, CancellationToken cancellationToken)
    {
        if (!CanManageCompany(dto.CompanyId))
        {
            return Forbid();
        }

        var companyExists = await db.Empresas.IgnoreQueryFilters().AnyAsync(x => x.Id == dto.CompanyId && x.Activa, cancellationToken);
        if (!companyExists)
        {
            return BadRequest(new { detail = "Empresa no encontrada o inactiva." });
        }

        var roles = await db.Roles.IgnoreQueryFilters()
            .Where(x => x.EmpresaId == dto.CompanyId && dto.Roles.Contains(x.Nombre))
            .ToListAsync(cancellationToken);
        if (roles.Count == 0)
        {
            return BadRequest(new { detail = "Selecciona al menos un rol valido para la empresa." });
        }

        var isAdministrator = roles.Any(x => x.Nombre == "Administrador");
        var isSupervisor = roles.Any(x => x.Nombre == "Supervisor");
        Guid? salesPointId = null;
        var supervisedSalesPointIds = Array.Empty<Guid>();
        if (isSupervisor)
        {
            supervisedSalesPointIds = (dto.SupervisedSalesPointIds ?? [])
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();
            if (supervisedSalesPointIds.Length == 0)
            {
                return BadRequest(new { detail = "Debe seleccionar al menos una sede para supervisar." });
            }

            var validSupervisedCount = await db.PuntosVenta.IgnoreQueryFilters()
                .CountAsync(x => x.EmpresaId == dto.CompanyId && x.Activa && supervisedSalesPointIds.Contains(x.Id), cancellationToken);
            if (validSupervisedCount != supervisedSalesPointIds.Length)
            {
                return BadRequest(new { detail = "Una de las sedes supervisadas no existe o esta inactiva para la empresa seleccionada." });
            }
        }
        else if (!isAdministrator)
        {
            if (!dto.SalesPointId.HasValue)
            {
                return BadRequest(new { detail = "Debe seleccionar una sede para el vendedor." });
            }

            salesPointId = await db.PuntosVenta.IgnoreQueryFilters()
                .Where(x => x.EmpresaId == dto.CompanyId && x.Id == dto.SalesPointId.Value && x.Activa)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!salesPointId.HasValue)
            {
                return BadRequest(new { detail = "Sede no encontrada o inactiva para la empresa seleccionada." });
            }
        }

        var user = new Usuario
        {
            EmpresaId = dto.CompanyId,
            NombreCompleto = dto.FullName,
            Email = dto.Email,
            PasswordHash = passwordHasher.Hash(dto.Password),
            PuntoVentaId = salesPointId,
            Activo = true
        };
        foreach (var role in roles)
        {
            user.UsuarioRoles.Add(new UsuarioRol { EmpresaId = dto.CompanyId, RolId = role.Id });
        }
        foreach (var supervisedSalesPointId in supervisedSalesPointIds)
        {
            user.SedesSupervisadas.Add(new UsuarioSedeSupervisada { EmpresaId = dto.CompanyId, UsuarioId = user.Id, PuntoVentaId = supervisedSalesPointId });
        }

        db.Usuarios.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        var salesPointName = salesPointId.HasValue
            ? await db.PuntosVenta.IgnoreQueryFilters().Where(x => x.Id == salesPointId.Value).Select(x => x.Nombre).FirstOrDefaultAsync(cancellationToken)
            : null;
        var supervisedSalesPointNames = supervisedSalesPointIds.Length == 0
            ? Array.Empty<string>()
            : await db.PuntosVenta.IgnoreQueryFilters()
                .Where(x => supervisedSalesPointIds.Contains(x.Id))
                .OrderBy(x => x.Nombre)
                .Select(x => x.Nombre)
                .ToArrayAsync(cancellationToken);
        return Ok(new UserDto(user.Id, user.NombreCompleto, user.Email, roles.Select(x => x.Nombre).ToArray(), user.EmpresaId, salesPointId, salesPointName, supervisedSalesPointIds, supervisedSalesPointNames));
    }

    private bool IsGlobalAdmin() =>
        string.Equals(User.FindFirstValue(ClaimTypes.Email), GlobalAdminEmail, StringComparison.OrdinalIgnoreCase);

    private bool CanManageCompany(Guid companyId) =>
        IsGlobalAdmin() || tenantContext.EmpresaId == companyId;
}

[ApiController]
[Authorize(Roles = "Administrador,Supervisor")]
[Route("api/roles")]
public sealed class RolesController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoleDto>>> Get(CancellationToken cancellationToken)
    {
        var roles = await db.Roles.OrderBy(x => x.Nombre).Select(x => new RoleDto(x.Id, x.Nombre, x.Descripcion)).ToListAsync(cancellationToken);
        return Ok(roles);
    }
}
