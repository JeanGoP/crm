namespace CrmSaas.Application.DTOs;

public sealed record LoginRequestDto(string? Login, string? Email, string Password, string? Tenant);
public sealed record RefreshTokenRequestDto(string RefreshToken);
public sealed record AuthResponseDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);
public sealed record UserDto(Guid Id, string FullName, string Login, string Email, IReadOnlyCollection<string> Roles, Guid CompanyId, Guid? SalesPointId, string? SalesPointName, IReadOnlyCollection<Guid> SupervisedSalesPointIds, IReadOnlyCollection<string> SupervisedSalesPointNames);
public sealed record CreateUserDto(string FullName, string Login, string Email, string Password, Guid CompanyId, Guid? SalesPointId, IReadOnlyCollection<string> Roles, IReadOnlyCollection<Guid>? SupervisedSalesPointIds);
public sealed record UpdateUserDto(string FullName, string Login, string Email, string? Password, Guid CompanyId, Guid? SalesPointId, IReadOnlyCollection<string> Roles, IReadOnlyCollection<Guid>? SupervisedSalesPointIds);
public sealed record RoleDto(Guid Id, string Name, string Description);
public sealed record CompanyDto(Guid Id, string Name, string Subdomain, string? CustomDomain, string? LogoDataUrl, string? ExternalInventoryDatabaseName, string? ExternalInventoryWarehouseCodes, bool Active);
public sealed record UpsertCompanyDto(string Name, string Subdomain, string? CustomDomain, string? LogoDataUrl, string? ExternalInventoryDatabaseName, string? ExternalInventoryWarehouseCodes, bool Active);
