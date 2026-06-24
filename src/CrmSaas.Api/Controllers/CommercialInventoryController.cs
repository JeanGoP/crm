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
[Route("api/commercial-inventory")]
public sealed class CommercialInventoryController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CommercialInventoryDto>>> Get(CancellationToken cancellationToken)
    {
        await RefreshExpiredReservations(cancellationToken);
        var rows = await Query()
            .OrderBy(x => x.PuntoVenta!.Nombre)
            .ThenBy(x => x.Producto!.Marca)
            .ThenBy(x => x.Producto!.Modelo)
            .ThenBy(x => x.NumeroChasis)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("summary")]
    public async Task<ActionResult<IReadOnlyCollection<CommercialInventorySummaryDto>>> Summary(CancellationToken cancellationToken)
    {
        await RefreshExpiredReservations(cancellationToken);
        var inventory = await db.InventarioComercial
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .ToListAsync(cancellationToken);

        var rows = inventory
            .GroupBy(x => new
            {
                x.ProductoId,
                ProductName = x.Producto is null ? "Producto" : ProductName(x.Producto),
                x.PuntoVentaId,
                SalesPointName = x.PuntoVenta?.Nombre ?? "Sede"
            })
            .Select(x => new CommercialInventorySummaryDto(
                x.Key.ProductoId,
                x.Key.ProductName,
                x.Key.PuntoVentaId,
                x.Key.SalesPointName,
                x.Count(i => i.Estado == EstadoInventarioComercial.Disponible && !i.EsUsada),
                x.Count(i => i.Estado == EstadoInventarioComercial.Separada),
                x.Count(i => i.Estado == EstadoInventarioComercial.Vendida),
                x.Count(i => i.EsUsada),
                x.Count(i => i.Estado == EstadoInventarioComercial.NoDisponible)))
            .OrderBy(x => x.SalesPointName)
            .ThenBy(x => x.ProductName)
            .ToList();

        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CommercialInventoryDto>> Create(UpsertCommercialInventoryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        await EnsureReferences(dto.ProductId, dto.SalesPointId, cancellationToken);
        await EnsureSerialsAreUnique(dto, null, cancellationToken);

        var entity = new InventarioComercial();
        Apply(entity, dto);
        db.InventarioComercial.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await LoadReferences(entity, cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CommercialInventoryDto>> Update(Guid id, UpsertCommercialInventoryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Inventario no encontrado.");
        await EnsureReferences(dto.ProductId, dto.SalesPointId, cancellationToken);
        await EnsureSerialsAreUnique(dto, id, cancellationToken);

        Apply(entity, dto);
        if (entity.Estado != EstadoInventarioComercial.Separada)
        {
            ClearReservation(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/reserve")]
    public async Task<ActionResult<CommercialInventoryDto>> Reserve(Guid id, ReserveCommercialInventoryDto dto, CancellationToken cancellationToken)
    {
        var entity = await Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Inventario no encontrado.");

        if (entity.Estado is not (EstadoInventarioComercial.Disponible or EstadoInventarioComercial.Usada))
        {
            throw new ValidationException("Solo se puede separar una unidad disponible o usada.");
        }

        var customerId = dto.CustomerId;
        if (dto.QuoteId.HasValue)
        {
            var quote = await db.Cotizaciones.Include(x => x.Cliente).FirstOrDefaultAsync(x => x.Id == dto.QuoteId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Cotizacion no encontrada.");
            customerId ??= quote.ClienteId;
            entity.CotizacionReservaId = quote.Id;
        }

        if (dto.CreditApplicationId.HasValue)
        {
            var application = await db.SolicitudesCredito.FirstOrDefaultAsync(x => x.Id == dto.CreditApplicationId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
            customerId ??= application.ClienteId;
            entity.SolicitudCreditoReservaId = application.Id;
        }

        if (customerId.HasValue && !await db.Clientes.AnyAsync(x => x.Id == customerId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Cliente no encontrado.");
        }

        entity.Estado = EstadoInventarioComercial.Separada;
        entity.ClienteReservaId = customerId;
        entity.FechaReserva = ColombiaTime.Now;
        entity.FechaVencimientoReserva = dto.ReservationExpiresAt?.Date.AddDays(1).AddTicks(-1) ?? ColombiaTime.Now.AddDays(3);
        entity.Observaciones = Normalize(dto.Notes) ?? entity.Observaciones;
        await db.SaveChangesAsync(cancellationToken);

        await LoadReferences(entity, cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/release")]
    public async Task<ActionResult<CommercialInventoryDto>> Release(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Inventario no encontrado.");
        ClearReservation(entity);
        entity.Estado = EstadoInventarioComercial.Disponible;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/sell")]
    public async Task<ActionResult<CommercialInventoryDto>> Sell(Guid id, CancellationToken cancellationToken)
    {
        var entity = await Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Inventario no encontrado.");
        entity.Estado = EstadoInventarioComercial.Vendida;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private IQueryable<InventarioComercial> Query() =>
        db.InventarioComercial
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .Include(x => x.ClienteReserva)
            .Include(x => x.CotizacionReserva)
            .Include(x => x.SolicitudCreditoReserva);

    private async Task RefreshExpiredReservations(CancellationToken cancellationToken)
    {
        var now = ColombiaTime.Now;
        var expired = await db.InventarioComercial
            .Where(x => x.Estado == EstadoInventarioComercial.Separada && x.FechaVencimientoReserva < now)
            .ToListAsync(cancellationToken);

        if (expired.Count == 0) return;
        foreach (var item in expired)
        {
            ClearReservation(item);
            item.Estado = EstadoInventarioComercial.Disponible;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureReferences(Guid productId, Guid salesPointId, CancellationToken cancellationToken)
    {
        if (!await db.Productos.AnyAsync(x => x.Id == productId, cancellationToken))
        {
            throw new KeyNotFoundException("Producto no encontrado.");
        }

        if (!await db.PuntosVenta.AnyAsync(x => x.Id == salesPointId && x.Activa, cancellationToken))
        {
            throw new KeyNotFoundException("Sede no encontrada o inactiva.");
        }
    }

    private async Task EnsureSerialsAreUnique(UpsertCommercialInventoryDto dto, Guid? currentId, CancellationToken cancellationToken)
    {
        var chassis = Normalize(dto.ChassisNumber);
        var engine = Normalize(dto.EngineNumber);
        var plate = Normalize(dto.Plate)?.ToUpperInvariant();
        if (chassis is null && engine is null && plate is null) return;

        var exists = await db.InventarioComercial.AnyAsync(x =>
            (!currentId.HasValue || x.Id != currentId.Value) &&
            ((chassis != null && x.NumeroChasis == chassis) ||
             (engine != null && x.NumeroMotor == engine) ||
             (plate != null && x.Placa == plate)), cancellationToken);

        if (exists)
        {
            throw new ValidationException("Ya existe una unidad con ese chasis, motor o placa.");
        }
    }

    private static void Validate(UpsertCommercialInventoryDto dto)
    {
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar un producto.");
        if (dto.SalesPointId == Guid.Empty) throw new ValidationException("Debe seleccionar una sede.");
        if (dto.Mileage.HasValue && dto.Mileage < 0) throw new ValidationException("El kilometraje no puede ser negativo.");
        if (dto.IsUsed && !dto.Mileage.HasValue) throw new ValidationException("Para motos usadas debe registrar kilometraje.");
        if (dto.Status == EstadoInventarioComercial.Separada) throw new ValidationException("Use la accion Separar para reservar inventario.");
    }

    private static void Apply(InventarioComercial entity, UpsertCommercialInventoryDto dto)
    {
        entity.ProductoId = dto.ProductId;
        entity.PuntoVentaId = dto.SalesPointId;
        entity.Vin = Normalize(dto.Vin);
        entity.NumeroChasis = Normalize(dto.ChassisNumber);
        entity.NumeroMotor = Normalize(dto.EngineNumber);
        entity.Placa = Normalize(dto.Plate)?.ToUpperInvariant();
        entity.Color = Normalize(dto.Color);
        entity.EsUsada = dto.IsUsed;
        entity.Kilometraje = dto.Mileage;
        entity.Estado = dto.IsUsed && dto.Status == EstadoInventarioComercial.Disponible ? EstadoInventarioComercial.Usada : dto.Status;
        entity.Observaciones = Normalize(dto.Notes);
    }

    private static void ClearReservation(InventarioComercial entity)
    {
        entity.ClienteReservaId = null;
        entity.CotizacionReservaId = null;
        entity.SolicitudCreditoReservaId = null;
        entity.FechaReserva = null;
        entity.FechaVencimientoReserva = null;
    }

    private async Task LoadReferences(InventarioComercial entity, CancellationToken cancellationToken)
    {
        await db.Entry(entity).Reference(x => x.Producto).LoadAsync(cancellationToken);
        await db.Entry(entity).Reference(x => x.PuntoVenta).LoadAsync(cancellationToken);
        await db.Entry(entity).Reference(x => x.ClienteReserva).LoadAsync(cancellationToken);
        await db.Entry(entity).Reference(x => x.CotizacionReserva).LoadAsync(cancellationToken);
        await db.Entry(entity).Reference(x => x.SolicitudCreditoReserva).LoadAsync(cancellationToken);
    }

    private static CommercialInventoryDto ToDto(InventarioComercial x)
    {
        var customerName = x.ClienteReserva is null ? null : $"{x.ClienteReserva.Nombres} {x.ClienteReserva.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.ClienteReserva is not null) customerName = x.ClienteReserva.Nombre;
        return new CommercialInventoryDto(
            x.Id,
            x.ProductoId,
            x.Producto is null ? "Producto" : ProductName(x.Producto),
            x.PuntoVentaId,
            x.PuntoVenta?.Nombre ?? "Sede",
            x.Vin,
            x.NumeroChasis,
            x.NumeroMotor,
            x.Placa,
            x.Color,
            x.EsUsada,
            x.Kilometraje,
            x.Estado,
            x.ClienteReservaId,
            customerName,
            x.CotizacionReservaId,
            x.CotizacionReserva?.Numero,
            x.SolicitudCreditoReservaId,
            x.SolicitudCreditoReserva?.Numero,
            x.FechaReserva,
            x.FechaVencimientoReserva,
            x.Estado == EstadoInventarioComercial.Separada && x.FechaVencimientoReserva < ColombiaTime.Now,
            x.Observaciones);
    }

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
