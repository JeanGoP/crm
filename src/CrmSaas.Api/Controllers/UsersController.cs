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
public sealed class UsersController(CrmDbContext db, IPasswordHasher passwordHasher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> Get(CancellationToken cancellationToken)
    {
        var users = await db.Usuarios.IgnoreQueryFilters()
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.PuntoVenta)
            .OrderBy(x => x.NombreCompleto)
            .Select(x => new UserDto(x.Id, x.NombreCompleto, x.Email, x.UsuarioRoles.Select(ur => ur.Rol!.Nombre).ToArray(), x.EmpresaId, x.PuntoVentaId, x.PuntoVenta == null ? null : x.PuntoVenta.Nombre))
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserDto dto, CancellationToken cancellationToken)
    {
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
        Guid? salesPointId = null;
        if (!isAdministrator)
        {
            if (!dto.SalesPointId.HasValue)
            {
                return BadRequest(new { detail = "Debe seleccionar una sede para usuarios vendedores o supervisores." });
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

        db.Usuarios.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        var salesPointName = salesPointId.HasValue
            ? await db.PuntosVenta.IgnoreQueryFilters().Where(x => x.Id == salesPointId.Value).Select(x => x.Nombre).FirstOrDefaultAsync(cancellationToken)
            : null;
        return Ok(new UserDto(user.Id, user.NombreCompleto, user.Email, roles.Select(x => x.Nombre).ToArray(), user.EmpresaId, salesPointId, salesPointName));
    }
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
