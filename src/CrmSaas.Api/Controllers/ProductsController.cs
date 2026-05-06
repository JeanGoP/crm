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
[Route("api/products")]
public sealed class ProductsController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProductDto>>> Get(CancellationToken cancellationToken)
    {
        var products = await db.Productos
            .OrderBy(x => x.Categoria).ThenBy(x => x.Nombre)
            .Select(x => new ProductDto(x.Id, x.Nombre, x.Categoria, x.Marca, x.Modelo, x.Referencia, x.Descripcion, x.Cilindraje, x.Anio, x.Color, x.Precio, x.Activo))
            .ToListAsync(cancellationToken);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Create(UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var product = new Producto
        {
            Nombre = dto.Name.Trim(),
            Categoria = NormalizeCategory(dto.Category),
            Marca = dto.Brand.Trim(),
            Modelo = dto.Model.Trim(),
            Referencia = dto.Reference.Trim(),
            Descripcion = dto.Description,
            Cilindraje = dto.EngineCc,
            Anio = dto.Year,
            Color = dto.Color,
            Precio = dto.Price,
            Activo = dto.Active
        };
        db.Productos.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpsertProductDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var product = await db.Productos.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Producto no encontrado.");
        product.Nombre = dto.Name.Trim();
        product.Categoria = NormalizeCategory(dto.Category);
        product.Marca = dto.Brand.Trim();
        product.Modelo = dto.Model.Trim();
        product.Referencia = dto.Reference.Trim();
        product.Descripcion = dto.Description;
        product.Cilindraje = dto.EngineCc;
        product.Anio = dto.Year;
        product.Color = dto.Color;
        product.Precio = dto.Price;
        product.Activo = dto.Active;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<ProductDto>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Productos.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Producto no encontrado.");
        product.Activo = false;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(product));
    }

    private static ProductDto ToDto(Producto x) => new(x.Id, x.Nombre, x.Categoria, x.Marca, x.Modelo, x.Referencia, x.Descripcion, x.Cilindraje, x.Anio, x.Color, x.Precio, x.Activo);

    private static void Validate(UpsertProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Category)) throw new ValidationException("La categoria es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.Reference)) throw new ValidationException("La referencia es obligatoria.");
        if (dto.Price <= 0) throw new ValidationException("El precio debe ser mayor a cero.");
    }

    private static string NormalizeCategory(string category) => string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
}
