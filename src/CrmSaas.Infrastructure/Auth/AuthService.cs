using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrmSaas.Infrastructure.Auth;

public sealed class AuthService(CrmSaas.Infrastructure.Persistence.CrmDbContext db, ITenantContext tenantContext, IPasswordHasher passwordHasher, IOptions<JwtOptions> options, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private const string GlobalAdminEmail = "admin@demo.com";

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var credential = NormalizeLogin(request.Login ?? request.Email ?? string.Empty);
        var candidates = await db.Usuarios.IgnoreQueryFilters()
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.PuntoVenta)
            .Include(x => x.SedesSupervisadas).ThenInclude(x => x.PuntoVenta)
            .Where(x => x.Activo && (x.Login == credential || x.Email == credential))
            .ToListAsync(cancellationToken);

        var validUsers = candidates
            .Where(user => passwordHasher.Verify(request.Password, user.PasswordHash))
            .ToList();

        if (validUsers.Count == 0)
        {
            await RegisterLoginAccessAsync(candidates.Count == 1 ? candidates[0] : null, credential, false, "Credenciales invalidas.", cancellationToken);
            throw new UnauthorizedAccessException("Credenciales invalidas.");
        }

        if (validUsers.Count > 1)
        {
            await RegisterLoginAccessAsync(null, credential, false, "Login duplicado en varias empresas.", cancellationToken);
            throw new UnauthorizedAccessException("El usuario existe en mas de una empresa. Solicite al administrador asignar un login unico.");
        }

        var user = validUsers[0];
        var empresa = await db.Empresas.IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == user.EmpresaId && x.Activa, cancellationToken);
        if (empresa is null)
        {
            await RegisterLoginAccessAsync(user, credential, false, "Empresa no encontrada o inactiva.", cancellationToken);
            throw new UnauthorizedAccessException("Empresa no encontrada o inactiva.");
        }

        tenantContext.SetTenant(empresa.Id, empresa.Subdominio);
        await RegisterLoginAccessAsync(user, credential, true, null, cancellationToken);
        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var now = DateTime.UtcNow;
        var refreshToken = await db.RefreshTokens.IgnoreQueryFilters()
            .Include(x => x.Usuario).ThenInclude(x => x!.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.Usuario).ThenInclude(x => x!.PuntoVenta)
            .Include(x => x.Usuario).ThenInclude(x => x!.SedesSupervisadas).ThenInclude(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevocadoEn == null && x.ExpiraEn > now && x.Usuario != null && x.Usuario.Activo, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token invalido.");

        tenantContext.SetTenant(refreshToken.EmpresaId);
        refreshToken.RevocadoEn = DateTime.UtcNow;
        return await IssueTokensAsync(refreshToken.Usuario!, cancellationToken);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(Usuario user, CancellationToken cancellationToken)
    {
        var jwt = options.Value;
        var roles = user.UsuarioRoles.Select(x => x.Rol!.Nombre).ToArray();
        var expires = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Email, user.Email),
            new("empresa_id", user.EmpresaId.ToString())
        };
        if (string.Equals(user.Email, GlobalAdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim("global_admin", "true"));
        }
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: expires, signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiraEn = DateTime.UtcNow.AddDays(jwt.RefreshTokenDays),
            EmpresaId = user.EmpresaId
        });
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            refreshToken,
            expires,
            new UserDto(
                user.Id,
                user.NombreCompleto,
                user.Login,
                user.Email,
                roles,
                user.EmpresaId,
                user.PuntoVentaId,
                user.PuntoVenta?.Nombre,
                user.SedesSupervisadas.Select(x => x.PuntoVentaId).ToArray(),
                user.SedesSupervisadas.Where(x => x.PuntoVenta != null).Select(x => x.PuntoVenta!.Nombre).ToArray()));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private async Task RegisterLoginAccessAsync(Usuario? user, string credential, bool successful, string? failureReason, CancellationToken cancellationToken)
    {
        var companyId = user?.EmpresaId ?? tenantContext.EmpresaId;
        if (!companyId.HasValue)
        {
            return;
        }

        var context = httpContextAccessor.HttpContext;
        db.IngresosPlataforma.Add(new IngresoPlataforma
        {
            EmpresaId = companyId.Value,
            UsuarioId = user?.Id,
            NombreUsuario = user?.NombreCompleto ?? credential,
            Login = credential,
            Email = user?.Email ?? (credential.Contains('@') ? credential : null),
            FechaIngreso = ColombiaTime.Now,
            Exitoso = successful,
            MotivoFallo = failureReason,
            DireccionIp = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                ?? context?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(context?.Request.Headers.UserAgent.FirstOrDefault(), 600)
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Length <= maxLength ? value : value[..maxLength];

    private static string NormalizeLogin(string value) => value.Trim().ToLowerInvariant();
}
