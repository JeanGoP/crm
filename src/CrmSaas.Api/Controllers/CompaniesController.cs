using System.Security.Claims;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/companies")]
public sealed class CompaniesController(CrmDbContext db, ITenantContext tenantContext) : ControllerBase
{
    private const string GlobalAdminEmail = "admin@demo.com";
    private const int MaxLogoDataUrlLength = 300000;
    private static readonly string[] AllowedLogoPrefixes =
    [
        "data:image/png;base64,",
        "data:image/jpeg;base64,",
        "data:image/webp;base64,"
    ];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompanyDto>>> Get(CancellationToken cancellationToken)
    {
        var query = IsGlobalAdmin()
            ? db.Empresas.IgnoreQueryFilters()
            : db.Empresas.AsQueryable();

        var companies = await query
            .OrderBy(x => x.Nombre)
            .Select(x => new CompanyDto(x.Id, x.Nombre, x.Subdominio, x.DominioPersonalizado, x.LogoDataUrl, x.Activa))
            .ToListAsync(cancellationToken);
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(UpsertCompanyDto dto, CancellationToken cancellationToken)
    {
        if (!IsGlobalAdmin())
        {
            return Forbid();
        }

        var subdomain = dto.Subdomain.Trim().ToLowerInvariant();
        if (await db.Empresas.IgnoreQueryFilters().AnyAsync(x => x.Subdominio == subdomain, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe una empresa con ese subdominio." });
        }

        var id = Guid.NewGuid();
        var company = new Empresa
        {
            Id = id,
            EmpresaId = id,
            Nombre = dto.Name.Trim(),
            Subdominio = subdomain,
            DominioPersonalizado = string.IsNullOrWhiteSpace(dto.CustomDomain) ? null : dto.CustomDomain.Trim(),
            LogoDataUrl = NormalizeLogo(dto.LogoDataUrl),
            Activa = dto.Active
        };

        db.Empresas.Add(company);
        await db.SaveChangesAsync(cancellationToken);
        await DatabaseSeeder.SeedCompanyDefaultsAsync(db, company.Id, cancellationToken);

        return Ok(ToDto(company));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, UpsertCompanyDto dto, CancellationToken cancellationToken)
    {
        if (!CanManageCompany(id))
        {
            return Forbid();
        }

        var company = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Empresa no encontrada.");
        var subdomain = dto.Subdomain.Trim().ToLowerInvariant();
        if (await db.Empresas.IgnoreQueryFilters().AnyAsync(x => x.Id != id && x.Subdominio == subdomain, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe una empresa con ese subdominio." });
        }

        company.Nombre = dto.Name.Trim();
        company.Subdominio = subdomain;
        company.DominioPersonalizado = string.IsNullOrWhiteSpace(dto.CustomDomain) ? null : dto.CustomDomain.Trim();
        company.LogoDataUrl = NormalizeLogo(dto.LogoDataUrl);
        company.Activa = dto.Active;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(company));
    }

    private static CompanyDto ToDto(Empresa company) =>
        new(company.Id, company.Nombre, company.Subdominio, company.DominioPersonalizado, company.LogoDataUrl, company.Activa);

    private bool IsGlobalAdmin() =>
        string.Equals(User.FindFirstValue(ClaimTypes.Email), GlobalAdminEmail, StringComparison.OrdinalIgnoreCase);

    private bool CanManageCompany(Guid companyId) =>
        IsGlobalAdmin() || tenantContext.EmpresaId == companyId;

    private static string? NormalizeLogo(string? logoDataUrl)
    {
        if (string.IsNullOrWhiteSpace(logoDataUrl))
        {
            return null;
        }

        var logo = logoDataUrl.Trim();
        if (logo.Length > MaxLogoDataUrlLength)
        {
            throw new InvalidOperationException("El logo es demasiado grande. Usa una imagen menor a 300 KB.");
        }

        if (!AllowedLogoPrefixes.Any(prefix => logo.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("El logo debe ser una imagen PNG, JPG o WebP.");
        }

        return logo;
    }

}
