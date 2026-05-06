using CrmSaas.Domain.Enums;

namespace CrmSaas.Application.DTOs;

public sealed record CustomerDto(Guid Id, string Name, string FirstNames, string LastNames, string? CompanyName, string Email, string? Phone, EstadoCliente Status, string? Tags);
public sealed record UpsertCustomerDto(string? Name, string FirstNames, string LastNames, string? CompanyName, string Email, string? Phone, EstadoCliente Status, string? Tags);

public sealed record LeadDto(Guid Id, string Name, string FirstNames, string LastNames, string Email, string? Phone, string Source, CalificacionProspecto Rating, bool Converted, Guid? CustomerId);
public sealed record UpsertLeadDto(string? Name, string FirstNames, string LastNames, string Email, string? Phone, string Source, CalificacionProspecto Rating);

public sealed record DealStageDto(Guid Id, string Name, int Order, decimal DefaultProbability, bool Active);
public sealed record UpsertDealStageDto(string Name, int Order, decimal DefaultProbability, bool Active);

public sealed record DealDto(Guid Id, string Title, Guid? CustomerId, Guid StageId, decimal Value, decimal CloseProbability, DateTime EstimatedCloseDate, EstadoNegocio Status);
public sealed record UpsertDealDto(string Title, Guid? CustomerId, Guid StageId, decimal Value, decimal CloseProbability, DateTime EstimatedCloseDate, EstadoNegocio Status);

public sealed record ActivityDto(Guid Id, string Title, string? Description, TipoActividad Type, EstadoActividad Status, DateTime ScheduledAt, DateTime? ReminderAt, Guid? CustomerId, Guid? DealId, Guid? AssignedUserId);
public sealed record UpsertActivityDto(string Title, string? Description, TipoActividad Type, EstadoActividad Status, DateTime ScheduledAt, DateTime? ReminderAt, Guid? CustomerId, Guid? DealId, Guid? AssignedUserId);

public sealed record ProductDto(Guid Id, string Brand, string Model, string Reference, int? EngineCc, int? Year, string? Color, decimal Price, bool Active);
public sealed record UpsertProductDto(string Brand, string Model, string Reference, int? EngineCc, int? Year, string? Color, decimal Price, bool Active);

public sealed record QuoteDto(
    Guid Id,
    string Number,
    TipoIdentificacionColombia IdentificationType,
    string? IdentificationNumber,
    string CustomerFirstNames,
    string CustomerLastNames,
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    decimal ProductPrice,
    decimal DownPayment,
    int TermMonths,
    decimal MonthlyInterestRate,
    decimal FinancedAmount,
    decimal EstimatedMonthlyPayment,
    decimal EstimatedTotalPayment,
    DateTime QuoteDate,
    DateTime ValidUntil,
    string? Notes);
public sealed record CreateQuoteDto(TipoIdentificacionColombia IdentificationType, string? IdentificationNumber, string CustomerFirstNames, string CustomerLastNames, Guid ProductId, decimal DownPayment, int TermMonths, decimal MonthlyInterestRate, string? Notes);

public sealed record CreditApplicationDto(
    Guid Id,
    string Number,
    Guid CustomerId,
    string CustomerName,
    Guid ProductId,
    string ProductName,
    Guid? QuoteId,
    Guid? DealId,
    TipoIdentificacionColombia IdentificationType,
    string IdentificationNumber,
    DateTime? BirthDate,
    string Mobile,
    string? Address,
    string? City,
    string? Occupation,
    decimal MonthlyIncome,
    decimal DownPayment,
    int TermMonths,
    decimal MotorcycleValue,
    EstadoSolicitudCredito Status,
    string? Notes,
    DateTime? SubmittedAt,
    DateTime? ReviewStartedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt,
    DateTime? DisbursedAt,
    string? DecisionUser,
    string? DecisionNotes,
    IReadOnlyCollection<CreditDocumentDto> Documents);

public sealed record UpsertCreditApplicationDto(
    Guid CustomerId,
    Guid ProductId,
    Guid? QuoteId,
    Guid? DealId,
    TipoIdentificacionColombia IdentificationType,
    string IdentificationNumber,
    DateTime? BirthDate,
    string Mobile,
    string? Address,
    string? City,
    string? Occupation,
    decimal MonthlyIncome,
    decimal DownPayment,
    int TermMonths,
    decimal MotorcycleValue,
    EstadoSolicitudCredito Status,
    string? Notes);

public sealed record CreditDocumentDto(
    Guid Id,
    TipoDocumentoCredito Type,
    string Name,
    EstadoDocumentoCredito Status,
    DateTime? ReceivedAt,
    string? Notes,
    bool HasFile,
    string? FileName,
    string? ContentType,
    long? SizeBytes,
    DateTime? UploadedAt);
public sealed record UpsertCreditDocumentDto(TipoDocumentoCredito Type, string Name, EstadoDocumentoCredito Status, DateTime? ReceivedAt, string? Notes);
public sealed record ChangeCreditApplicationStatusDto(EstadoSolicitudCredito Status);
public sealed record CreditApplicationDecisionDto(EstadoSolicitudCredito Status, string? Notes);

public sealed record Customer360Dto(
    CustomerDto Customer,
    IReadOnlyCollection<QuoteDto> Quotes,
    IReadOnlyCollection<CreditApplicationDto> CreditApplications,
    IReadOnlyCollection<DealDto> Deals,
    IReadOnlyCollection<ActivityDto> Activities,
    IReadOnlyCollection<CustomerTimelineItemDto> Timeline);

public sealed record CustomerTimelineItemDto(
    DateTime OccurredAt,
    string Type,
    string Title,
    string Description,
    string Tone,
    Guid? RelatedId);

public sealed record DashboardDto(decimal OpenPipelineValue, decimal WeightedPipelineValue, int ActiveCustomers, int OpenLeads, int PendingActivities, IReadOnlyCollection<RecentActivityDto> RecentActivities);
public sealed record RecentActivityDto(string Title, DateTime ScheduledAt, EstadoActividad Status);
