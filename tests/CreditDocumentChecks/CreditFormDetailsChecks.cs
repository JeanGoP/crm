using CrmSaas.Api.Services;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using FluentValidation;

static class CreditFormDetailsChecks
{
    public static void Run()
    {
        var details = new CreditFormDetailsDto { HousingType = "Familiar", WorkPhone = "+57 300-123-4567", Reference1Address = "Calle 8", ExtraPayment = 500000, ExtraPaymentCount = 2, VehicleChassis = "TEST-CHASIS" };
        var json = CreditFormDetails.Save(details);
        if (CreditFormDetails.Read(json) != details) throw new Exception("Additional details must round-trip.");
        if (CreditFormDetails.Save(null, json) != json) throw new Exception("Older callers must preserve details.");
        var cleared = CreditFormDetails.Read(CreditFormDetails.Save(new(), json));
        if (cleared?.HousingType != null) throw new Exception("Explicit clearing must persist.");
        try { CreditFormDetails.Save(details with { ExtraPayment = -1 }); throw new Exception("Negative amount accepted."); } catch (ValidationException) { }
        try { CreditFormDetails.Save(details with { Employer = new string('x', 501) }); throw new Exception("Oversized input accepted."); } catch (ValidationException) { }
        var application = new SolicitudCredito { Celular = "3001234567", FormDetailsJson = json };
        if (CreditPhoneValidation.FindDuplicates(application).Count != 1) throw new Exception("Work phone must participate in duplicate checks.");
        application.FormDetailsJson = null;
        application.Codeudores.Add(new CodeudorSolicitudCredito { Activo = true, FormDetailsJson = json });
        if (CreditPhoneValidation.FindDuplicates(application).Count != 1) throw new Exception("Co-debtor work phone must participate in duplicate checks.");
        var dto = CreditCoDebtorService.ToDto(application.Codeudores.Single());
        if (dto.FormDetails != details) throw new Exception("Co-debtor DTO must expose saved details.");
        Console.WriteLine("OK: additional credit form fields, compatibility, limits and work phones.");
    }
}
