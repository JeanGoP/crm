using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Common;
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
        ApplyProfile(entity, dto, code);
        AddDocuments(entity, dto.Documents);
        db.PerfilesRequisito.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<RequirementProfileDto>> Update(Guid id, UpsertRequirementProfileDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<ActionResult<RequirementProfileDto>>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var code = NormalizeCode(dto.Code);
            if (!await db.PerfilesRequisito.AnyAsync(x => x.Id == id, cancellationToken))
            {
                throw new KeyNotFoundException("Perfil de requisitos no encontrado.");
            }

            if (await db.PerfilesRequisito.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
            {
                return BadRequest(new { detail = "Ya existe otro perfil con ese codigo." });
            }

            // Las operaciones directas evitan que EF conserve documentos eliminados
            // en el ChangeTracker y luego intente modificarlos o borrarlos otra vez.
            var updatedAt = ColombiaTime.Now;
            var updatedBy = User.Identity?.Name ?? "system";
            await db.PerfilesRequisito
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Nombre, dto.Name.Trim())
                    .SetProperty(x => x.Codigo, code)
                    .SetProperty(x => x.Descripcion, Normalize(dto.Description))
                    .SetProperty(x => x.EsContado, dto.IsCash)
                    .SetProperty(x => x.Activo, dto.Active)
                    .SetProperty(x => x.FechaActualizacion, updatedAt)
                    .SetProperty(x => x.UsuarioActualizacion, updatedBy),
                    cancellationToken);

            await db.DocumentosPerfilRequisito
                .Where(x => x.PerfilRequisitoId == id)
                .ExecuteDeleteAsync(cancellationToken);

            db.DocumentosPerfilRequisito.AddRange(CreateDocuments(id, dto.Documents));

            await db.SaveChangesAsync(cancellationToken);
            var updated = await db.PerfilesRequisito
                .AsNoTracking()
                .Include(x => x.Documentos)
                .SingleAsync(x => x.Id == id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Ok(ToDto(updated));
        });
    }

    private static void ApplyProfile(PerfilRequisito entity, UpsertRequirementProfileDto dto, string code)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.Descripcion = Normalize(dto.Description);
        entity.EsContado = dto.IsCash;
        entity.Activo = dto.Active;
    }

    private static void AddDocuments(PerfilRequisito entity, IEnumerable<UpsertRequirementDocumentDto> documents)
    {
        foreach (var document in CreateDocuments(entity.Id, documents))
        {
            entity.Documentos.Add(document);
        }
    }

    private static IEnumerable<DocumentoPerfilRequisito> CreateDocuments(
        Guid profileId,
        IEnumerable<UpsertRequirementDocumentDto> documents) =>
        documents
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(document => new DocumentoPerfilRequisito
            {
                PerfilRequisitoId = profileId,
                Tipo = document.Type,
                Nombre = document.Name.Trim(),
                Descripcion = Normalize(document.Description),
                Obligatorio = document.Required,
                Orden = document.Order
            });

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
