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
            .Include(x => x.Sedes)
                .ThenInclude(x => x.PuntoVenta)
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
        var salesPointIds = SalesPointIds(dto);
        Apply(entity, dto, code, salesPointIds);
        AddSalesPoints(entity, salesPointIds);
        db.Promociones.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(await GetByIdAsync(entity.Id, cancellationToken)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<PromotionDto>> Update(Guid id, UpsertPromotionDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<ActionResult<PromotionDto>>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var entity = await db.Promociones.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Promocion no encontrada.");
            var code = NormalizeCode(dto.Code);
            if (await db.Promociones.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
            {
                return BadRequest(new { detail = "Ya existe otra promocion con ese codigo." });
            }

            await ValidateScopeAsync(dto, cancellationToken);
            var salesPointIds = SalesPointIds(dto);
            Apply(entity, dto, code, salesPointIds);
            await db.PromocionesPuntosVenta
                .Where(x => x.PromocionId == id)
                .ExecuteDeleteAsync(cancellationToken);
            db.PromocionesPuntosVenta.AddRange(CreateSalesPoints(id, salesPointIds));
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
            var updated = ToDto(await GetByIdAsync(id, cancellationToken));
            await transaction.CommitAsync(cancellationToken);
            return Ok(updated);
        });
    }

    private async Task ValidateScopeAsync(UpsertPromotionDto dto, CancellationToken cancellationToken)
    {
        if (dto.ProductId.HasValue && !await db.Productos.AnyAsync(x => x.Id == dto.ProductId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }

        var salesPointIds = SalesPointIds(dto);
        if (salesPointIds.Count > 0
            && await db.PuntosVenta.CountAsync(x => salesPointIds.Contains(x.Id), cancellationToken) != salesPointIds.Count)
        {
            throw new KeyNotFoundException("Una o mas sedes no fueron encontradas.");
        }
    }

    private static void Apply(Promocion entity, UpsertPromotionDto dto, string code, IReadOnlyCollection<Guid> salesPointIds)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.TipoDescuento = NormalizeDiscountType(dto.DiscountType);
        entity.ValorDescuento = dto.DiscountValue;
        entity.ProductoId = dto.ProductId;
        entity.Marca = Normalize(dto.Brand);
        entity.Color = Normalize(dto.Color);
        entity.PuntoVentaId = salesPointIds.Count == 1 ? salesPointIds.Single() : null;
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

    private async Task<Promocion> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Promociones
            .AsNoTracking()
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .Include(x => x.Sedes)
                .ThenInclude(x => x.PuntoVenta)
            .SingleAsync(x => x.Id == id, cancellationToken);

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
        PromotionSalesPointIds(x),
        PromotionSalesPointNames(x),
        x.VigenteDesde,
        x.VigenteHasta,
        x.Activa);

    private static IReadOnlyCollection<Guid> SalesPointIds(UpsertPromotionDto dto) =>
        (dto.SalesPointIds ?? [])
            .Concat(dto.SalesPointId.HasValue ? [dto.SalesPointId.Value] : [])
            .Distinct()
            .ToList();

    private static void AddSalesPoints(Promocion promotion, IEnumerable<Guid> salesPointIds)
    {
        foreach (var salesPoint in CreateSalesPoints(promotion.Id, salesPointIds))
        {
            promotion.Sedes.Add(salesPoint);
        }
    }

    private static IEnumerable<PromocionPuntoVenta> CreateSalesPoints(Guid promotionId, IEnumerable<Guid> salesPointIds) =>
        salesPointIds.Select(salesPointId => new PromocionPuntoVenta
        {
            PromocionId = promotionId,
            PuntoVentaId = salesPointId
        });

    private static IReadOnlyCollection<Guid> PromotionSalesPointIds(Promocion promotion) =>
        promotion.Sedes.Count > 0
            ? promotion.Sedes.Select(x => x.PuntoVentaId).Distinct().ToList()
            : promotion.PuntoVentaId.HasValue ? [promotion.PuntoVentaId.Value] : [];

    private static IReadOnlyCollection<string> PromotionSalesPointNames(Promocion promotion) =>
        promotion.Sedes.Count > 0
            ? promotion.Sedes.Where(x => x.PuntoVenta is not null).Select(x => x.PuntoVenta!.Nombre).Distinct().Order().ToList()
            : promotion.PuntoVenta is not null ? [promotion.PuntoVenta.Nombre] : [];

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
