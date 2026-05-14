using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/commercial-reports")]
public sealed class CommercialReportsController(ICommercialReportService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CommercialReportsDto>> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(from, to, cancellationToken));
}
