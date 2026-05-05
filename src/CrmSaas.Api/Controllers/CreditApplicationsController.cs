using CrmSaas.Application.DTOs;
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
[Route("api/credit-applications")]
public sealed class CreditApplicationsController(CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CreditApplicationDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CreditApplicationDto>> Create(UpsertCreditApplicationDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var customer = await db.Clientes.FirstOrDefaultAsync(x => x.Id == dto.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");
        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Moto no encontrada.");
        if (dto.QuoteId.HasValue && !await db.Cotizaciones.AnyAsync(x => x.Id == dto.QuoteId.Value, cancellationToken)) throw new KeyNotFoundException("Cotizacion no encontrada.");
        if (dto.DealId.HasValue && !await db.Negocios.AnyAsync(x => x.Id == dto.DealId.Value, cancellationToken)) throw new KeyNotFoundException("Negocio no encontrado.");

        var entity = new SolicitudCredito
        {
            Numero = $"SOL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ClienteId = customer.Id,
            ProductoId = product.Id,
            CotizacionId = dto.QuoteId,
            NegocioId = dto.DealId,
            TipoIdentificacion = dto.IdentificationType,
            NumeroIdentificacion = dto.IdentificationNumber.Trim(),
            FechaNacimiento = dto.BirthDate,
            Celular = dto.Mobile.Trim(),
            Direccion = dto.Address,
            Ciudad = dto.City,
            Ocupacion = dto.Occupation,
            IngresosMensuales = dto.MonthlyIncome,
            CuotaInicial = dto.DownPayment,
            PlazoMeses = dto.TermMonths,
            ValorMoto = dto.MotorcycleValue > 0 ? dto.MotorcycleValue : product.Precio,
            Estado = dto.Status,
            Observaciones = dto.Notes
        };

        foreach (var document in DefaultDocuments())
        {
            entity.Documentos.Add(document);
        }

        db.SolicitudesCredito.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        entity.Cliente = customer;
        entity.Producto = product;
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CreditApplicationDto>> Update(Guid id, UpsertCreditApplicationDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        if (!await db.Clientes.AnyAsync(x => x.Id == dto.CustomerId, cancellationToken)) throw new KeyNotFoundException("Cliente no encontrado.");
        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Moto no encontrada.");

        entity.ClienteId = dto.CustomerId;
        entity.ProductoId = dto.ProductId;
        entity.CotizacionId = dto.QuoteId;
        entity.NegocioId = dto.DealId;
        entity.TipoIdentificacion = dto.IdentificationType;
        entity.NumeroIdentificacion = dto.IdentificationNumber.Trim();
        entity.FechaNacimiento = dto.BirthDate;
        entity.Celular = dto.Mobile.Trim();
        entity.Direccion = dto.Address;
        entity.Ciudad = dto.City;
        entity.Ocupacion = dto.Occupation;
        entity.IngresosMensuales = dto.MonthlyIncome;
        entity.CuotaInicial = dto.DownPayment;
        entity.PlazoMeses = dto.TermMonths;
        entity.ValorMoto = dto.MotorcycleValue > 0 ? dto.MotorcycleValue : product.Precio;
        entity.Estado = dto.Status;
        entity.Observaciones = dto.Notes;

        await db.SaveChangesAsync(cancellationToken);
        entity.Producto = product;
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<CreditApplicationDto>> ChangeStatus(Guid id, ChangeCreditApplicationStatusDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        entity.Estado = dto.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<CreditApplicationDto>> UpdateDocument(Guid id, Guid documentId, UpsertCreditDocumentDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        var document = entity.Documentos.FirstOrDefault(x => x.Id == documentId)
            ?? throw new KeyNotFoundException("Documento no encontrado.");

        document.Tipo = dto.Type;
        document.Nombre = string.IsNullOrWhiteSpace(dto.Name) ? document.Nombre : dto.Name.Trim();
        document.Estado = dto.Status;
        document.FechaRecepcion = dto.Status is EstadoDocumentoCredito.Recibido or EstadoDocumentoCredito.Validado
            ? dto.ReceivedAt ?? DateTime.UtcNow
            : dto.ReceivedAt;
        document.Observaciones = dto.Notes;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    private static void Validate(UpsertCreditApplicationDto dto)
    {
        if (dto.CustomerId == Guid.Empty) throw new ValidationException("Debe seleccionar un cliente.");
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar una moto.");
        if (string.IsNullOrWhiteSpace(dto.IdentificationNumber)) throw new ValidationException("El numero de identificacion es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Mobile)) throw new ValidationException("El celular o WhatsApp es obligatorio.");
        if (dto.MonthlyIncome < 0) throw new ValidationException("Los ingresos no pueden ser negativos.");
        if (dto.DownPayment < 0) throw new ValidationException("La cuota inicial no puede ser negativa.");
        if (dto.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");
    }

    private static IReadOnlyCollection<DocumentoSolicitudCredito> DefaultDocuments() =>
    [
        new() { Tipo = TipoDocumentoCredito.Cedula, Nombre = "Cedula" },
        new() { Tipo = TipoDocumentoCredito.SoporteIngresos, Nombre = "Soporte de ingresos" },
        new() { Tipo = TipoDocumentoCredito.ReciboServicio, Nombre = "Recibo de servicio o direccion" },
        new() { Tipo = TipoDocumentoCredito.Referencias, Nombre = "Referencias" }
    ];

    private static CreditApplicationDto ToDto(SolicitudCredito x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Moto" : $"{x.Producto.Marca} {x.Producto.Modelo} {x.Producto.Referencia}".Trim();
        return new CreditApplicationDto(
            x.Id,
            x.Numero,
            x.ClienteId,
            customerName,
            x.ProductoId,
            productName,
            x.CotizacionId,
            x.NegocioId,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.FechaNacimiento,
            x.Celular,
            x.Direccion,
            x.Ciudad,
            x.Ocupacion,
            x.IngresosMensuales,
            x.CuotaInicial,
            x.PlazoMeses,
            x.ValorMoto,
            x.Estado,
            x.Observaciones,
            x.Documentos.OrderBy(d => d.Tipo).Select(d => new CreditDocumentDto(d.Id, d.Tipo, d.Nombre, d.Estado, d.FechaRecepcion, d.Observaciones)).ToList());
    }
}
