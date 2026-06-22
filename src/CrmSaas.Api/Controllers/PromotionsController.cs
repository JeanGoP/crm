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
[Route("api/promotions")]
public sealed class PromotionsController(CrmDbContext db) : ControllerBase
{
    private static readonly string[] DiscountTypes = ["Valor", "Porcentaje"];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PromotionDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.Promociones
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .OrderByDescending(x => x.Activa)
            .ThenByDescending(x => x.VigenteHasta)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<PromotionDto>> Create(UpsertPromotionDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var code = NormalizeCode(dto.Code);
        if (await db.Promociones.AnyAsync(x => x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe una promocion con ese codigo." });
        }

        await ValidateScopeAsync(dto, cancellationToken);
        var entity = new Promocion();
        Apply(entity, dto, code);
        db.Promociones.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await LoadReferencesAsync(entity, cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<PromotionDto>> Update(Guid id, UpsertPromotionDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Promocion no encontrada.");
        var code = NormalizeCode(dto.Code);
        if (await db.Promociones.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe otra promocion con ese codigo." });
        }

        await ValidateScopeAsync(dto, cancellationToken);
        Apply(entity, dto, code);
        await db.SaveChangesAsync(cancellationToken);
        await LoadReferencesAsync(entity, cancellationToken);
        return Ok(ToDto(entity));
    }

    private async Task ValidateScopeAsync(UpsertPromotionDto dto, CancellationToken cancellationToken)
    {
        if (dto.ProductId.HasValue && !await db.Productos.AnyAsync(x => x.Id == dto.ProductId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }

        if (dto.SalesPointId.HasValue && !await db.PuntosVenta.AnyAsync(x => x.Id == dto.SalesPointId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Sede no encontrada.");
        }
    }

    private static void Apply(Promocion entity, UpsertPromotionDto dto, string code)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.TipoDescuento = NormalizeDiscountType(dto.DiscountType);
        entity.ValorDescuento = dto.DiscountValue;
        entity.ProductoId = dto.ProductId;
        entity.Marca = Normalize(dto.Brand);
        entity.Color = Normalize(dto.Color);
        entity.PuntoVentaId = dto.SalesPointId;
        entity.VigenteDesde = dto.ValidFrom.Date;
        entity.VigenteHasta = dto.ValidUntil.Date;
        entity.Activa = dto.Active;
    }

    private static void Validate(UpsertPromotionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationException("El nombre de la promocion es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new ValidationException("El codigo de la promocion es obligatorio.");
        if (!DiscountTypes.Contains(NormalizeDiscountType(dto.DiscountType))) throw new ValidationException("Tipo de descuento no valido.");
        if (dto.DiscountValue <= 0) throw new ValidationException("El descuento debe ser mayor a cero.");
        if (NormalizeDiscountType(dto.DiscountType) == "Porcentaje" && dto.DiscountValue > 100) throw new ValidationException("El porcentaje no puede superar 100%.");
        if (dto.ValidUntil.Date < dto.ValidFrom.Date) throw new ValidationException("La fecha final no puede ser anterior a la fecha inicial.");
    }

    private async Task LoadReferencesAsync(Promocion entity, CancellationToken cancellationToken)
    {
        if (entity.ProductoId.HasValue) await db.Entry(entity).Reference(x => x.Producto).LoadAsync(cancellationToken);
        if (entity.PuntoVentaId.HasValue) await db.Entry(entity).Reference(x => x.PuntoVenta).LoadAsync(cancellationToken);
    }

    private static PromotionDto ToDto(Promocion x) => new(
        x.Id,
        x.Nombre,
        x.Codigo,
        x.TipoDescuento,
        x.ValorDescuento,
        x.ProductoId,
        x.Producto is null ? null : ProductName(x.Producto),
        x.Marca,
        x.Color,
        x.PuntoVentaId,
        x.PuntoVenta?.Nombre,
        x.VigenteDesde,
        x.VigenteHasta,
        x.Activa);

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Linea} {product.Version} {product.Referencia}".Trim();
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant().Replace(" ", "_");

    private static string NormalizeDiscountType(string value) =>
        value.Trim().Equals("Porcentaje", StringComparison.OrdinalIgnoreCase) ? "Porcentaje" : "Valor";

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
