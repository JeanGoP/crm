using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Services;

public sealed class ErpCreditExportService(CrmDbContext db, IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task QueueApprovedAsync(SolicitudCredito application, CancellationToken ct)
    {
        if (!Guid.TryParse(configuration["ErpIntegration:CrmCompanyId"], out var companyId)
            || companyId == Guid.Empty || companyId != application.EmpresaId)
            return;
        if (await db.ErpCreditExports.AnyAsync(x => x.SolicitudCreditoId == application.Id, ct)) return;

        var quote = application.CotizacionId.HasValue
            ? await db.Cotizaciones.Include(x => x.Items).ThenInclude(x => x.Producto)
                .FirstOrDefaultAsync(x => x.Id == application.CotizacionId.Value, ct)
            : null;
        var quoteItems = quote?.Items.OrderBy(x => x.Orden)
            .Where(x => quote.EsPaquete || x.ProductoId == application.ProductoId)
            .Select(x => new ErpCreditItem(
                x.Producto?.Referencia ?? application.Producto?.Referencia ?? string.Empty,
                x.Producto?.Nombre ?? application.Producto?.Nombre ?? string.Empty,
                x.PrecioProducto, 1)).ToArray();
        var items = quoteItems is { Length: > 0 } ? quoteItems :
            [new ErpCreditItem(application.Producto?.Referencia ?? string.Empty,
                application.Producto?.Nombre ?? string.Empty, application.ValorMoto, 1)];

        var export = new ErpCreditExport
        {
            EmpresaId = application.EmpresaId,
            SolicitudCreditoId = application.Id,
            ProximoIntentoUtc = DateTime.UtcNow
        };
        var customerName = string.Join(' ', new[] { application.Cliente?.Nombres, application.Cliente?.Apellidos }
            .Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        if (customerName.Length == 0) customerName = application.Cliente?.Nombre ?? string.Empty;
        export.PayloadJson = JsonSerializer.Serialize(new ErpApprovedCredit(
            export.Id, application.EmpresaId, configuration["ErpIntegration:CompanyNit"] ?? string.Empty,
            application.Id, application.Numero, DateTime.UtcNow,
            new ErpCreditCustomer(application.TipoIdentificacion.ToString(), application.NumeroIdentificacion,
                customerName, application.Celular), items,
            application.ValorAprobadoAnalista ?? application.ValorMoto,
            application.CuotaInicialAprobada ?? application.CuotaInicial,
            Math.Max(0, (application.ValorAprobadoAnalista ?? application.ValorMoto)
                - (application.CuotaInicialAprobada ?? application.CuotaInicial)),
            application.PlazoAprobadoMeses ?? application.PlazoMeses,
            application.FechaPrimerVencimiento,
            quote?.UsuarioCreacion ?? application.UsuarioCreacion,
            quote?.NombreSede), JsonOptions);
        db.ErpCreditExports.Add(export);
    }

    private sealed record ErpCreditCustomer(string IdentificationType, string IdentificationNumber, string Name, string? Phone);
    private sealed record ErpCreditItem(string Reference, string Name, decimal Price, int Quantity);
    private sealed record ErpApprovedCredit(Guid EventId, Guid CrmCompanyId, string CompanyNit,
        Guid ApplicationId, string ApplicationNumber, DateTime ApprovedAtUtc,
        ErpCreditCustomer Customer, ErpCreditItem[] Items, decimal ApprovedAmount,
        decimal DownPayment, decimal FinancedAmount, int TermMonths, DateTime? FirstDueDate,
        string? Advisor, string? SalesPoint);
}

public sealed class ErpCreditExportWorker(
    IServiceScopeFactory scopes, IHttpClientFactory clients, IConfiguration configuration,
    ILogger<ErpCreditExportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        do
        {
            try { await SendPendingAsync(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) { logger.LogError(error, "Falló el ciclo de envío CRM → ERP."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SendPendingAsync(CancellationToken ct)
    {
        if (!configuration.GetValue<bool>("ErpIntegration:Enabled")) return;
        if (!Guid.TryParse(configuration["ErpIntegration:CrmCompanyId"], out var companyId)
            || companyId == Guid.Empty) return;
        var baseUrl = configuration["ErpIntegration:BaseUrl"];
        var host = configuration["ErpIntegration:HostHeader"];
        var key = configuration["ErpIntegration:SharedKey"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttp || !IPAddress.IsLoopback(uri.HostNameType == UriHostNameType.IPv4
                || uri.HostNameType == UriHostNameType.IPv6 ? IPAddress.Parse(uri.Host) : IPAddress.None)
            || !uri.AbsolutePath.Equals("/ERP/", StringComparison.OrdinalIgnoreCase)
            || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0
            || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(key) || key.Length < 32)
        {
            logger.LogError("La integración ERP está habilitada sin una URL HTTP loopback, HostHeader y clave válidos.");
            return;
        }

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var due = await db.ErpCreditExports.IgnoreQueryFilters()
            .Where(x => x.EmpresaId == companyId && x.Estado == "Pendiente" && x.ProximoIntentoUtc <= DateTime.UtcNow)
            .OrderBy(x => x.ProximoIntentoUtc).Take(10).ToListAsync(ct);
        if (due.Count == 0) return;
        var client = clients.CreateClient(nameof(ErpCreditExportWorker));
        client.Timeout = TimeSpan.FromSeconds(10);
        foreach (var item in due)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post,
                    new Uri(uri, "api/v1/integrations/crm/approved-credits"));
                request.Headers.Host = host;
                request.Headers.Add("X-Crm-Integration-Key", key);
                request.Content = new StringContent(item.PayloadJson, System.Text.Encoding.UTF8, "application/json");
                using var response = await client.SendAsync(request, ct);
                if (response.IsSuccessStatusCode)
                {
                    using var receipt = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct);
                    item.ReciboErpId = receipt?.RootElement.GetProperty("receiptId").GetInt64();
                    item.Estado = "Enviado";
                    item.EnviadoUtc = DateTime.UtcNow;
                    item.UltimoError = null;
                }
                else
                {
                    item.UltimoError = $"ERP HTTP {(int)response.StatusCode}";
                    if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
                        item.Estado = "Revisar";
                }
            }
            catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException)
            {
                item.UltimoError = $"Error de transporte: {error.GetType().Name}";
            }
            item.Intentos++;
            item.ProximoIntentoUtc = DateTime.UtcNow.AddMinutes(Math.Min(30, Math.Pow(2, Math.Min(item.Intentos, 5))));
            await db.SaveChangesAsync(ct);
            if (item.Estado != "Enviado") logger.LogWarning("Exportación ERP pendiente o en revisión: {ApplicationId}, {Error}", item.SolicitudCreditoId, item.UltimoError);
        }
    }
}
