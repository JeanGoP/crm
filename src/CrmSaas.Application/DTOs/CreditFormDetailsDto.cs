namespace CrmSaas.Application.DTOs;

// Snapshot of the additional information supplied for the signed credit form.
public sealed record CreditFormDetailsDto
{
    public DateTime? BirthDate { get; init; }
    public string? CompanyTaxId { get; init; }
    public string? CompanyLocation { get; init; }
    public decimal? AdvancePayment { get; init; }
    public string? PurchaseSupport { get; init; }
    public string? IdentificationType { get; init; }
    public string? HousingType { get; init; }
    public string? MaritalStatus { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Employer { get; init; }
    public string? JobTitle { get; init; }
    public string? Occupation { get; init; }
    public string? WorkPhone { get; init; }
    public string? WorkAddress { get; init; }
    public string? WorkEmail { get; init; }
    public string? LocationReference { get; init; }
    public string? Reference1Address { get; init; }
    public string? Reference2Address { get; init; }
    public string? Zone { get; init; }
    public string? Advisor { get; init; }
    public string? SalesPoint { get; init; }
    public string? BusinessType { get; init; }
    public string? VehicleType { get; init; }
    public string? VehicleLine { get; init; }
    public string? VehicleVariant { get; init; }
    public string? VehicleModel { get; init; }
    public string? VehicleColor { get; init; }
    public string? VehicleEngineCc { get; init; }
    public string? VehiclePlate { get; init; }
    public string? VehicleChassis { get; init; }
    public string? VehicleBrand { get; init; }
    public string? VehicleEngine { get; init; }
    public string? VehicleNotes { get; init; }
    public decimal? ExtraPayment { get; init; }
    public int? ExtraPaymentCount { get; init; }
    public decimal? MonthlyPayment { get; init; }
    public decimal? Soat { get; init; }
    public decimal? Registration { get; init; }
    public decimal? TotalCredit { get; init; }
}
