using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService service, IValidator<UpsertCustomerDto> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CustomerDto>>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));

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
}
