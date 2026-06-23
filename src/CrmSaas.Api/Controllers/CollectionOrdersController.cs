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
[Route("api/collection-orders")]
public sealed class CollectionOrdersController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CollectionOrderDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.OrdenesRecaudo
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Detalles)
            .OrderByDescending(x => x.FechaEmision)
            .ToListAsync(cancellationToken);

        rows.ForEach(RefreshOverdueStatus);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CollectionOrderDto>> Create(UpsertCollectionOrderDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var application = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.Id == dto.CreditApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        EnsureCanCreateOrder(application);

        var entity = new OrdenRecaudo
        {
            Numero = $"REC-{ColombiaTime.Now:yyyyMMddHHmmss}",
            SolicitudCreditoId = application.Id,
            ClienteId = application.ClienteId,
            FechaEmision = ColombiaTime.Now,
            FechaVencimiento = dto.DueDate.Date,
            ValorPagado = dto.PaidAmount,
            Observaciones = Normalize(dto.Notes)
        };

        SetDetails(entity, dto);
        ApplyStatus(entity, dto.Status);
        db.OrdenesRecaudo.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        entity.SolicitudCredito = application;
        entity.Cliente = application.Cliente;
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionOrderDto>> Update(Guid id, UpsertCollectionOrderDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.OrdenesRecaudo
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Orden de recaudo no encontrada.");

        if (entity.SolicitudCreditoId != dto.CreditApplicationId)
        {
            throw new ValidationException("No se puede cambiar la solicitud asociada a una orden existente.");
        }

        entity.FechaVencimiento = dto.DueDate.Date;
        entity.ValorPagado = dto.PaidAmount;
        entity.Observaciones = Normalize(dto.Notes);
        SetDetails(entity, dto);
        ApplyStatus(entity, dto.Status);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    private static void Validate(UpsertCollectionOrderDto dto)
    {
        if (dto.CreditApplicationId == Guid.Empty) throw new ValidationException("Debe seleccionar una solicitud.");
        if (dto.VehicleAmount < 0) throw new ValidationException("El valor de vehiculo no puede ser negativo.");
        if (dto.DocumentsAmount < 0) throw new ValidationException("El valor de documentos no puede ser negativo.");
        if (dto.AdvanceAmount < 0) throw new ValidationException("El anticipo no puede ser negativo.");
        if (dto.PaidAmount < 0) throw new ValidationException("El valor pagado no puede ser negativo.");
        if (dto.VehicleAmount + dto.DocumentsAmount + dto.AdvanceAmount <= 0) throw new ValidationException("La orden debe tener al menos un concepto con valor.");
    }

    private static void EnsureCanCreateOrder(SolicitudCredito application)
    {
        if (application.Estado is EstadoSolicitudCredito.Rechazada or EstadoSolicitudCredito.Desistida)
        {
            throw new ValidationException("No se pueden emitir ordenes de recaudo para solicitudes rechazadas o desistidas.");
        }
    }

    private static void SetDetails(OrdenRecaudo entity, UpsertCollectionOrderDto dto)
    {
        entity.Detalles.Clear();
        AddDetail(entity, TipoConceptoRecaudo.Vehiculo, "Vehiculo", dto.VehicleAmount);
        AddDetail(entity, TipoConceptoRecaudo.Documentos, "Documentos", dto.DocumentsAmount);
        AddDetail(entity, TipoConceptoRecaudo.Anticipo, "Anticipo", dto.AdvanceAmount);
        entity.Total = entity.Detalles.Sum(x => x.Valor);
    }

    private static void AddDetail(OrdenRecaudo entity, TipoConceptoRecaudo type, string concept, decimal amount)
    {
        if (amount <= 0) return;
        entity.Detalles.Add(new DetalleOrdenRecaudo
        {
            Tipo = type,
            Concepto = concept,
            Valor = amount
        });
    }

    private static void ApplyStatus(OrdenRecaudo entity, EstadoOrdenRecaudo requestedStatus)
    {
        if (requestedStatus == EstadoOrdenRecaudo.Anulada)
        {
            entity.Estado = EstadoOrdenRecaudo.Anulada;
            entity.FechaPago = null;
            return;
        }

        if (requestedStatus == EstadoOrdenRecaudo.Vencida && entity.ValorPagado <= 0)
        {
            entity.Estado = EstadoOrdenRecaudo.Vencida;
            entity.FechaPago = null;
            return;
        }

        if (entity.ValorPagado >= entity.Total)
        {
            entity.Estado = EstadoOrdenRecaudo.Pagada;
            entity.FechaPago ??= ColombiaTime.Now;
            return;
        }

        if (entity.ValorPagado > 0)
        {
            entity.Estado = EstadoOrdenRecaudo.Parcial;
            entity.FechaPago = null;
            return;
        }

        entity.Estado = entity.FechaVencimiento.Date < ColombiaTime.Now.Date ? EstadoOrdenRecaudo.Vencida : EstadoOrdenRecaudo.Emitida;
        entity.FechaPago = null;
    }

    private static void RefreshOverdueStatus(OrdenRecaudo entity)
    {
        if (entity.Estado == EstadoOrdenRecaudo.Emitida && entity.FechaVencimiento.Date < ColombiaTime.Now.Date)
        {
            entity.Estado = EstadoOrdenRecaudo.Vencida;
        }
    }

    private static CollectionOrderDto ToDto(OrdenRecaudo x)
    {
        var details = x.Detalles.OrderBy(d => d.Tipo).Select(d => new CollectionOrderDetailDto(d.Id, d.Tipo, d.Concepto, d.Valor)).ToList();
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var vehicleAmount = details.Where(d => d.Type == TipoConceptoRecaudo.Vehiculo).Sum(d => d.Amount);
        var documentsAmount = details.Where(d => d.Type == TipoConceptoRecaudo.Documentos).Sum(d => d.Amount);
        var advanceAmount = details.Where(d => d.Type == TipoConceptoRecaudo.Anticipo).Sum(d => d.Amount);
        return new CollectionOrderDto(
            x.Id,
            x.Numero,
            x.SolicitudCreditoId,
            x.SolicitudCredito?.Numero ?? string.Empty,
            x.ClienteId,
            customerName,
            x.FechaEmision,
            x.FechaVencimiento,
            vehicleAmount,
            documentsAmount,
            advanceAmount,
            x.Total,
            x.ValorPagado,
            Math.Max(0, x.Total - x.ValorPagado),
            x.FechaPago,
            x.Estado,
            x.Observaciones,
            details);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
