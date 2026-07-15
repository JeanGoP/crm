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
[Route("api/product-categories")]
public sealed class ProductCategoriesController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductCategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var categories = await db.CategoriasProducto
            .OrderByDescending(x => x.Activa)
            .ThenBy(x => x.Nombre)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductCategoryDto>> Create(UpsertProductCategoryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var name = NormalizeName(dto.Name);
        if (await db.CategoriasProducto.AnyAsync(x => x.Nombre == name, cancellationToken))
        {
            throw new ValidationException("Ya existe una categoria con ese nombre.");
        }

        var category = new CategoriaProducto
        {
            Nombre = name,
            Descripcion = NormalizeOptional(dto.Description),
            CotizarComoPaquete = dto.QuoteAsBundle,
            Activa = dto.Active
        };
        db.CategoriasProducto.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(category));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductCategoryDto>> Update(Guid id, UpsertProductCategoryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var category = await db.CategoriasProducto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Categoria no encontrada.");
        var name = NormalizeName(dto.Name);
        if (await db.CategoriasProducto.AnyAsync(x => x.Id != id && x.Nombre == name, cancellationToken))
        {
            throw new ValidationException("Ya existe una categoria con ese nombre.");
        }

        category.Nombre = name;
        category.Descripcion = NormalizeOptional(dto.Description);
        category.CotizarComoPaquete = dto.QuoteAsBundle;
        category.Activa = dto.Active;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(category));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductCategoryDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.CategoriasProducto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Categoria no encontrada.");
        category.Activa = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(category));
    }

    private static ProductCategoryDto ToDto(CategoriaProducto category) => new(
        category.Id,
        category.Nombre,
        category.Descripcion,
        category.CotizarComoPaquete,
        category.Activa);

    private static void Validate(UpsertProductCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre de la categoria es obligatorio.");
        if (dto.Name.Length > 80) throw new ValidationException("El nombre de la categoria no puede superar 80 caracteres.");
    }

    private static string NormalizeName(string value) => value.Trim();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
