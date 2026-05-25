using CrmSaas.Domain.Enums;

namespace CrmSaas.Application.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string FirstNames,
    string LastNames,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? SecondLastName,
    TipoIdentificacionColombia? IdentificationType,
    string? IdentificationNumber,
    string? CompanyName,
    string Email,
    string? PhoneCountryCode,
    string? Phone,
    string? Address,
    string? City,
    DateTime? BirthDate,
    string? Occupation,
    EstadoCliente Status,
    string? Tags,
    string? Notes);
public sealed record UpsertCustomerDto(
    string? Name,
    string FirstNames,
    string LastNames,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? SecondLastName,
    TipoIdentificacionColombia? IdentificationType,
    string? IdentificationNumber,
    string? CompanyName,
    string? Email,
    string? PhoneCountryCode,
    string? Phone,
    string? Address,
    string? City,
    DateTime? BirthDate,
    string? Occupation,
    EstadoCliente Status,
    string? Tags,
    string? Notes);

public sealed record LeadDto(Guid Id, string Name, string FirstNames, string LastNames, string FirstName, string? MiddleName, string LastName, string? SecondLastName, string Email, string? Phone, string Source, CalificacionProspecto Rating, bool Converted, Guid? CustomerId);
public sealed record UpsertLeadDto(string? Name, string FirstNames, string LastNames, string? FirstName, string? MiddleName, string? LastName, string? SecondLastName, string Email, string? Phone, string Source, CalificacionProspecto Rating);

public sealed record DealStageDto(Guid Id, string Name, int Order, decimal DefaultProbability, bool Active);
public sealed record UpsertDealStageDto(string Name, int Order, decimal DefaultProbability, bool Active);

public sealed record DealDto(Guid Id, string Title, Guid? CustomerId, Guid StageId, decimal Value, decimal CloseProbability, DateTime EstimatedCloseDate, EstadoNegocio Status);
public sealed record UpsertDealDto(string Title, Guid? CustomerId, Guid StageId, decimal Value, decimal CloseProbability, DateTime EstimatedCloseDate, EstadoNegocio Status);

public sealed record ActivityDto(
    Guid Id,
    string Title,
    string? Description,
    TipoActividad Type,
    EstadoActividad Status,
    DateTime ScheduledAt,
    DateTime? ReminderAt,
    Guid? CustomerId,
    Guid? DealId,
    Guid? AssignedUserId,
    string? CustomerName,
    string? DealTitle);
public sealed record UpsertActivityDto(string Title, string? Description, TipoActividad Type, EstadoActividad Status, DateTime ScheduledAt, DateTime? ReminderAt, Guid? CustomerId, Guid? DealId, Guid? AssignedUserId);

public sealed record ProductDto(Guid Id, string Name, string Category, string Brand, string Model, string Reference, string? Description, int? EngineCc, int? Year, string? Color, decimal Price, bool Active);
public sealed record UpsertProductDto(string Name, string Category, string Brand, string Model, string Reference, string? Description, int? EngineCc, int? Year, string? Color, decimal Price, bool Active);

public sealed record QuoteDto(
    Guid Id,
    string Number,
    TipoIdentificacionColombia IdentificationType,
    string? IdentificationNumber,
    string CustomerFirstNames,
    string CustomerLastNames,
    string CustomerFirstName,
    string? CustomerMiddleName,
    string CustomerLastName,
    string? CustomerSecondLastName,
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
public sealed record CreateQuoteDto(
    TipoIdentificacionColombia IdentificationType,
    string? IdentificationNumber,
    string CustomerFirstNames,
    string CustomerLastNames,
    string? CustomerFirstName,
    string? CustomerMiddleName,
    string? CustomerLastName,
    string? CustomerSecondLastName,
    string? PhoneCountryCode,
    string? PhoneNumber,
    Guid ProductId,
    decimal DownPayment,
    int TermMonths,
    decimal MonthlyInterestRate,
    string? Notes);

public sealed record ColombianIdentityLookupDto(
    string DocumentNumber,
    string? DocumentType,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? SecondLastName,
    string? FullName,
    DateTime? DateOfBirth,
    DateTime? ExpeditionDate,
    string? ExpeditionCity,
    string? ExpeditionDepartment,
    string? Gender,
    bool? IsAlive,
    string Source);

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
    string? CoDebtorName,
    string? CoDebtorIdentification,
    string? CoDebtorMobile,
    string? CoDebtorRelationship,
    decimal? CoDebtorMonthlyIncome,
    string? Reference1Name,
    string? Reference1Mobile,
    string? Reference1Relationship,
    string? Reference2Name,
    string? Reference2Mobile,
    string? Reference2Relationship,
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
    string? CoDebtorName,
    string? CoDebtorIdentification,
    string? CoDebtorMobile,
    string? CoDebtorRelationship,
    decimal? CoDebtorMonthlyIncome,
    string? Reference1Name,
    string? Reference1Mobile,
    string? Reference1Relationship,
    string? Reference2Name,
    string? Reference2Mobile,
    string? Reference2Relationship,
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

public sealed record CustomerAiAnalysisDto(
    string Summary,
    IReadOnlyCollection<string> PendingItems,
    string RiskLevel,
    string Priority,
    string NextBestAction,
    string WhatsappMessage,
    IReadOnlyCollection<string> Signals);

public sealed record DashboardDto(
    decimal OpenPipelineValue,
    decimal WeightedPipelineValue,
    int ActiveCustomers,
    int OpenLeads,
    int PendingActivities,
    int OverdueActivities,
    int TodayActivities,
    IReadOnlyCollection<RecentActivityDto> RecentActivities,
    IReadOnlyCollection<CommercialAlertDto> Alerts);
public sealed record RecentActivityDto(string Title, DateTime ScheduledAt, EstadoActividad Status);
public sealed record CommercialAlertDto(string Type, string Severity, string Title, string Description, DateTime CreatedAt, string? ActionUrl);

public sealed record CommercialReportsDto(
    CommercialReportSummaryDto Summary,
    IReadOnlyCollection<SalesBySellerDto> SalesBySeller,
    IReadOnlyCollection<QuotesByStatusDto> QuotesByStatus,
    IReadOnlyCollection<CreditsByStatusDto> CreditsByStatus,
    IReadOnlyCollection<TopQuotedProductDto> TopQuotedProducts);

public sealed record CommercialReportSummaryDto(
    int TotalQuotes,
    int QuotesConvertedToCredit,
    decimal QuoteToCreditConversionRate,
    int ApprovedCredits,
    int RejectedCredits,
    decimal CreditApprovalRate,
    decimal ApprovedCreditAmount);

public sealed record SalesBySellerDto(string Seller, int Quotes, int ApprovedCredits, decimal ApprovedAmount);
public sealed record QuotesByStatusDto(string Status, int Count, decimal Amount);
public sealed record CreditsByStatusDto(string Status, int Count, decimal Amount);
public sealed record TopQuotedProductDto(Guid ProductId, string ProductName, string Brand, string Model, int QuoteCount, decimal QuotedAmount);
