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
[Route("api/procedures")]
public sealed class ProceduresController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProcedureDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.Tramites
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .OrderBy(x => x.Estado == EstadoTramite.Completado || x.Estado == EstadoTramite.Cancelado)
            .ThenBy(x => x.FechaEstimada)
            .ToListAsync(cancellationToken);

        rows.ForEach(RefreshOverdueStatus);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ProcedureDto>> Create(UpsertProcedureDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var application = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Cotizacion)
            .FirstOrDefaultAsync(x => x.Id == dto.CreditApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        var salesPoint = await ResolveSalesPointAsync(dto.SalesPointId ?? application.Cotizacion?.PuntoVentaId, cancellationToken);
        var entity = new Tramite
        {
            Numero = $"TRA-{ColombiaTime.Now:yyyyMMddHHmmss}",
            SolicitudCreditoId = application.Id,
            ClienteId = application.ClienteId,
            ProductoId = application.ProductoId,
            PuntoVentaId = salesPoint?.Id,
            Tipo = dto.Type,
            FechaInicio = dto.StartDate,
            FechaEstimada = dto.EstimatedDate?.Date ?? EstimateDate(dto.Type, dto.StartDate, salesPoint),
            Responsable = Normalize(dto.Responsible),
            Tercero = Normalize(dto.ThirdParty),
            NotificarCliente = dto.NotifyCustomer,
            FechaNotificacionCliente = dto.CustomerNotifiedAt,
            Observaciones = Normalize(dto.Notes)
        };
        ApplyStatus(entity, dto.Status, dto.CompletedAt);

        db.Tramites.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        entity.SolicitudCredito = application;
        entity.Cliente = application.Cliente;
        entity.Producto = application.Producto;
        entity.PuntoVenta = salesPoint;
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProcedureDto>> Update(Guid id, UpsertProcedureDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.Tramites
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Tramite no encontrado.");

        if (entity.SolicitudCreditoId != dto.CreditApplicationId)
        {
            throw new ValidationException("No se puede cambiar la solicitud asociada a un tramite existente.");
        }

        var salesPoint = await ResolveSalesPointAsync(dto.SalesPointId ?? entity.PuntoVentaId, cancellationToken);
        entity.PuntoVentaId = salesPoint?.Id;
        entity.PuntoVenta = salesPoint;
        entity.Tipo = dto.Type;
        entity.FechaInicio = dto.StartDate;
        entity.FechaEstimada = dto.EstimatedDate?.Date ?? EstimateDate(dto.Type, dto.StartDate, salesPoint);
        entity.Responsable = Normalize(dto.Responsible);
        entity.Tercero = Normalize(dto.ThirdParty);
        entity.NotificarCliente = dto.NotifyCustomer;
        entity.FechaNotificacionCliente = dto.CustomerNotifiedAt;
        entity.Observaciones = Normalize(dto.Notes);
        ApplyStatus(entity, dto.Status, dto.CompletedAt);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private async Task<PuntoVenta?> ResolveSalesPointAsync(Guid? salesPointId, CancellationToken cancellationToken)
    {
        if (salesPointId.HasValue)
        {
            return await db.PuntosVenta.FirstOrDefaultAsync(x => x.Id == salesPointId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Sede no encontrada.");
        }

        return await db.PuntosVenta.OrderByDescending(x => x.Activa).ThenBy(x => x.Nombre).FirstOrDefaultAsync(cancellationToken);
    }

    private static void Validate(UpsertProcedureDto dto)
    {
        if (dto.CreditApplicationId == Guid.Empty) throw new ValidationException("Debe seleccionar una solicitud.");
        if (dto.StartDate == default) throw new ValidationException("Debe indicar la fecha de inicio.");
        if (dto.Status == EstadoTramite.Completado && dto.CompletedAt is null) throw new ValidationException("Debe indicar la fecha de finalizacion.");
    }

    private static DateTime EstimateDate(TipoTramite type, DateTime startDate, PuntoVenta? salesPoint)
    {
        var days = type switch
        {
            TipoTramite.Soat => salesPoint?.TiempoSoatDias ?? 2,
            TipoTramite.Matricula => salesPoint?.TiempoMatriculaDias ?? 15,
            TipoTramite.Placas => Math.Max(1, salesPoint?.TiempoMatriculaDias ?? 15),
            TipoTramite.Terceros => 5,
            _ => 5
        };
        return startDate.Date.AddDays(days);
    }

    private static void ApplyStatus(Tramite entity, EstadoTramite status, DateTime? completedAt)
    {
        entity.Estado = status;
        entity.FechaFinalizacion = status == EstadoTramite.Completado ? completedAt ?? ColombiaTime.Now : null;
        RefreshOverdueStatus(entity);
    }

    private static void RefreshOverdueStatus(Tramite entity)
    {
        if (entity.Estado is EstadoTramite.Completado or EstadoTramite.Cancelado) return;
        if (entity.FechaEstimada.Date < ColombiaTime.Now.Date)
        {
            entity.Estado = EstadoTramite.Atrasado;
        }
    }

    private static ProcedureDto ToDto(Tramite x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
        var message = $"Hola {customerName}, te informamos que tu tramite de {ProcedureType(x.Tipo)} para {productName} se encuentra en estado {ProcedureStatus(x.Estado)}. Fecha estimada: {x.FechaEstimada:yyyy-MM-dd}.";
        return new ProcedureDto(
            x.Id,
            x.Numero,
            x.SolicitudCreditoId,
            x.SolicitudCredito?.Numero ?? string.Empty,
            x.ClienteId,
            customerName,
            x.Cliente?.Telefono ?? x.SolicitudCredito?.Celular ?? string.Empty,
            x.ProductoId,
            productName,
            x.PuntoVentaId,
            x.PuntoVenta?.Nombre,
            x.Tipo,
            x.Estado,
            x.FechaInicio,
            x.FechaEstimada,
            x.FechaFinalizacion,
            x.Responsable,
            x.Tercero,
            x.NotificarCliente,
            x.FechaNotificacionCliente,
            x.Estado == EstadoTramite.Atrasado,
            message,
            x.Observaciones);
    }

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static string ProcedureType(TipoTramite type) => type switch
    {
        TipoTramite.Soat => "SOAT",
        TipoTramite.Matricula => "matricula",
        TipoTramite.Placas => "placas",
        TipoTramite.Terceros => "tramites de terceros",
        _ => "tramite"
    };

    private static string ProcedureStatus(EstadoTramite status) => status switch
    {
        EstadoTramite.Pendiente => "pendiente",
        EstadoTramite.EnProceso => "en proceso",
        EstadoTramite.Completado => "completado",
        EstadoTramite.Atrasado => "atrasado",
        EstadoTramite.Cancelado => "cancelado",
        _ => "pendiente"
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
