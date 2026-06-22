using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/requirement-profiles")]
public sealed class RequirementProfilesController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RequirementProfileDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.PerfilesRequisito
            .Include(x => x.Documentos)
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<RequirementProfileDto>> Create(UpsertRequirementProfileDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var code = NormalizeCode(dto.Code);
        if (await db.PerfilesRequisito.AnyAsync(x => x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe un perfil con ese codigo." });
        }

        var entity = new PerfilRequisito();
        Apply(entity, dto, code);
        db.PerfilesRequisito.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<RequirementProfileDto>> Update(Guid id, UpsertRequirementProfileDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.PerfilesRequisito
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Perfil de requisitos no encontrado.");
        var code = NormalizeCode(dto.Code);
        if (await db.PerfilesRequisito.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe otro perfil con ese codigo." });
        }

        Apply(entity, dto, code);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private static void Apply(PerfilRequisito entity, UpsertRequirementProfileDto dto, string code)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.Descripcion = Normalize(dto.Description);
        entity.EsContado = dto.IsCash;
        entity.Activo = dto.Active;
        entity.Documentos.Clear();

        foreach (var document in dto.Documents.OrderBy(x => x.Order).ThenBy(x => x.Name))
        {
            entity.Documentos.Add(new DocumentoPerfilRequisito
            {
                Tipo = document.Type,
                Nombre = document.Name.Trim(),
                Descripcion = Normalize(document.Description),
                Obligatorio = document.Required,
                Orden = document.Order
            });
        }
    }

    private static void Validate(UpsertRequirementProfileDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre del perfil es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ValidationException("El codigo del perfil es obligatorio.");
        if (dto.Documents.Count == 0) throw new ValidationException("El perfil debe tener al menos un documento.");
        foreach (var document in dto.Documents)
        {
            if (string.IsNullOrWhiteSpace(document.Name)) throw new ValidationException("Todos los documentos deben tener nombre.");
            if (document.Order <= 0) throw new ValidationException("El orden de los documentos debe ser mayor a cero.");
            if (!Enum.IsDefined(typeof(TipoDocumentoCredito), document.Type)) throw new ValidationException("Tipo de documento no valido.");
        }
    }

    private static RequirementProfileDto ToDto(PerfilRequisito profile) => new(
        profile.Id,
        profile.Nombre,
        profile.Codigo,
        profile.Descripcion,
        profile.EsContado,
        profile.Activo,
        profile.Documentos
            .OrderBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .Select(x => new RequirementDocumentDto(x.Id, x.Tipo, x.Nombre, x.Descripcion, x.Obligatorio, x.Orden))
            .ToList());

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant().Replace(" ", "_");

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
