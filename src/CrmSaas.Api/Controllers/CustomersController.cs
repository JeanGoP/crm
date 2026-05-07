using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
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
[Route("api/customers")]
public sealed class CustomersController(ICustomerService service, IValidator<UpsertCustomerDto> validator, CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CustomerDto>>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<Customer360Dto>> Summary(Guid id, CancellationToken cancellationToken)
    {
        var customer = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        var quotes = await db.Cotizaciones
            .Include(x => x.Producto)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCotizacion)
            .ToListAsync(cancellationToken);
        var creditApplications = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var dealTimelineItems = await db.Negocios
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => new DealTimelineEntry(
                new DealDto(x.Id, x.Titulo, x.ClienteId, x.EtapaNegocioId, x.Valor, x.ProbabilidadCierre, x.FechaEstimadaCierre, x.Estado),
                x.FechaActualizacion ?? x.FechaCreacion))
            .ToListAsync(cancellationToken);
        var deals = dealTimelineItems.Select(x => x.Deal).ToList();
        var activities = await db.Actividades
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaProgramada)
            .Select(x => new ActivityDto(x.Id, x.Titulo, x.Descripcion, x.Tipo, x.Estado, x.FechaProgramada, x.RecordatorioEn, x.ClienteId, x.NegocioId, x.UsuarioAsignadoId, null, null))
            .ToListAsync(cancellationToken);
        var notes = await db.Notas
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var timeline = BuildTimeline(quotes, creditApplications, dealTimelineItems, activities, notes);

        return Ok(new Customer360Dto(
            ToCustomerDto(customer),
            quotes.Select(ToQuoteDto).ToList(),
            creditApplications.Select(ToCreditApplicationDto).ToList(),
            deals,
            activities,
            timeline));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        var created = await service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static CustomerDto ToCustomerDto(Cliente x)
    {
        var displayName = $"{x.Nombres} {x.Apellidos}".Trim();
        return new CustomerDto(x.Id, string.IsNullOrWhiteSpace(displayName) ? x.Nombre : displayName, x.Nombres, x.Apellidos, x.EmpresaCliente, x.Email, x.Telefono, x.Estado, x.Etiquetas);
    }

    private static QuoteDto ToQuoteDto(Cotizacion x)
    {
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
        var termMonths = x.PlazoMeses <= 0 ? 24 : x.PlazoMeses;
        var financedAmount = x.ValorFinanciado <= 0 && x.CuotaMensualEstimada <= 0 ? Math.Max(x.PrecioProducto - x.CuotaInicial, 0) : x.ValorFinanciado;
        var totalPayment = x.TotalPagarEstimado <= 0 ? x.PrecioProducto : x.TotalPagarEstimado;
        return new QuoteDto(
            x.Id,
            x.Numero,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.NombresCliente,
            x.ApellidosCliente,
            x.ClienteId,
            x.ProductoId,
            productName,
            x.PrecioProducto,
            x.CuotaInicial,
            termMonths,
            x.TasaInteresMensual,
            financedAmount,
            x.CuotaMensualEstimada,
            totalPayment,
            x.FechaCotizacion,
            x.ValidaHasta,
            x.Observaciones);
    }

    private static CreditApplicationDto ToCreditApplicationDto(SolicitudCredito x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
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
            x.CodeudorNombre,
            x.CodeudorIdentificacion,
            x.CodeudorCelular,
            x.CodeudorParentesco,
            x.CodeudorIngresosMensuales,
            x.Referencia1Nombre,
            x.Referencia1Celular,
            x.Referencia1Relacion,
            x.Referencia2Nombre,
            x.Referencia2Celular,
            x.Referencia2Relacion,
            x.Estado,
            x.Observaciones,
            x.FechaEnvio,
            x.FechaInicioEstudio,
            x.FechaAprobacion,
            x.FechaRechazo,
            x.FechaDesembolso,
            x.UsuarioDecision,
            x.ObservacionDecision,
            x.Documentos.OrderBy(d => d.Tipo).Select(d => new CreditDocumentDto(
                d.Id,
                d.Tipo,
                d.Nombre,
                d.Estado,
                d.FechaRecepcion,
                d.Observaciones,
                !string.IsNullOrWhiteSpace(d.RutaArchivo),
                d.NombreArchivo,
                d.ContentType,
                d.TamanoBytes,
                d.FechaCarga)).ToList());
    }

    private static IReadOnlyCollection<CustomerTimelineItemDto> BuildTimeline(
        IReadOnlyCollection<Cotizacion> quotes,
        IReadOnlyCollection<SolicitudCredito> creditApplications,
        IReadOnlyCollection<DealTimelineEntry> deals,
        IReadOnlyCollection<ActivityDto> activities,
        IReadOnlyCollection<Nota> notes)
    {
        var items = new List<CustomerTimelineItemDto>();

        items.AddRange(quotes.Select(x => new CustomerTimelineItemDto(
            x.FechaCotizacion,
            "Cotizacion",
            $"Cotizacion {x.Numero}",
            $"{(x.Producto is null ? "Producto" : ProductName(x.Producto))} por {x.PrecioProducto:C0}.",
            "info",
            x.Id)));

        foreach (var application in creditApplications)
        {
            items.Add(new CustomerTimelineItemDto(
                application.FechaCreacion,
                "Solicitud",
                $"Solicitud {application.Numero}",
                $"{(application.Producto is null ? "Producto" : ProductName(application.Producto))} - {application.Estado}.",
                application.Estado is EstadoSolicitudCredito.Rechazada ? "error" : application.Estado is EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada ? "success" : "warning",
                application.Id));

            AddDecision(items, application.FechaEnvio, "Solicitud enviada", $"{application.Numero} paso a documentos pendientes.", "info", application.Id);
            AddDecision(items, application.FechaInicioEstudio, "Estudio iniciado", $"{application.Numero} paso a estudio de credito.", "warning", application.Id);
            AddDecision(items, application.FechaAprobacion, "Credito aprobado", $"{application.Numero} fue aprobada por {application.UsuarioDecision ?? "el sistema"}.", "success", application.Id);
            AddDecision(items, application.FechaRechazo, "Credito rechazado", $"{application.Numero} fue rechazada por {application.UsuarioDecision ?? "el sistema"}.", "error", application.Id);
            AddDecision(items, application.FechaDesembolso, "Credito desembolsado", $"{application.Numero} fue desembolsada.", "success", application.Id);

            items.AddRange(application.Documentos
                .Where(x => x.FechaCarga.HasValue || x.FechaRecepcion.HasValue)
                .Select(x => new CustomerTimelineItemDto(
                    x.FechaCarga ?? x.FechaRecepcion ?? application.FechaCreacion,
                    "Documento",
                    $"{x.Nombre}: {x.Estado}",
                    x.NombreArchivo is null ? "Documento actualizado en la solicitud." : $"Archivo cargado: {x.NombreArchivo}.",
                    x.Estado is EstadoDocumentoCredito.Rechazado ? "error" : x.Estado is EstadoDocumentoCredito.Validado ? "success" : "info",
                    x.Id)));
        }

        items.AddRange(deals.Select(x => new CustomerTimelineItemDto(
            x.OccurredAt,
            "Pipeline",
            x.Deal.Title,
            $"{x.Deal.Status} - {x.Deal.Value:C0} - probabilidad {x.Deal.CloseProbability:N0}%.",
            x.Deal.Status is EstadoNegocio.Perdido ? "error" : x.Deal.Status is EstadoNegocio.Ganado ? "success" : "info",
            x.Deal.Id)));

        items.AddRange(activities.Select(x => new CustomerTimelineItemDto(
            x.ScheduledAt,
            "Actividad",
            x.Title,
            $"{x.Type} - {x.Status}. {x.Description ?? string.Empty}".Trim(),
            x.Status is EstadoActividad.Completada ? "success" : x.Status is EstadoActividad.Cancelada ? "error" : "warning",
            x.Id)));

        items.AddRange(notes.Select(x => new CustomerTimelineItemDto(
            x.FechaCreacion,
            "Nota",
            "Nota registrada",
            x.Contenido,
            "default",
            x.Id)));

        return items
            .OrderByDescending(x => x.OccurredAt)
            .Take(80)
            .ToList();
    }

    private static void AddDecision(ICollection<CustomerTimelineItemDto> items, DateTime? date, string title, string description, string tone, Guid relatedId)
    {
        if (!date.HasValue) return;
        items.Add(new CustomerTimelineItemDto(date.Value, "Decision", title, description, tone, relatedId));
    }

    private sealed record DealTimelineEntry(DealDto Deal, DateTime OccurredAt);

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }
}
