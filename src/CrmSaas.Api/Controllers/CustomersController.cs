using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
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
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Items)
            .ThenInclude(x => x.Producto)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCotizacion)
            .ToListAsync(cancellationToken);
        var creditApplications = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
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
        var files = await db.Archivos
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var deliveries = await db.EntregasMoto
            .Include(x => x.Producto)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaEntrega)
            .ToListAsync(cancellationToken);
        var timeline = BuildTimeline(quotes, creditApplications, dealTimelineItems, activities, notes, files, deliveries);

        return Ok(new Customer360Dto(
            ToCustomerDto(customer),
            quotes.Select(ToQuoteDto).ToList(),
            creditApplications.Select(ToCreditApplicationDto).ToList(),
            deals,
            activities,
            timeline));
    }

    [HttpGet("{id:guid}/ai-analysis")]
    public async Task<ActionResult<CustomerAiAnalysisDto>> AiAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var customer = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        var quotes = await db.Cotizaciones
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Items)
            .ThenInclude(x => x.Producto)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCotizacion)
            .ToListAsync(cancellationToken);
        var creditApplications = await db.SolicitudesCredito
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var deals = await db.Negocios
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaActualizacion ?? x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var activities = await db.Actividades
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaProgramada)
            .ToListAsync(cancellationToken);

        return Ok(BuildAiAnalysis(customer, quotes, creditApplications, deals, activities));
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
        return new CustomerDto(
            x.Id,
            string.IsNullOrWhiteSpace(displayName) ? x.Nombre : displayName,
            x.Nombres,
            x.Apellidos,
            x.PrimerNombre,
            x.SegundoNombre,
            x.PrimerApellido,
            x.SegundoApellido,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.EmpresaCliente,
            x.Email,
            x.IndicativoTelefono,
            x.Telefono,
            x.Direccion,
            x.Ciudad,
            x.FechaNacimiento,
            x.Ocupacion,
            x.Estado,
            x.Etiquetas,
            x.Observaciones);
    }

    private static QuoteDto ToQuoteDto(Cotizacion x)
    {
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
        var termMonths = x.PlazoMeses <= 0 ? 24 : x.PlazoMeses;
        var financedAmount = x.ValorFinanciado <= 0 && x.CuotaMensualEstimada <= 0
            ? Math.Max(DiscountedPrice(x.PrecioProducto, x.DescuentoPromocion) + x.Seguro + x.GastosAdministrativos - x.CuotaInicial, 0)
            : x.ValorFinanciado;
        var totalPayment = x.TotalPagarEstimado <= 0 ? x.CuotaInicial + financedAmount : x.TotalPagarEstimado;
        return new QuoteDto(
            x.Id,
            x.Numero,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.NombresCliente,
            x.ApellidosCliente,
            x.PrimerNombreCliente,
            x.SegundoNombreCliente,
            x.PrimerApellidoCliente,
            x.SegundoApellidoCliente,
            x.ClienteId,
            x.ProductoId,
            productName,
            x.PuntoVentaId,
            x.NombreSede,
            x.MarcaSede,
            x.ModalidadEntregaSede,
            x.CondicionesSede,
            x.PerfilRequisitoId,
            x.PerfilRequisito?.Nombre,
            x.PromocionId,
            x.NombrePromocion,
            x.DescuentoPromocion,
            DiscountedPrice(x.PrecioProducto, x.DescuentoPromocion),
            x.PrecioProducto,
            x.CuotaInicial,
            x.Seguro,
            x.GastosAdministrativos,
            termMonths,
            x.TasaInteresMensual,
            financedAmount,
            x.CuotaMensualEstimada,
            totalPayment,
            x.TipoCredito,
            x.UsoConfiguracionFinancieraEmpresa,
            x.FechaCotizacion,
            x.ValidaHasta,
            x.Observaciones,
            QuoteItems(x).ToList());
    }

    private static IEnumerable<QuoteItemDto> QuoteItems(Cotizacion quote)
    {
        if (quote.Items.Count > 0)
        {
            return quote.Items.OrderBy(x => x.Orden).Select(ToItemDto);
        }

        return
        [
            new QuoteItemDto(
                quote.Id,
                quote.ProductoId,
                quote.Producto is null ? "Producto" : ProductName(quote.Producto),
                quote.PrecioProducto,
                quote.PromocionId,
                quote.NombrePromocion,
                quote.DescuentoPromocion,
                DiscountedPrice(quote.PrecioProducto, quote.DescuentoPromocion),
                quote.CuotaInicial,
                quote.Seguro,
                quote.GastosAdministrativos,
                quote.PlazoMeses <= 0 ? 24 : quote.PlazoMeses,
                quote.TasaInteresMensual,
                quote.ValorFinanciado,
                quote.CuotaMensualEstimada,
                quote.TotalPagarEstimado,
                quote.TipoCredito,
                quote.UsoConfiguracionFinancieraEmpresa,
                1)
        ];
    }

    private static QuoteItemDto ToItemDto(CotizacionItem item)
    {
        var financedAmount = item.ValorFinanciado <= 0 && item.CuotaMensualEstimada <= 0
            ? Math.Max(DiscountedPrice(item.PrecioProducto, item.DescuentoPromocion) + item.Seguro + item.GastosAdministrativos - item.CuotaInicial, 0)
            : item.ValorFinanciado;
        var totalPayment = item.TotalPagarEstimado <= 0 ? item.CuotaInicial + financedAmount : item.TotalPagarEstimado;
        return new QuoteItemDto(
            item.Id,
            item.ProductoId,
            item.Producto is null ? "Producto" : ProductName(item.Producto),
            item.PrecioProducto,
            item.PromocionId,
            item.NombrePromocion,
            item.DescuentoPromocion,
            DiscountedPrice(item.PrecioProducto, item.DescuentoPromocion),
            item.CuotaInicial,
            item.Seguro,
            item.GastosAdministrativos,
            item.PlazoMeses <= 0 ? 24 : item.PlazoMeses,
            item.TasaInteresMensual,
            financedAmount,
            item.CuotaMensualEstimada,
            totalPayment,
            item.TipoCredito,
            item.UsoConfiguracionFinancieraEmpresa,
            item.Orden);
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
            x.PerfilRequisitoId,
            x.PerfilRequisito?.Nombre,
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
                d.ClienteId,
                d.Tipo,
                d.Nombre,
                d.Estado,
                d.FechaRecepcion,
                d.FechaVencimiento,
                d.Observaciones,
                d.FechaRechazo,
                d.MotivoRechazo,
                d.FechaValidacion,
                d.UsuarioValidacion,
                d.FechaVencimiento.HasValue && d.FechaVencimiento.Value.Date < ColombiaTime.Now.Date,
                d.FechaVencimiento.HasValue ? (int)(d.FechaVencimiento.Value.Date - ColombiaTime.Now.Date).TotalDays : null,
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
        IReadOnlyCollection<Nota> notes,
        IReadOnlyCollection<Archivo> files,
        IReadOnlyCollection<EntregaMoto> deliveries)
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
                $"{(application.Producto is null ? "Producto" : ProductName(application.Producto))} - {CreditStatus(application.Estado)}.",
                application.Estado is EstadoSolicitudCredito.Rechazada or EstadoSolicitudCredito.Desistida ? "error" : application.Estado is EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada ? "success" : "warning",
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

        items.AddRange(files.Select(x => new CustomerTimelineItemDto(
            x.FechaCreacion,
            "Archivo",
            x.Nombre,
            $"Archivo adjunto al cliente ({x.ContentType}, {ReadableBytes(x.TamanoBytes)}).",
            "default",
            x.Id)));

        items.AddRange(deliveries.Select(x => new CustomerTimelineItemDto(
            x.FechaEntrega,
            "Entrega",
            $"Entrega {x.Numero}",
            $"{(x.Producto is null ? "Producto" : ProductName(x.Producto))} - {DeliveryStatus(x.Estado)}{(string.IsNullOrWhiteSpace(x.Placa) ? string.Empty : $" - placa {x.Placa}")}.",
            x.Estado is EstadoEntregaMoto.Entregada ? "success" : x.Estado is EstadoEntregaMoto.Cancelada ? "error" : "warning",
            x.Id)));

        return items
            .OrderByDescending(x => x.OccurredAt)
            .Take(120)
            .ToList();
    }

    private static void AddDecision(ICollection<CustomerTimelineItemDto> items, DateTime? date, string title, string description, string tone, Guid relatedId)
    {
        if (!date.HasValue) return;
        items.Add(new CustomerTimelineItemDto(date.Value, "Decision", title, description, tone, relatedId));
    }

    private static CustomerAiAnalysisDto BuildAiAnalysis(
        Cliente customer,
        IReadOnlyCollection<Cotizacion> quotes,
        IReadOnlyCollection<SolicitudCredito> creditApplications,
        IReadOnlyCollection<Negocio> deals,
        IReadOnlyCollection<Actividad> activities)
    {
        var customerName = $"{customer.Nombres} {customer.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName)) customerName = customer.Nombre;

        var latestQuote = quotes.OrderByDescending(x => x.FechaCotizacion).FirstOrDefault();
        var latestApplication = creditApplications.OrderByDescending(x => x.FechaCreacion).FirstOrDefault();
        var openDeal = deals.FirstOrDefault(x => x.Estado == EstadoNegocio.Abierto);
        var today = ColombiaTime.Today;
        var pendingActivities = activities.Where(x => x.Estado is EstadoActividad.Pendiente or EstadoActividad.EnProceso).ToList();
        var overdueActivities = pendingActivities.Where(x => x.FechaProgramada.Date < today).ToList();
        var futureActivities = pendingActivities.Where(x => x.FechaProgramada.Date >= today).ToList();
        var lastContactDate = new[] {
                latestQuote?.FechaCotizacion,
                latestApplication?.FechaCreacion,
                activities.FirstOrDefault()?.FechaProgramada
            }
            .Where(x => x.HasValue)
            .Select(x => x!.Value.Date)
            .DefaultIfEmpty(customer.FechaCreacion.Date)
            .Max();
        var daysWithoutFollowUp = Math.Max(0, (today - lastContactDate).Days);

        var pending = new List<string>();
        var signals = new List<string>();

        if (latestQuote is null) pending.Add("No tiene cotizaciones registradas.");
        else signals.Add($"Ultima cotizacion: {latestQuote.Numero} para {ProductName(latestQuote.Producto!)} por {Money(latestQuote.PrecioProducto)}.");

        if (latestApplication is not null)
        {
            signals.Add($"Solicitud de credito {latestApplication.Numero} en estado {CreditStatus(latestApplication.Estado)}.");
            var pendingDocs = latestApplication.Documentos
                .Where(x => x.Estado is EstadoDocumentoCredito.Pendiente or EstadoDocumentoCredito.Rechazado)
                .Select(x => $"{x.Nombre}: {x.Estado}")
                .ToList();
            pending.AddRange(pendingDocs.Select(x => $"Documento pendiente o por corregir: {x}."));
            if (string.IsNullOrWhiteSpace(latestApplication.CodeudorNombre) && latestApplication.IngresosMensuales <= 0)
                pending.Add("Revisar ingresos y posible codeudor antes de avanzar el credito.");
        }
        else if (latestQuote is not null)
        {
            pending.Add("Aun no tiene solicitud de credito asociada a la cotizacion.");
        }

        if (openDeal is not null) signals.Add($"Negocio abierto: {openDeal.Titulo} con probabilidad {openDeal.ProbabilidadCierre:N0}%.");
        if (overdueActivities.Count > 0) pending.Add($"Tiene {overdueActivities.Count} actividad(es) vencida(s).");
        if (futureActivities.Count == 0) pending.Add("No tiene una actividad futura programada.");
        if (daysWithoutFollowUp >= 5) pending.Add($"No registra seguimiento reciente hace {daysWithoutFollowUp} dias.");

        if (pending.Count == 0) pending.Add("No se detectan pendientes criticos en este momento.");

        var riskLevel = "Bajo";
        var priority = "Media";
        if (latestApplication?.Estado is EstadoSolicitudCredito.Rechazada or EstadoSolicitudCredito.Desistida || overdueActivities.Count > 0 || latestApplication?.Documentos.Any(x => x.Estado == EstadoDocumentoCredito.Rechazado) == true)
        {
            riskLevel = "Alto";
            priority = "Alta";
        }
        else if (latestApplication?.Documentos.Any(x => x.Estado == EstadoDocumentoCredito.Pendiente) == true || latestApplication?.Estado is EstadoSolicitudCredito.DocumentosPendientes or EstadoSolicitudCredito.EnEstudio || daysWithoutFollowUp >= 5)
        {
            riskLevel = "Medio";
            priority = "Alta";
        }
        else if (latestQuote is not null && latestApplication is null)
        {
            riskLevel = "Medio";
            priority = "Media";
        }

        var productName = latestApplication?.Producto is not null
            ? ProductName(latestApplication.Producto)
            : latestQuote?.Producto is not null
                ? ProductName(latestQuote.Producto)
                : "el producto de interes";

        var nextAction = latestApplication?.Documentos.Any(x => x.Estado is EstadoDocumentoCredito.Pendiente or EstadoDocumentoCredito.Rechazado) == true
            ? "Contactar al cliente para solicitar o corregir los documentos pendientes."
            : overdueActivities.Count > 0
                ? "Resolver la actividad vencida y registrar el resultado del seguimiento."
                : latestApplication?.Estado == EstadoSolicitudCredito.EnEstudio
                    ? "Hacer seguimiento al estudio de credito y confirmar si requiere informacion adicional."
                    : latestQuote is not null && latestApplication is null
                        ? "Contactar al cliente para confirmar interes y avanzar hacia solicitud de credito."
                        : "Programar una llamada de seguimiento comercial.";

        var firstName = string.IsNullOrWhiteSpace(customer.Nombres) ? customerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "cliente" : customer.Nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "cliente";
        var whatsappMessage = $"Hola {firstName}, te escribo para hacer seguimiento a tu proceso con {productName}. {WhatsappCallToAction(nextAction)}";
        var summary = BuildSummary(customerName, latestQuote, latestApplication, openDeal, daysWithoutFollowUp);

        if (signals.Count == 0) signals.Add("Cliente registrado sin proceso comercial avanzado.");

        return new CustomerAiAnalysisDto(summary, pending, riskLevel, priority, nextAction, whatsappMessage, signals);
    }

    private static string BuildSummary(string customerName, Cotizacion? quote, SolicitudCredito? application, Negocio? deal, int daysWithoutFollowUp)
    {
        var parts = new List<string> { $"{customerName} tiene un proceso comercial en seguimiento." };
        if (quote is not null) parts.Add($"Su ultima cotizacion es {quote.Numero} por {ProductName(quote.Producto!)}.");
        if (application is not null) parts.Add($"La solicitud de credito {application.Numero} esta en estado {CreditStatus(application.Estado)}.");
        if (deal is not null) parts.Add($"Tiene un negocio abierto con probabilidad {deal.ProbabilidadCierre:N0}%.");
        if (daysWithoutFollowUp > 0) parts.Add($"El ultimo movimiento detectado fue hace {daysWithoutFollowUp} dia(s).");
        return string.Join(" ", parts);
    }

    private static string WhatsappCallToAction(string nextAction)
    {
        if (nextAction.Contains("documentos", StringComparison.OrdinalIgnoreCase)) return "Nos falta completar algunos documentos para continuar. Me los puedes enviar por este medio?";
        if (nextAction.Contains("credito", StringComparison.OrdinalIgnoreCase)) return "Quiero confirmar contigo la informacion para avanzar con el credito. Me confirmas si seguimos adelante?";
        if (nextAction.Contains("cotizacion", StringComparison.OrdinalIgnoreCase)) return "Quiero saber si pudiste revisar la cotizacion y si deseas que avancemos con la solicitud.";
        return "Quedo atento para ayudarte a continuar con el proceso.";
    }

    private static string Money(decimal value) => "$" + value.ToString("N0");

    private static string ReadableBytes(long bytes)
    {
        if (bytes >= 1024 * 1024) return $"{bytes / 1024m / 1024m:N1} MB";
        if (bytes >= 1024) return $"{bytes / 1024m:N1} KB";
        return $"{bytes:N0} bytes";
    }

    private sealed record DealTimelineEntry(DealDto Deal, DateTime OccurredAt);

    private static string CreditStatus(EstadoSolicitudCredito status) => status switch
    {
        EstadoSolicitudCredito.Borrador => "Cotizado",
        EstadoSolicitudCredito.DocumentosPendientes => "Documentos pendientes",
        EstadoSolicitudCredito.DocumentosRecibidos => "Credito en estudio",
        EstadoSolicitudCredito.EnEstudio => "Credito en estudio",
        EstadoSolicitudCredito.Aprobada => "Aprobado",
        EstadoSolicitudCredito.Rechazada => "Rechazado",
        EstadoSolicitudCredito.Desembolsada => "Entregado",
        EstadoSolicitudCredito.Interesado => "Interesado",
        EstadoSolicitudCredito.Desistida => "Desistido",
        _ => status.ToString()
    };

    private static string DeliveryStatus(EstadoEntregaMoto status) => status switch
    {
        EstadoEntregaMoto.Programada => "Programada",
        EstadoEntregaMoto.Entregada => "Entregada",
        EstadoEntregaMoto.Cancelada => "Cancelada",
        _ => status.ToString()
    };

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static decimal DiscountedPrice(decimal productPrice, decimal discount) => Math.Max(productPrice - Math.Max(discount, 0), 0);
}
