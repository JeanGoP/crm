using System.Net;
using CrmSaas.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

public sealed record ErpTokenVerificationRequest(string Token, string Method, string Path, string BodyHash);

[ApiController]
[Route("api/erp-integration/verify")]
public sealed class ErpTokenVerificationController(CrmErpTokenService tokens) : ControllerBase
{
    [HttpPost]
    public IActionResult Verify(ErpTokenVerificationRequest input)
    {
        var remote = HttpContext.Connection.RemoteIpAddress;
        if (remote is null || !IPAddress.IsLoopback(remote.IsIPv4MappedToIPv6 ? remote.MapToIPv4() : remote))
            return Unauthorized();
        if (input.Path is not ("/api/v1/integrations/crm/companies" or "/api/v1/integrations/crm/approved-credits")
            || !tokens.Verify(input.Token, input.Method, input.Path, input.BodyHash))
            return Unauthorized();
        return Ok(new { valid = true });
    }
}
