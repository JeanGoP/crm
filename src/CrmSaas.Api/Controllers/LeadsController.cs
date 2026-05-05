using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leads")]
public sealed class LeadsController(ILeadService service, IValidator<UpsertLeadDto> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<LeadDto>>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<LeadDto>> Create(UpsertLeadDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.CreateAsync(dto, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LeadDto>> Update(Guid id, UpsertLeadDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<ActionResult<LeadDto>> Convert(Guid id, CancellationToken cancellationToken) => Ok(await service.ConvertAsync(id, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
