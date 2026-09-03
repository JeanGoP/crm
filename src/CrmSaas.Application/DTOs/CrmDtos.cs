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

public sealed record ProductPhotoDto(Guid Id, string FileName, string ContentType, long SizeBytes, bool IsQuoteDefault, string DataUrl);
public sealed record ProductCategoryDto(Guid Id, string Name, string? Description, bool QuoteAsBundle, bool Active);
public sealed record UpsertProductCategoryDto(string Name, string? Description, bool QuoteAsBundle, bool Active);
public sealed record ProductSalesPointPriceDto(Guid SalesPointId, string? SalesPointName, decimal Price, DateTime? PriceValidFrom, bool Active);
public sealed record UpsertProductSalesPointPriceDto(Guid SalesPointId, decimal Price, DateTime? PriceValidFrom, bool Active);
public sealed record ProductDto(Guid Id, string Name, string Category, string Brand, string Model, string? Line, string? Version, string Reference, string? Description, int? EngineCc, int? Year, string? Color, decimal Price, decimal Soat, decimal RegistrationFee, decimal Taxes, string? TechnicalSheet, DateTime? PriceValidFrom, bool Active, IReadOnlyCollection<ProductPhotoDto> Photos, IReadOnlyCollection<ProductSalesPointPriceDto> SalesPointPrices);
public sealed record UpsertProductDto(string Name, string Category, string Brand, string Model, string? Line, string? Version, string Reference, string? Description, int? EngineCc, int? Year, string? Color, decimal Price, decimal Soat, decimal RegistrationFee, decimal Taxes, string? TechnicalSheet, DateTime? PriceValidFrom, bool Active, IReadOnlyCollection<UpsertProductSalesPointPriceDto>? SalesPointPrices);
public sealed record ProductInventorySyncResultDto(int Created, int Existing, int Skipped, int PendingPrice, IReadOnlyCollection<string> Warnings);
public sealed record ExternalInventoryItemDto(string WarehouseCode, string WarehouseName, string Code, string Name, string? Presentation, string? SerialNumber, string? EngineNumber, string? ChassisNumber, int Quantity, Guid? ProductId, string? ProductName, decimal? ProductPrice, bool ProductActive, bool IsInCatalog);
public sealed record ExternalInventoryWarehouseDto(string Code, string Name);

public sealed record CommercialInventoryDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid SalesPointId,
    string SalesPointName,
    string? Vin,
    string? ChassisNumber,
    string? EngineNumber,
    string? Plate,
    string? Color,
    bool IsUsed,
    int? Mileage,
    EstadoInventarioComercial Status,
    Guid? ReservedCustomerId,
    string? ReservedCustomerName,
    Guid? ReservedQuoteId,
    string? ReservedQuoteNumber,
    Guid? ReservedCreditApplicationId,
    string? ReservedCreditApplicationNumber,
    DateTime? ReservedAt,
    DateTime? ReservationExpiresAt,
    bool ReservationExpired,
    string? Notes);

public sealed record UpsertCommercialInventoryDto(
    Guid ProductId,
    Guid SalesPointId,
    string? Vin,
    string? ChassisNumber,
    string? EngineNumber,
    string? Plate,
    string? Color,
    bool IsUsed,
    int? Mileage,
    EstadoInventarioComercial Status,
    string? Notes);

public sealed record ReserveCommercialInventoryDto(
    Guid? CustomerId,
    Guid? QuoteId,
    Guid? CreditApplicationId,
    DateTime? ReservationExpiresAt,
    string? Notes);

public sealed record CommercialInventorySummaryDto(Guid ProductId, string ProductName, Guid SalesPointId, string SalesPointName, int Available, int Reserved, int Sold, int Used, int Unavailable);

public sealed record FinancialSettingsDto(
    Guid Id,
    decimal MinimumWage,
    decimal ConsumerAnnualRate,
    decimal LowAmountAnnualRate,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    int PaymentRounding,
    bool UseMontelibanoTable,
    bool Active);
public sealed record UpsertFinancialSettingsDto(
    decimal MinimumWage,
    decimal ConsumerAnnualRate,
    decimal LowAmountAnnualRate,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    int PaymentRounding,
    bool UseMontelibanoTable,
    bool Active);

public sealed record QuoteChargeConceptDto(
    Guid Id,
    string Name,
    string Code,
    string CalculationGroup,
    string DefaultValueSource,
    decimal DefaultAmount,
    int Order,
    bool Active);
public sealed record UpsertQuoteChargeConceptDto(
    string Name,
    string Code,
    string CalculationGroup,
    string DefaultValueSource,
    decimal DefaultAmount,
    int Order,
    bool Active);

public sealed record SalesPointDto(
    Guid Id,
    string Name,
    string Code,
    string City,
    string? Address,
    string? Phone,
    string MainBrand,
    string? BrandLogoDataUrl,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    int QuoteValidityDays,
    string DeliveryMode,
    int SoatDays,
    int RegistrationDays,
    string? SoatProvider,
    string? RegistrationAgent,
    string? CommercialTerms,
    string? ExternalInventoryWarehouseCodes,
    IReadOnlyCollection<SalesPointRateDto> Rates,
    bool Active);

public sealed record SalesPointRateDto(
    Guid Id,
    string Name,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    bool Active);

public sealed record QuoteSalesPointDto(
    Guid Id,
    string Name,
    string City,
    IReadOnlyCollection<SalesPointRateDto> Rates);

public sealed record UpsertSalesPointRateDto(
    Guid? Id,
    string Name,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    bool Active);

public sealed record UpsertSalesPointDto(
    string Name,
    string Code,
    string City,
    string? Address,
    string? Phone,
    string MainBrand,
    string? BrandLogoDataUrl,
    decimal FactorMonthlyRate,
    int MaxTermMonths,
    int QuoteValidityDays,
    string DeliveryMode,
    int SoatDays,
    int RegistrationDays,
    string? SoatProvider,
    string? RegistrationAgent,
    string? CommercialTerms,
    string? ExternalInventoryWarehouseCodes,
    IReadOnlyCollection<UpsertSalesPointRateDto>? Rates,
    bool Active);

public sealed record QuoteSimulationDto(
    Guid ProductId,
    decimal ProductPrice,
    decimal DownPayment,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    Guid? SalesPointId,
    Guid? SalesPointRateId);
public sealed record QuoteSimulationResultDto(
    decimal DownPayment,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    Guid? SalesPointRateId,
    string? SalesPointRateName,
    Guid? PromotionId,
    string? PromotionName,
    decimal PromotionDiscount,
    decimal DiscountedProductPrice,
    decimal FinancedAmount,
    decimal EstimatedMonthlyPayment,
    decimal EstimatedTotalPayment,
    string CreditType,
    bool UsedCompanyFinancialSettings);

public sealed record QuoteInitialPaymentDto(
    DateTime DueDate,
    decimal Amount);

public sealed record QuoteItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal ProductPrice,
    Guid? PromotionId,
    string? PromotionName,
    decimal PromotionDiscount,
    decimal DiscountedProductPrice,
    decimal DownPayment,
    decimal InitialPaymentPaidToday,
    decimal InitialPaymentBalance,
    DateTime? CreditStartDate,
    IReadOnlyCollection<QuoteInitialPaymentDto> InitialPaymentSchedule,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    decimal FinancedAmount,
    decimal EstimatedMonthlyPayment,
    decimal EstimatedTotalPayment,
    string? CreditType,
    bool UsedCompanyFinancialSettings,
    int Order,
    string? InventoryWarehouseCode = null,
    string? InventoryWarehouseName = null,
    string? InventoryPresentation = null,
    string? InventorySerialNumber = null,
    string? InventoryEngineNumber = null,
    string? InventoryChassisNumber = null);

public sealed record CreateQuoteItemDto(
    Guid ProductId,
    decimal ProductPrice,
    decimal DownPayment,
    decimal InitialPaymentPaidToday,
    IReadOnlyCollection<QuoteInitialPaymentDto>? InitialPaymentSchedule,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    string? InventoryWarehouseCode = null,
    string? InventoryWarehouseName = null,
    string? InventoryPresentation = null,
    string? InventorySerialNumber = null,
    string? InventoryEngineNumber = null,
    string? InventoryChassisNumber = null);

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
    string? ProductTechnicalSheet,
    Guid? SalesPointId,
    string? SalesPointName,
    string? SalesPointBrand,
    string? SalesPointDeliveryMode,
    string? SalesPointCommercialTerms,
    Guid? SalesPointRateId,
    string? SalesPointRateName,
    Guid? RequirementProfileId,
    string? RequirementProfileName,
    Guid? PromotionId,
    string? PromotionName,
    decimal PromotionDiscount,
    decimal DiscountedProductPrice,
    decimal ProductPrice,
    decimal DownPayment,
    decimal InitialPaymentPaidToday,
    decimal InitialPaymentBalance,
    DateTime? CreditStartDate,
    IReadOnlyCollection<QuoteInitialPaymentDto> InitialPaymentSchedule,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    decimal FinancedAmount,
    decimal EstimatedMonthlyPayment,
    decimal EstimatedTotalPayment,
    string? CreditType,
    bool UsedCompanyFinancialSettings,
    DateTime QuoteDate,
    DateTime ValidUntil,
    string? Notes,
    IReadOnlyCollection<QuoteItemDto> Items);
public sealed record CreateQuoteDto(
    TipoIdentificacionColombia IdentificationType,
    string? IdentificationNumber,
    string? CustomerFirstNames,
    string? CustomerLastNames,
    string? CustomerFirstName,
    string? CustomerMiddleName,
    string? CustomerLastName,
    string? CustomerSecondLastName,
    string? PhoneCountryCode,
    string? PhoneNumber,
    Guid? RequirementProfileId,
    Guid ProductId,
    IReadOnlyCollection<CreateQuoteItemDto>? Items,
    decimal DownPayment,
    decimal Insurance,
    decimal AdministrativeFees,
    int TermMonths,
    decimal MonthlyInterestRate,
    Guid? SalesPointId,
    Guid? SalesPointRateId,
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
    Guid? RequirementProfileId,
    string? RequirementProfileName,
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
    string? CoDebtorReference1Name,
    string? CoDebtorReference1Mobile,
    string? CoDebtorReference1Relationship,
    string? CoDebtorReference2Name,
    string? CoDebtorReference2Mobile,
    string? CoDebtorReference2Relationship,
    EstadoSolicitudCredito Status,
    string? Notes,
    DateTime? SubmittedAt,
    DateTime? ReviewStartedAt,
    DateTime? Step0ReviewedAt,
    bool RuntChecked,
    bool SimitChecked,
    bool IdentityValidated,
    string? Step0User,
    string? Step0Notes,
    decimal? AnalystApprovedAmount,
    decimal? ApprovedDownPayment,
    int? ApprovedTermMonths,
    decimal? ApprovedMonthlyPayment,
    bool RequiresCoDebtorForApproval,
    string? FinalConditions,
    string? StudyResult,
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
    Guid? RequirementProfileId,
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
    string? CoDebtorReference1Name,
    string? CoDebtorReference1Mobile,
    string? CoDebtorReference1Relationship,
    string? CoDebtorReference2Name,
    string? CoDebtorReference2Mobile,
    string? CoDebtorReference2Relationship,
    EstadoSolicitudCredito Status,
    string? Notes);

public sealed record CreditDocumentDto(
    Guid Id,
    Guid? CustomerId,
    TipoDocumentoCredito Type,
    string Name,
    EstadoDocumentoCredito Status,
    DateTime? ReceivedAt,
    DateTime? ExpiresAt,
    string? Notes,
    DateTime? RejectedAt,
    string? RejectionReason,
    DateTime? ValidatedAt,
    string? ValidatedBy,
    bool IsExpired,
    int? DaysToExpire,
    bool HasFile,
    string? FileName,
    string? ContentType,
    long? SizeBytes,
    DateTime? UploadedAt);
public sealed record UpsertCreditDocumentDto(TipoDocumentoCredito Type, string Name, EstadoDocumentoCredito Status, DateTime? ReceivedAt, DateTime? ExpiresAt, string? Notes, string? RejectionReason);
public sealed record ChangeCreditApplicationStatusDto(EstadoSolicitudCredito Status);
public sealed record CreditApplicationDecisionDto(
    EstadoSolicitudCredito Status,
    string? Notes,
    string? Result,
    decimal? ApprovedAmount,
    decimal? ApprovedDownPayment,
    int? ApprovedTermMonths,
    decimal? ApprovedMonthlyPayment,
    bool RequiresCoDebtor,
    string? FinalConditions);
public sealed record CreditStudyStep0Dto(bool RuntChecked, bool SimitChecked, bool IdentityValidated, string? Notes);
public sealed record CreditStudyRecalculationDto(decimal ApprovedAmount, decimal ApprovedDownPayment, int ApprovedTermMonths, decimal ApprovedMonthlyPayment, string? Notes);

public sealed record RequirementDocumentDto(
    Guid Id,
    TipoDocumentoCredito Type,
    string Name,
    string? Description,
    bool Required,
    int Order);

public sealed record UpsertRequirementDocumentDto(
    TipoDocumentoCredito Type,
    string Name,
    string? Description,
    bool Required,
    int Order);

public sealed record RequirementProfileDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsCash,
    bool Active,
    IReadOnlyCollection<RequirementDocumentDto> Documents);

public sealed record UpsertRequirementProfileDto(
    string Name,
    string Code,
    string? Description,
    bool IsCash,
    bool Active,
    IReadOnlyCollection<UpsertRequirementDocumentDto> Documents);

public sealed record PromotionDto(
    Guid Id,
    string Name,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    Guid? ProductId,
    string? ProductName,
    string? Brand,
    string? Color,
    Guid? SalesPointId,
    string? SalesPointName,
    IReadOnlyCollection<Guid> SalesPointIds,
    IReadOnlyCollection<string> SalesPointNames,
    DateTime ValidFrom,
    DateTime ValidUntil,
    bool Active);

public sealed record UpsertPromotionDto(
    string Name,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    Guid? ProductId,
    string? Brand,
    string? Color,
    Guid? SalesPointId,
    IReadOnlyCollection<Guid>? SalesPointIds,
    DateTime ValidFrom,
    DateTime ValidUntil,
    bool Active);

public sealed record MotorcycleDeliveryDto(
    Guid Id,
    string Number,
    Guid CreditApplicationId,
    string CreditApplicationNumber,
    Guid CustomerId,
    string CustomerName,
    Guid ProductId,
    string ProductName,
    DateTime DeliveryDate,
    string? ResponsibleAdvisor,
    string? Vin,
    string? ChassisNumber,
    string? EngineNumber,
    string? Plate,
    int? DeliveryMileage,
    bool HelmetDelivered,
    bool SoatDelivered,
    bool RegistrationDelivered,
    bool WarrantyManualDelivered,
    bool DeliveryCertificateSigned,
    bool PreDeliveryChecklistCompleted,
    string? DeliveryProtocol,
    string? DeliveryPhotoDataUrl,
    string? DeliveryPhotoFileName,
    DateTime? FirstServiceScheduledAt,
    Guid? FirstServiceActivityId,
    EstadoEntregaMoto Status,
    string? Notes);

public sealed record UpsertMotorcycleDeliveryDto(
    Guid CreditApplicationId,
    DateTime DeliveryDate,
    string? ResponsibleAdvisor,
    string? Vin,
    string? ChassisNumber,
    string? EngineNumber,
    string? Plate,
    int? DeliveryMileage,
    bool HelmetDelivered,
    bool SoatDelivered,
    bool RegistrationDelivered,
    bool WarrantyManualDelivered,
    bool DeliveryCertificateSigned,
    bool PreDeliveryChecklistCompleted,
    string? DeliveryProtocol,
    string? DeliveryPhotoDataUrl,
    string? DeliveryPhotoFileName,
    DateTime? FirstServiceScheduledAt,
    EstadoEntregaMoto Status,
    string? Notes);

public sealed record CollectionOrderDetailDto(
    Guid Id,
    TipoConceptoRecaudo Type,
    string Concept,
    decimal Amount);

public sealed record CollectionOrderDto(
    Guid Id,
    string Number,
    Guid CreditApplicationId,
    string CreditApplicationNumber,
    Guid CustomerId,
    string CustomerName,
    DateTime IssueDate,
    DateTime DueDate,
    decimal VehicleAmount,
    decimal DocumentsAmount,
    decimal AdvanceAmount,
    decimal Total,
    decimal PaidAmount,
    decimal Balance,
    DateTime? PaidAt,
    EstadoOrdenRecaudo Status,
    string? Notes,
    IReadOnlyCollection<CollectionOrderDetailDto> Details);

public sealed record UpsertCollectionOrderDto(
    Guid CreditApplicationId,
    DateTime DueDate,
    decimal VehicleAmount,
    decimal DocumentsAmount,
    decimal AdvanceAmount,
    decimal PaidAmount,
    EstadoOrdenRecaudo Status,
    string? Notes);

public sealed record ProcedureDto(
    Guid Id,
    string Number,
    Guid CreditApplicationId,
    string CreditApplicationNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerMobile,
    Guid ProductId,
    string ProductName,
    Guid? SalesPointId,
    string? SalesPointName,
    TipoTramite Type,
    EstadoTramite Status,
    DateTime StartDate,
    DateTime EstimatedDate,
    DateTime? CompletedAt,
    string? Responsible,
    string? ThirdParty,
    bool NotifyCustomer,
    DateTime? CustomerNotifiedAt,
    bool IsOverdue,
    string WhatsappMessage,
    string? Notes);

public sealed record UpsertProcedureDto(
    Guid CreditApplicationId,
    Guid? SalesPointId,
    TipoTramite Type,
    EstadoTramite Status,
    DateTime StartDate,
    DateTime? EstimatedDate,
    DateTime? CompletedAt,
    string? Responsible,
    string? ThirdParty,
    bool NotifyCustomer,
    DateTime? CustomerNotifiedAt,
    string? Notes);

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

public sealed record LoginAccessReportDto(
    int TotalAccesses,
    int SuccessfulAccesses,
    int FailedAccesses,
    int TodayAccesses,
    DateTime? LastAccessAt,
    IReadOnlyCollection<LoginAccessDto> Items);

public sealed record LoginAccessDto(
    Guid Id,
    Guid? UserId,
    string UserName,
    string Login,
    string? Email,
    Guid CompanyId,
    string CompanyName,
    DateTime AccessedAt,
    bool Successful,
    string? FailureReason,
    string? IpAddress,
    string? UserAgent);
