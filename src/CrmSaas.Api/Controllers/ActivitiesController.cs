using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public sealed class ActivitiesController(IActivityService service, IValidator<UpsertActivityDto> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ActivityDto>>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.CreateAsync(dto, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, UpsertActivityDto dto, CancellationToken cancellationToken)
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
}
