using CrmSaas.Domain.Entities;
using FluentValidation;

namespace CrmSaas.Api.Services;

public static class CreditPhoneValidation
{
    // National numbers are Colombian. Do not truncate foreign country codes or
    // arbitrary leading digits: that would merge genuinely different numbers.
    public static string Normalize(string? value)
    {
        var digits = new string((value ?? "").Where(c => c is >= '0' and <= '9').ToArray());
        if (digits.StartsWith("00", StringComparison.Ordinal)) digits = digits[2..];
        if (digits.Length == 12 && digits.StartsWith("57", StringComparison.Ordinal)) digits = digits[2..];
        return digits;
    }

    public sealed record Duplicate(string Phone, IReadOnlyList<string> Fields);

    public static IReadOnlyList<Duplicate> FindDuplicates(SolicitudCredito application)
    {
        var fields = new List<(string Label, string? Phone)>
        {
            ("Cliente - celular", application.Celular),
            ("Cliente - referencia 1", application.Referencia1Celular),
            ("Cliente - referencia 2", application.Referencia2Celular)
        };
        var index = 0;
        foreach (var person in application.Codeudores.Where(x => x.Activo).OrderBy(x => x.Orden))
        {
            var label = $"Codeudor {++index} ({person.Nombre})";
            fields.Add(($"{label} - celular", person.Celular));
            fields.Add(($"{label} - referencia 1", person.Referencia1Celular));
            fields.Add(($"{label} - referencia 2", person.Referencia2Celular));
        }
        // The old primary-codebtor fields mirror the collection; never count both.
        if (application.Codeudores.Count == 0 && !string.IsNullOrWhiteSpace(application.CodeudorNombre))
        {
            fields.Add(("Codeudor 1 - celular", application.CodeudorCelular));
            fields.Add(("Codeudor 1 - referencia 1", application.CodeudorReferencia1Celular));
            fields.Add(("Codeudor 1 - referencia 2", application.CodeudorReferencia2Celular));
        }
        return fields.Select(x => (x.Label, Phone: Normalize(x.Phone)))
            .Where(x => x.Phone.Length > 0).GroupBy(x => x.Phone)
            .Where(x => x.Count() > 1)
            .Select(x => new Duplicate(x.Key, x.Select(f => f.Label).ToArray())).ToArray();
    }

    public static void Validate(SolicitudCredito application)
    {
        var duplicates = FindDuplicates(application);
        if (duplicates.Count > 0)
            throw new ValidationException("No se puede guardar: hay teléfonos repetidos en la solicitud. " +
                string.Join("; ", duplicates.Select(x => $"{x.Phone}: {string.Join(", ", x.Fields)}")) + ". Corrija los campos indicados.");
    }
}
