using System.Text.Json;
using CrmSaas.Application.DTOs;
using FluentValidation;

namespace CrmSaas.Api.Services;

public static class CreditFormDetails
{
    public static CreditFormDetailsDto? Read(string? json) => string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CreditFormDetailsDto>(json);

    public static string? Save(CreditFormDetailsDto? value, string? existing = null)
    {
        // Older callers must not erase information they do not understand.
        if (value is null) return existing;
        foreach (var property in typeof(CreditFormDetailsDto).GetProperties())
        {
            var field = property.GetValue(value);
            if (field is string text && text.Length > 500)
                throw new ValidationException("Los campos adicionales admiten máximo 500 caracteres.");
            if (field is decimal amount && amount < 0 || field is int count && count < 0)
                throw new ValidationException("Los valores de venta adicionales no pueden ser negativos.");
        }
        return JsonSerializer.Serialize(value);
    }
}
