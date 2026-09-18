using System.Text.Json;
using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static class QuoteFinancingOptions
{
    public static IReadOnlyCollection<QuoteFinancingOptionDto> Read(string? json, string? creditType, int term, decimal payment, decimal total)
    {
        if (creditType == "Contado") return [];
        if (!string.IsNullOrWhiteSpace(json))
        {
            var options = JsonSerializer.Deserialize<List<QuoteFinancingOptionDto>>(json);
            if (options is { Count: > 0 }) return options;
        }
        return term > 0 ? [new(term, payment, total)] : [];
    }
}
