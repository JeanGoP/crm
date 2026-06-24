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
    private const int MaxDeliveryPhotoDataUrlLength = 1_500_000;
    private static readonly string[] AllowedDeliveryPhotoPrefixes = ["data:image/png;base64,", "data:image/jpeg;base64,", "data:image/webp;base64,"];

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
            ChecklistPreEntregaCompletado = dto.PreDeliveryChecklistCompleted,
            ProtocoloEntrega = Normalize(dto.DeliveryProtocol) ?? BuildDefaultProtocol(application.Producto),
            FotoEntregaDataUrl = NormalizeDeliveryPhoto(dto.DeliveryPhotoDataUrl),
            FotoEntregaNombre = Normalize(dto.DeliveryPhotoFileName),
            PrimeraRevisionProgramadaEn = ResolveFirstServiceDate(dto.FirstServiceScheduledAt, dto.DeliveryDate, dto.Status),
            Estado = dto.Status,
            Observaciones = Normalize(dto.Notes)
        };

        ApplyDeliveryStatus(application, entity.Estado);
        ScheduleFirstService(entity, application);
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
        entity.ChecklistPreEntregaCompletado = dto.PreDeliveryChecklistCompleted;
        entity.ProtocoloEntrega = Normalize(dto.DeliveryProtocol) ?? BuildDefaultProtocol(entity.Producto);
        entity.FotoEntregaDataUrl = NormalizeDeliveryPhoto(dto.DeliveryPhotoDataUrl);
        entity.FotoEntregaNombre = Normalize(dto.DeliveryPhotoFileName);
        entity.PrimeraRevisionProgramadaEn = ResolveFirstServiceDate(dto.FirstServiceScheduledAt, dto.DeliveryDate, dto.Status);
        entity.Estado = dto.Status;
        entity.Observaciones = Normalize(dto.Notes);

        ApplyDeliveryStatus(application, entity.Estado);
        ScheduleFirstService(entity, application);
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
            if (!dto.PreDeliveryChecklistCompleted) throw new ValidationException("Para entregar debe completarse el checklist obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.DeliveryPhotoDataUrl)) throw new ValidationException("Para entregar debe adjuntarse una foto de entrega.");
        }

        NormalizeDeliveryPhoto(dto.DeliveryPhotoDataUrl);
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

    private void ScheduleFirstService(EntregaMoto delivery, SolicitudCredito application)
    {
        if (delivery.Estado != EstadoEntregaMoto.Entregada || delivery.PrimeraRevisionProgramadaEn is null)
        {
            return;
        }

        var productName = application.Producto is null ? "producto entregado" : ProductName(application.Producto);
        var activityId = delivery.ActividadPrimeraRevisionId ?? Guid.NewGuid();
        var title = "Primera revision postventa";
        var description = $"Agendar y confirmar primera revision del cliente por entrega {delivery.Numero} - {productName}.";

        var existing = db.Actividades.Local.FirstOrDefault(x => x.Id == activityId);
        if (existing is null)
        {
            existing = db.Actividades.FirstOrDefault(x => x.Id == activityId);
        }

        if (existing is null)
        {
            existing = new Actividad
            {
                Id = activityId,
                Titulo = title,
                Tipo = TipoActividad.Llamada,
                Estado = EstadoActividad.Pendiente,
                ClienteId = application.ClienteId
            };
            db.Actividades.Add(existing);
        }

        existing.Titulo = title;
        existing.Descripcion = description;
        existing.Tipo = TipoActividad.Llamada;
        existing.Estado = EstadoActividad.Pendiente;
        existing.FechaProgramada = delivery.PrimeraRevisionProgramadaEn.Value;
        existing.RecordatorioEn = delivery.PrimeraRevisionProgramadaEn.Value.AddHours(-24);
        existing.ClienteId = application.ClienteId;
        delivery.ActividadPrimeraRevisionId = activityId;
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
            x.ChecklistPreEntregaCompletado,
            x.ProtocoloEntrega,
            x.FotoEntregaDataUrl,
            x.FotoEntregaNombre,
            x.PrimeraRevisionProgramadaEn,
            x.ActividadPrimeraRevisionId,
            x.Estado,
            x.Observaciones);
    }

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static DateTime? ResolveFirstServiceDate(DateTime? requestedDate, DateTime deliveryDate, EstadoEntregaMoto status)
    {
        if (status != EstadoEntregaMoto.Entregada)
        {
            return requestedDate;
        }

        return requestedDate ?? deliveryDate.AddDays(30);
    }

    private static string BuildDefaultProtocol(Producto? product)
    {
        var brand = string.IsNullOrWhiteSpace(product?.Marca) ? "la marca" : product.Marca.Trim();
        return $"Protocolo digital {brand}: validar identidad del cliente, seriales del vehiculo, estado fisico, accesorios, documentos, explicacion de garantia y firma del acta.";
    }

    private static string? NormalizeDeliveryPhoto(string? dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl))
        {
            return null;
        }

        var photo = dataUrl.Trim();
        if (photo.Length > MaxDeliveryPhotoDataUrlLength)
        {
            throw new ValidationException("La foto de entrega es demasiado grande. Usa una imagen menor a 1 MB.");
        }

        if (!AllowedDeliveryPhotoPrefixes.Any(prefix => photo.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("La foto de entrega debe ser PNG, JPG o WebP.");
        }

        return photo;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
