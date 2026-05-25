using CrmSaas.Application.DTOs;
using FluentValidation;

namespace CrmSaas.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class UpsertCustomerValidator : AbstractValidator<UpsertCustomerDto>
{
    public UpsertCustomerValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80).When(x => string.IsNullOrWhiteSpace(x.FirstNames));
        RuleFor(x => x.FirstNames).NotEmpty().MaximumLength(120).When(x => string.IsNullOrWhiteSpace(x.FirstName));
        RuleFor(x => x.MiddleName).MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80).When(x => string.IsNullOrWhiteSpace(x.LastNames));
        RuleFor(x => x.LastNames).NotEmpty().MaximumLength(120).When(x => string.IsNullOrWhiteSpace(x.LastName));
        RuleFor(x => x.SecondLastName).MaximumLength(80);
        RuleFor(x => x.Email).EmailAddress().MaximumLength(180).When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.IdentificationNumber).MaximumLength(40);
        RuleFor(x => x.Phone).MaximumLength(60);
    }
}

public sealed class UpsertLeadValidator : AbstractValidator<UpsertLeadDto>
{
    public UpsertLeadValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80).When(x => string.IsNullOrWhiteSpace(x.FirstNames));
        RuleFor(x => x.FirstNames).NotEmpty().MaximumLength(120).When(x => string.IsNullOrWhiteSpace(x.FirstName));
        RuleFor(x => x.MiddleName).MaximumLength(80);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80).When(x => string.IsNullOrWhiteSpace(x.LastNames));
        RuleFor(x => x.LastNames).NotEmpty().MaximumLength(120).When(x => string.IsNullOrWhiteSpace(x.LastName));
        RuleFor(x => x.SecondLastName).MaximumLength(80);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(180);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(120);
    }
}

public sealed class UpsertDealValidator : AbstractValidator<UpsertDealDto>
{
    public UpsertDealValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.StageId).NotEmpty();
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CloseProbability).InclusiveBetween(0, 100);
    }
}

public sealed class UpsertActivityValidator : AbstractValidator<UpsertActivityDto>
{
    public UpsertActivityValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(180);
        RuleFor(x => x.ScheduledAt).NotEmpty();
    }
}
