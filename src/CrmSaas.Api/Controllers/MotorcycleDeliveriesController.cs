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
[Route("api/motorcycle-deliveries")]
public sealed class MotorcycleDeliveriesController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<MotorcycleDeliveryDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.EntregasMoto
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .OrderByDescending(x => x.FechaEntrega)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<MotorcycleDeliveryDto>> Create(UpsertMotorcycleDeliveryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        if (await db.EntregasMoto.AnyAsync(x => x.SolicitudCreditoId == dto.CreditApplicationId, cancellationToken))
        {
            throw new ValidationException("Esta solicitud ya tiene una entrega registrada.");
        }

        var application = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == dto.CreditApplicationId, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        EnsureCanDeliver(application);

        var entity = new EntregaMoto
        {
            Numero = $"ENT-{ColombiaTime.Now:yyyyMMddHHmmss}",
            SolicitudCreditoId = application.Id,
            ClienteId = application.ClienteId,
            ProductoId = application.ProductoId,
            FechaEntrega = dto.DeliveryDate,
            AsesorResponsable = Normalize(dto.ResponsibleAdvisor),
            Vin = Normalize(dto.Vin),
            NumeroChasis = Normalize(dto.ChassisNumber),
            NumeroMotor = Normalize(dto.EngineNumber),
            Placa = Normalize(dto.Plate)?.ToUpperInvariant(),
            KilometrajeEntrega = dto.DeliveryMileage,
            CascoEntregado = dto.HelmetDelivered,
            SoatEntregado = dto.SoatDelivered,
            MatriculaEntregada = dto.RegistrationDelivered,
            ManualGarantiaEntregado = dto.WarrantyManualDelivered,
            ActaEntregaFirmada = dto.DeliveryCertificateSigned,
            Estado = dto.Status,
            Observaciones = Normalize(dto.Notes)
        };

        ApplyDeliveryStatus(application, entity.Estado);
        db.EntregasMoto.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        entity.SolicitudCredito = application;
        entity.Cliente = application.Cliente;
        entity.Producto = application.Producto;
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MotorcycleDeliveryDto>> Update(Guid id, UpsertMotorcycleDeliveryDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.EntregasMoto
            .Include(x => x.SolicitudCredito)
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Entrega no encontrada.");

        if (entity.SolicitudCreditoId != dto.CreditApplicationId)
        {
            throw new ValidationException("No se puede cambiar la solicitud asociada a una entrega existente.");
        }

        var application = entity.SolicitudCredito ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        EnsureCanDeliver(application);

        entity.FechaEntrega = dto.DeliveryDate;
        entity.AsesorResponsable = Normalize(dto.ResponsibleAdvisor);
        entity.Vin = Normalize(dto.Vin);
        entity.NumeroChasis = Normalize(dto.ChassisNumber);
        entity.NumeroMotor = Normalize(dto.EngineNumber);
        entity.Placa = Normalize(dto.Plate)?.ToUpperInvariant();
        entity.KilometrajeEntrega = dto.DeliveryMileage;
        entity.CascoEntregado = dto.HelmetDelivered;
        entity.SoatEntregado = dto.SoatDelivered;
        entity.MatriculaEntregada = dto.RegistrationDelivered;
        entity.ManualGarantiaEntregado = dto.WarrantyManualDelivered;
        entity.ActaEntregaFirmada = dto.DeliveryCertificateSigned;
        entity.Estado = dto.Status;
        entity.Observaciones = Normalize(dto.Notes);

        ApplyDeliveryStatus(application, entity.Estado);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private static void Validate(UpsertMotorcycleDeliveryDto dto)
    {
        if (dto.CreditApplicationId == Guid.Empty) throw new ValidationException("Debe seleccionar una solicitud aprobada.");
        if (dto.DeliveryMileage.HasValue && dto.DeliveryMileage < 0) throw new ValidationException("El kilometraje no puede ser negativo.");
        if (dto.Status == EstadoEntregaMoto.Entregada)
        {
            if (string.IsNullOrWhiteSpace(dto.ChassisNumber)) throw new ValidationException("El numero de chasis es obligatorio para entregar.");
            if (string.IsNullOrWhiteSpace(dto.EngineNumber)) throw new ValidationException("El numero de motor es obligatorio para entregar.");
            if (!dto.DeliveryCertificateSigned) throw new ValidationException("Para entregar debe estar firmada el acta de entrega.");
        }
    }

    private static void EnsureCanDeliver(SolicitudCredito application)
    {
        if (application.Estado is not (EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada))
        {
            throw new ValidationException("Solo se pueden registrar entregas para solicitudes aprobadas o desembolsadas.");
        }
    }

    private static void ApplyDeliveryStatus(SolicitudCredito application, EstadoEntregaMoto status)
    {
        if (status == EstadoEntregaMoto.Entregada)
        {
            application.Estado = EstadoSolicitudCredito.Desembolsada;
            application.FechaDesembolso ??= ColombiaTime.Now;
        }
    }

    private static MotorcycleDeliveryDto ToDto(EntregaMoto x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
        return new MotorcycleDeliveryDto(
            x.Id,
            x.Numero,
            x.SolicitudCreditoId,
            x.SolicitudCredito?.Numero ?? string.Empty,
            x.ClienteId,
            customerName,
            x.ProductoId,
            productName,
            x.FechaEntrega,
            x.AsesorResponsable,
            x.Vin,
            x.NumeroChasis,
            x.NumeroMotor,
            x.Placa,
            x.KilometrajeEntrega,
            x.CascoEntregado,
            x.SoatEntregado,
            x.MatriculaEntregada,
            x.ManualGarantiaEntregado,
            x.ActaEntregaFirmada,
            x.Estado,
            x.Observaciones);
    }

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
