using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activities")]
public sealed class ActivitiesController(IActivityService service, IValidator<UpsertActivityDto> validator) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    private bool CanSeeAll => User.IsInRole("Administrador") || User.IsInRole("Supervisor")
        || string.Equals(User.FindFirstValue("global_admin"), "true", StringComparison.OrdinalIgnoreCase);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ActivityDto>>> Get(CancellationToken cancellationToken) =>
        Ok(await service.GetAsync(CurrentUserId, CurrentUserEmail, CanSeeAll, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.CreateAsync(CanSeeAll ? dto : dto with { AssignedUserId = CurrentUserId }, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.UpdateAsync(id, dto, CurrentUserId, CurrentUserEmail, CanSeeAll, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
