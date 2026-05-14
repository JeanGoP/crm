namespace CrmSaas.Application.DTOs;

public sealed record LoginRequestDto(string Email, string Password, string? Tenant);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public sealed record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);
public sealed record UserDto(Guid Id, string FullName, string Email, IReadOnlyCollection<string> Roles, Guid CompanyId);
public sealed record CreateUserDto(string FullName, string Email, string Password, Guid CompanyId, IReadOnlyCollection<string> Roles);
public sealed record RoleDto(Guid Id, string Name, string Description);
public sealed record CompanyDto(Guid Id, string Name, string Subdomain, string? CustomDomain, string? LogoDataUrl, bool Active);
public sealed record UpsertCompanyDto(string Name, string Subdomain, string? CustomDomain, string? LogoDataUrl, bool Active);
