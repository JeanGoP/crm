using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quote-charge-concepts")]
public sealed class QuoteChargeConceptsController(CrmDbContext db) : ControllerBase
{
    private static readonly string[] CalculationGroups = ["Seguro", "Gasto"];
    private static readonly string[] DefaultSources = ["Manual", "SoatProducto", "MatriculaProducto", "ImpuestosProducto", "ValorFijo"];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<QuoteChargeConceptDto>>> Get(CancellationToken cancellationToken)
    {
        await EnsureDefaultsAsync(cancellationToken);
        var rows = await db.ConceptosCotizacion
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.Orden)
            .ThenBy(x => x.Nombre)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<QuoteChargeConceptDto>> Create(UpsertQuoteChargeConceptDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var code = NormalizeCode(dto.Code);
        if (await db.ConceptosCotizacion.AnyAsync(x => x.Codigo == code, cancellationToken))
        {
            throw new ValidationException("Ya existe un concepto de cotizacion con ese codigo.");
        }

        var entity = new ConceptoCotizacion
        {
            Nombre = dto.Name.Trim(),
            Codigo = code,
            GrupoCalculo = NormalizeGroup(dto.CalculationGroup),
            FuenteValor = NormalizeSource(dto.DefaultValueSource),
            ValorPredeterminado = Math.Max(dto.DefaultAmount, 0),
            Orden = dto.Order <= 0 ? await NextOrderAsync(cancellationToken) : dto.Order,
            Activo = dto.Active
        };
        db.ConceptosCotizacion.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<QuoteChargeConceptDto>> Update(Guid id, UpsertQuoteChargeConceptDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.ConceptosCotizacion.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Concepto de cotizacion no encontrado.");
        var code = NormalizeCode(dto.Code);
        if (await db.ConceptosCotizacion.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
        {
            throw new ValidationException("Ya existe un concepto de cotizacion con ese codigo.");
        }

        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.GrupoCalculo = NormalizeGroup(dto.CalculationGroup);
        entity.FuenteValor = NormalizeSource(dto.DefaultValueSource);
        entity.ValorPredeterminado = Math.Max(dto.DefaultAmount, 0);
        entity.Orden = dto.Order <= 0 ? entity.Orden : dto.Order;
        entity.Activo = dto.Active;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<QuoteChargeConceptDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.ConceptosCotizacion.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Concepto de cotizacion no encontrado.");
        entity.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private async Task EnsureDefaultsAsync(CancellationToken cancellationToken)
    {
        if (await db.ConceptosCotizacion.AnyAsync(cancellationToken)) return;

        db.ConceptosCotizacion.AddRange(
            Default("Seguro", "SEGURO", "Seguro", "SoatProducto", 1),
            Default("Matricula", "MATRICULA", "Gasto", "MatriculaProducto", 2),
            Default("Impuestos", "IMPUESTOS", "Gasto", "ImpuestosProducto", 3));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> NextOrderAsync(CancellationToken cancellationToken)
    {
        var max = await db.ConceptosCotizacion.Select(x => (int?)x.Orden).MaxAsync(cancellationToken) ?? 0;
        return max + 1;
    }

    private static ConceptoCotizacion Default(string name, string code, string group, string source, int order) => new()
    {
        Nombre = name,
        Codigo = code,
        GrupoCalculo = group,
        FuenteValor = source,
        Orden = order,
        Activo = true
    };

    private static QuoteChargeConceptDto ToDto(ConceptoCotizacion concept) => new(
        concept.Id,
        concept.Nombre,
        concept.Codigo,
        concept.GrupoCalculo,
        concept.FuenteValor,
        concept.ValorPredeterminado,
        concept.Orden,
        concept.Activo);

    private static void Validate(UpsertQuoteChargeConceptDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre del concepto es obligatorio.");
        if (dto.Name.Length > 80) throw new ValidationException("El nombre no puede superar 80 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ValidationException("El codigo del concepto es obligatorio.");
        if (dto.Code.Length > 40) throw new ValidationException("El codigo no puede superar 40 caracteres.");
        _ = NormalizeGroup(dto.CalculationGroup);
        _ = NormalizeSource(dto.DefaultValueSource);
        if (dto.DefaultAmount < 0) throw new ValidationException("El valor predeterminado no puede ser negativo.");
    }

    private static string NormalizeCode(string value) =>
        value.Trim().ToUpperInvariant().Replace(" ", "_");

    private static string NormalizeGroup(string value)
    {
        var normalized = CalculationGroups.FirstOrDefault(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
        return normalized ?? throw new ValidationException("El grupo debe ser Seguro o Gasto.");
    }

    private static string NormalizeSource(string value)
    {
        var normalized = DefaultSources.FirstOrDefault(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
        return normalized ?? throw new ValidationException("La fuente del valor no es valida.");
    }
}
