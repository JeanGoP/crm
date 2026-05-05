using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IValidator<LoginRequestDto> loginValidator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenRequestDto request, CancellationToken cancellationToken) =>
        Ok(await authService.RefreshAsync(request, cancellationToken));
}
