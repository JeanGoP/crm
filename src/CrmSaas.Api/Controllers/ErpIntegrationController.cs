using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using CrmSaas.Application.Abstractions;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

public sealed record ErpCompanyChoice(long Id, string Name, string Nit);
public sealed record ErpIntegrationState(bool Available, bool SendingEnabled, long? SelectedCompanyId,
    string? SelectedCompanyName, string? SelectedCompanyNit, IReadOnlyList<ErpCompanyChoice> Companies, string? Message);
public sealed record SelectErpCompanyRequest(long ErpCompanyId);

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/erp-integration")]
public sealed class ErpIntegrationController(
    CrmDbContext db, ITenantContext tenant, IConfiguration configuration,
    IHttpClientFactory clients, ILogger<ErpIntegrationController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ErpIntegrationState>> Get(CancellationToken ct)
    {
        if (!IsPlatformAdmin()) return Forbid();
        var company = await CurrentCompanyAsync(ct);
        try
        {
            var choices = await AvailableCompaniesAsync(ct);
            return Ok(new ErpIntegrationState(true, configuration.GetValue<bool>("ErpIntegration:Enabled"), company.ErpEmpresaId, company.ErpEmpresaNombre,
                company.ErpEmpresaNit, choices, null));
        }
        catch (InvalidOperationException error)
        {
            return Ok(new ErpIntegrationState(false, configuration.GetValue<bool>("ErpIntegration:Enabled"), company.ErpEmpresaId, company.ErpEmpresaNombre,
                company.ErpEmpresaNit, [], error.Message));
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(error, "No se pudo consultar el catálogo de empresas ERP.");
            return Ok(new ErpIntegrationState(false, configuration.GetValue<bool>("ErpIntegration:Enabled"), company.ErpEmpresaId, company.ErpEmpresaNombre,
                company.ErpEmpresaNit, [], "No se pudo contactar el ERP desde el servidor CRM."));
        }
    }

    [HttpPut("company")]
    public async Task<ActionResult<ErpIntegrationState>> SelectCompany(SelectErpCompanyRequest request, CancellationToken ct)
    {
        if (!IsPlatformAdmin()) return Forbid();
        if (request.ErpCompanyId < 1) return BadRequest(new { detail = "Selecciona una empresa del ERP." });
        var company = await CurrentCompanyAsync(ct);
        IReadOnlyList<ErpCompanyChoice> choices;
        try { choices = await AvailableCompaniesAsync(ct); }
        catch (Exception error) when (error is InvalidOperationException or HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(error, "No se pudo validar la empresa ERP seleccionada.");
            return StatusCode(503, new { detail = "No se pudo validar la empresa con el ERP. Intenta nuevamente." });
        }
        var selected = choices.SingleOrDefault(x => x.Id == request.ErpCompanyId);
        if (selected is null) return BadRequest(new { detail = "La empresa seleccionada no está disponible para este CRM." });
        if (company.ErpEmpresaId != selected.Id)
        {
            if (await db.ErpCreditExports.AnyAsync(x => x.EmpresaId == company.Id, ct))
                return Conflict(new { detail = "Esta empresa ya tiene créditos enviados o pendientes. No se puede cambiar el destino automáticamente." });
        }
        company.ErpEmpresaId = selected.Id;
        company.ErpEmpresaNombre = selected.Name;
        company.ErpEmpresaNit = selected.Nit;
        await db.SaveChangesAsync(ct);
        return Ok(new ErpIntegrationState(true, configuration.GetValue<bool>("ErpIntegration:Enabled"), selected.Id, selected.Name, selected.Nit, choices, null));
    }

    private async Task<CrmSaas.Domain.Entities.Empresa> CurrentCompanyAsync(CancellationToken ct)
    {
        var id = tenant.EmpresaId ?? throw new InvalidOperationException("No se resolvió la empresa activa.");
        return await db.Empresas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Empresa CRM no encontrada.");
    }

    private bool IsPlatformAdmin() =>
        string.Equals(User.FindFirstValue(ClaimTypes.Email), "admin@demo.com", StringComparison.OrdinalIgnoreCase);

    private async Task<IReadOnlyList<ErpCompanyChoice>> AvailableCompaniesAsync(CancellationToken ct)
    {
        var url = configuration["ErpIntegration:BaseUrl"];
        var host = configuration["ErpIntegration:HostHeader"];
        var key = configuration["ErpIntegration:SharedKey"];
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttp || uri.AbsolutePath != "/ERP/"
            || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0
            || !IPAddress.TryParse(uri.Host, out var address) || !IPAddress.IsLoopback(address)
            || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException("La conexión interna con el ERP aún no está configurada en el servidor.");

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri, "api/v1/integrations/crm/companies"));
        request.Headers.Host = host;
        request.Headers.Add("X-Crm-Integration-Key", key);
        var client = clients.CreateClient(nameof(ErpIntegrationController));
        client.Timeout = TimeSpan.FromSeconds(8);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("El ERP todavía no habilita el catálogo de empresas para la integración.");
        return await response.Content.ReadFromJsonAsync<List<ErpCompanyChoice>>(cancellationToken: ct) ?? [];
    }
}
