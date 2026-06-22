using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrmSaas.Infrastructure.Auth;

public sealed class AuthService(CrmSaas.Infrastructure.Persistence.CrmDbContext db, ITenantContext tenantContext, IPasswordHasher passwordHasher, IOptions<JwtOptions> options) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (!tenantContext.EmpresaId.HasValue)
        {
            var empresa = await db.Empresas.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Subdominio == request.Tenant, cancellationToken)
                ?? throw new UnauthorizedAccessException("Empresa no encontrada.");
            tenantContext.SetTenant(empresa.Id, empresa.Subdominio);
        }

        var user = await db.Usuarios
            .Include(x => x.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.Activo, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciales invalidas.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales invalidas.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var refreshToken = await db.RefreshTokens
            .Include(x => x.Usuario).ThenInclude(x => x!.UsuarioRoles).ThenInclude(x => x.Rol)
            .Include(x => x.Usuario).ThenInclude(x => x!.PuntoVenta)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.Activo, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token invalido.");

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
            new(ClaimTypes.Email, user.Email),
            new("empresa_id", user.EmpresaId.ToString())
        };
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

        return new AuthResponseDto(accessToken, refreshToken, expires, new UserDto(user.Id, user.NombreCompleto, user.Email, roles, user.EmpresaId, user.PuntoVentaId, user.PuntoVenta?.Nombre));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
