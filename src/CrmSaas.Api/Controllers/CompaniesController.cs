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
public sealed class CompaniesController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CompanyDto>>> Get(CancellationToken cancellationToken)
    {
        var companies = await db.Empresas.IgnoreQueryFilters()
            .OrderBy(x => x.Nombre)
            .Select(x => new CompanyDto(x.Id, x.Nombre, x.Subdominio, x.DominioPersonalizado, x.Activa))
            .ToListAsync(cancellationToken);
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(UpsertCompanyDto dto, CancellationToken cancellationToken)
    {
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
            Activa = dto.Active
        };

        db.Empresas.Add(company);
        await db.SaveChangesAsync(cancellationToken);
        await DatabaseSeeder.SeedCompanyDefaultsAsync(db, company.Id, cancellationToken);

        return Ok(new CompanyDto(company.Id, company.Nombre, company.Subdominio, company.DominioPersonalizado, company.Activa));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CompanyDto>> Update(Guid id, UpsertCompanyDto dto, CancellationToken cancellationToken)
    {
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
        company.Activa = dto.Active;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(new CompanyDto(company.Id, company.Nombre, company.Subdominio, company.DominioPersonalizado, company.Activa));
    }
}
