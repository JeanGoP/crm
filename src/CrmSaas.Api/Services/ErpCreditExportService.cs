using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Services;

public sealed class ErpCreditExportService(CrmDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task QueueApprovedAsync(SolicitudCredito application, CancellationToken ct)
    {
        var company = await db.Empresas.FirstOrDefaultAsync(x => x.Id == application.EmpresaId, ct);
        if (company?.ErpEmpresaId is null || string.IsNullOrWhiteSpace(company.ErpEmpresaNit)) return;
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
            export.Id, application.EmpresaId, company.ErpEmpresaId.Value, company.ErpEmpresaNit,
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
    private sealed record ErpApprovedCredit(Guid EventId, Guid CrmCompanyId, long ErpCompanyId, string CompanyNit,
        Guid ApplicationId, string ApplicationNumber, DateTime ApprovedAtUtc,
        ErpCreditCustomer Customer, ErpCreditItem[] Items, decimal ApprovedAmount,
        decimal DownPayment, decimal FinancedAmount, int TermMonths, DateTime? FirstDueDate,
        string? Advisor, string? SalesPoint);
}

public sealed class ErpCreditExportWorker(
    IServiceScopeFactory scopes, IHttpClientFactory clients, IConfiguration configuration,
    CrmErpTokenService tokens, ILogger<ErpCreditExportWorker> logger) : BackgroundService
{
    public static bool PayloadMatchesCompany(string payloadJson, Guid crmCompanyId, long erpCompanyId, string? erpNit)
    {
        try
        {
            using var payload = JsonDocument.Parse(payloadJson);
            var root = payload.RootElement;
            return root.TryGetProperty("crmCompanyId", out var crmValue) && crmValue.TryGetGuid(out var payloadCrmId)
                && payloadCrmId == crmCompanyId
                && root.TryGetProperty("erpCompanyId", out var erpValue) && erpValue.TryGetInt64(out var payloadErpId)
                && payloadErpId == erpCompanyId
                && root.TryGetProperty("companyNit", out var nitValue) && nitValue.GetString() == erpNit;
        }
        catch (JsonException) { return false; }
    }

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
        var (uri, host) = ErpLoopback.Resolve(configuration);

        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var linkedCompanies = await db.Empresas.IgnoreQueryFilters()
            .Where(x => x.Activa && x.ErpEmpresaId != null)
            .Select(x => new { x.Id, x.ErpEmpresaId, x.ErpEmpresaNit }).ToListAsync(ct);
        if (linkedCompanies.Count == 0) return;
        var client = clients.CreateClient(nameof(ErpCreditExportWorker));
        client.Timeout = TimeSpan.FromSeconds(10);
        foreach (var linkedCompany in linkedCompanies)
        {
            var companyId = linkedCompany.Id;
            var due = await db.ErpCreditExports.IgnoreQueryFilters()
                .Where(x => x.EmpresaId == companyId && x.Estado == "Pendiente" && x.ProximoIntentoUtc <= DateTime.UtcNow)
                .OrderBy(x => x.ProximoIntentoUtc).Take(10).ToListAsync(ct);
            foreach (var item in due)
            {
                try
                {
                    if (!PayloadMatchesCompany(item.PayloadJson, linkedCompany.Id,
                        linkedCompany.ErpEmpresaId!.Value, linkedCompany.ErpEmpresaNit))
                    {
                        item.Estado = "Revisar";
                        item.UltimoError = "El destino del evento no coincide con la empresa CRM vinculada.";
                    }
                    else
                    {
                        const string path = "/api/v1/integrations/crm/approved-credits";
                        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, path.TrimStart('/')));
                        request.Headers.Host = host;
                        request.Headers.Add("X-Crm-Integration-Token", tokens.Create("POST", path,
                            CrmErpTokenService.HashBody(item.PayloadJson)));
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
                }
                catch (JsonException)
                {
                    item.Estado = "Revisar";
                    item.UltimoError = "El evento ERP contiene datos inválidos.";
                }
                catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
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
}
