using System.Security.Claims;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Common;
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
        var query = IsGlobalAdmin()
            ? db.Usuarios.IgnoreQueryFilters()
            : db.Usuarios.AsQueryable();

        var users = await query
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.PuntoVenta)
            .Include(x => x.SedesSupervisadas).ThenInclude(x => x.PuntoVenta)
            .OrderBy(x => x.NombreCompleto)
            .Select(x => new UserDto(
                x.Id,
                x.NombreCompleto,
                x.Login,
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
        if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest(new { detail = "El nombre completo es obligatorio." });
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest(new { detail = "El email es obligatorio." });
        if (string.IsNullOrWhiteSpace(dto.Login)) return BadRequest(new { detail = "El login es obligatorio." });
        if (UserPasswordPolicy.IsSuperUser(dto.Email) || UserPasswordPolicy.IsSuperUser(dto.Login))
            return BadRequest(new { detail = "Ese identificador está reservado para el superusuario existente." });

        if (!CanManageCompany(dto.CompanyId))
        {
            return Forbid();
        }

        var companyExists = await db.Empresas.IgnoreQueryFilters().AnyAsync(x => x.Id == dto.CompanyId && x.Activa, cancellationToken);
        if (!companyExists)
        {
            return BadRequest(new { detail = "Empresa no encontrada o inactiva." });
        }

        var login = NormalizeLogin(dto.Login);
        if (login.Length > 80)
        {
            return BadRequest(new { detail = "El login no puede superar 80 caracteres." });
        }

        if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(x => x.Login == login, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe un usuario con ese login." });
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
            Login = login,
            Email = dto.Email,
            PasswordHash = passwordHasher.Hash(UserPasswordPolicy.SharedPassword),
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
        return Ok(new UserDto(user.Id, user.NombreCompleto, user.Login, user.Email, roles.Select(x => x.Nombre).ToArray(), user.EmpresaId, salesPointId, salesPointName, supervisedSalesPointIds, supervisedSalesPointNames));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName)) return BadRequest(new { detail = "El nombre completo es obligatorio." });
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest(new { detail = "El email es obligatorio." });
        if (string.IsNullOrWhiteSpace(dto.Login)) return BadRequest(new { detail = "El login es obligatorio." });

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<ActionResult<UserDto>>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var userQuery = IsGlobalAdmin()
                ? db.Usuarios.IgnoreQueryFilters()
                : db.Usuarios.AsQueryable();
            var existingUser = await userQuery
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.EmpresaId, x.Email, x.PasswordHash })
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            if (UserPasswordPolicy.IsSuperUser(existingUser.Email) && !IsGlobalAdmin()) return Forbid();
            if (UserPasswordPolicy.IsSuperUser(existingUser.Email) != UserPasswordPolicy.IsSuperUser(dto.Email)
                || (!UserPasswordPolicy.IsSuperUser(existingUser.Email) && UserPasswordPolicy.IsSuperUser(dto.Login)))
                return BadRequest(new { detail = "No se puede asignar ni cambiar el identificador reservado del superusuario." });

            if (!CanManageCompany(existingUser.EmpresaId) || !CanManageCompany(dto.CompanyId))
            {
                return Forbid();
            }

            var companyExists = await db.Empresas.IgnoreQueryFilters().AnyAsync(x => x.Id == dto.CompanyId && x.Activa, cancellationToken);
            if (!companyExists)
            {
                return BadRequest(new { detail = "Empresa no encontrada o inactiva." });
            }

            var login = NormalizeLogin(dto.Login);
            if (login.Length > 80)
            {
                return BadRequest(new { detail = "El login no puede superar 80 caracteres." });
            }

            if (await db.Usuarios.IgnoreQueryFilters().AnyAsync(x => x.Id != id && x.Login == login, cancellationToken))
            {
                return BadRequest(new { detail = "Ya existe un usuario con ese login." });
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

            var updatedAt = ColombiaTime.Now;
            var updatedBy = User.Identity?.Name ?? "system";
            var normalizedFullName = dto.FullName.Trim();
            var normalizedEmail = dto.Email.Trim();
            var targetUser = db.Usuarios.IgnoreQueryFilters().Where(x => x.Id == id);
            var passwordHash = UserPasswordPolicy.HashForUpdate(existingUser.Email, existingUser.PasswordHash, passwordHasher);
            var affectedRows = await targetUser.ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EmpresaId, dto.CompanyId)
                    .SetProperty(x => x.NombreCompleto, normalizedFullName)
                    .SetProperty(x => x.Login, login)
                    .SetProperty(x => x.Email, normalizedEmail)
                    .SetProperty(x => x.PasswordHash, passwordHash)
                    .SetProperty(x => x.PuntoVentaId, salesPointId)
                    .SetProperty(x => x.FechaActualizacion, updatedAt)
                    .SetProperty(x => x.UsuarioActualizacion, updatedBy), cancellationToken);
            if (affectedRows != 1)
            {
                throw new KeyNotFoundException("Usuario no encontrado.");
            }

            await db.UsuarioRoles.IgnoreQueryFilters()
                .Where(x => x.UsuarioId == id)
                .ExecuteDeleteAsync(cancellationToken);
            await db.UsuariosSedesSupervisadas.IgnoreQueryFilters()
                .Where(x => x.UsuarioId == id)
                .ExecuteDeleteAsync(cancellationToken);

            db.UsuarioRoles.AddRange(roles.Select(role => new UsuarioRol
            {
                EmpresaId = dto.CompanyId,
                UsuarioId = id,
                RolId = role.Id
            }));
            db.UsuariosSedesSupervisadas.AddRange(supervisedSalesPointIds.Select(supervisedSalesPointId => new UsuarioSedeSupervisada
            {
                EmpresaId = dto.CompanyId,
                UsuarioId = id,
                PuntoVentaId = supervisedSalesPointId
            }));
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

            await transaction.CommitAsync(cancellationToken);
            return Ok(new UserDto(id, normalizedFullName, login, normalizedEmail, roles.Select(x => x.Nombre).ToArray(), dto.CompanyId, salesPointId, salesPointName, supervisedSalesPointIds, supervisedSalesPointNames));
        });
    }

    private bool IsGlobalAdmin() =>
        string.Equals(User.FindFirstValue(ClaimTypes.Email), GlobalAdminEmail, StringComparison.OrdinalIgnoreCase);

    private bool CanManageCompany(Guid companyId) =>
        IsGlobalAdmin() || tenantContext.EmpresaId == companyId;

    private static string NormalizeLogin(string value) => value.Trim().ToLowerInvariant();
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
