using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/pipeline")]
public sealed class PipelineController(IPipelineService service, IValidator<UpsertDealDto> dealValidator) : ControllerBase
{
    [HttpGet("stages")]
    public async Task<ActionResult<IReadOnlyCollection<DealStageDto>>> GetStages(CancellationToken cancellationToken) => Ok(await service.GetStagesAsync(cancellationToken));

    [HttpPost("stages")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<DealStageDto>> CreateStage(UpsertDealStageDto dto, CancellationToken cancellationToken) => Ok(await service.CreateStageAsync(dto, cancellationToken));

    [HttpPut("stages/{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<DealStageDto>> UpdateStage(Guid id, UpsertDealStageDto dto, CancellationToken cancellationToken) => Ok(await service.UpdateStageAsync(id, dto, cancellationToken));

    [HttpGet("deals")]
    public async Task<ActionResult<IReadOnlyCollection<DealDto>>> GetDeals(CancellationToken cancellationToken) => Ok(await service.GetDealsAsync(cancellationToken));

    [HttpPost("deals")]
    public async Task<ActionResult<DealDto>> CreateDeal(UpsertDealDto dto, CancellationToken cancellationToken)
    {
        await dealValidator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.CreateDealAsync(dto, cancellationToken));
    }

    [HttpPut("deals/{id:guid}")]
    public async Task<ActionResult<DealDto>> UpdateDeal(Guid id, UpsertDealDto dto, CancellationToken cancellationToken)
    {
        await dealValidator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.UpdateDealAsync(id, dto, cancellationToken));
    }

    [HttpDelete("deals/{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> DeleteDeal(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteDealAsync(id, cancellationToken);
        return NoContent();
    }
}
