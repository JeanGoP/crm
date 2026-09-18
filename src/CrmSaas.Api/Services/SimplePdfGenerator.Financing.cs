using System.Text;
using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static partial class SimplePdfGenerator
{
    private static IEnumerable<string> FinancingPages(QuoteDto quote)
    {
        if (quote.CreditType == "Contado") yield break;
        var groups = quote.IsBundle || quote.Items.Count <= 1
            ? new[] { (Name: quote.IsBundle ? "Paquete completo" : quote.ProductName, Initial: quote.DownPayment, Financed: quote.FinancedAmount, Options: quote.FinancingOptions) }
            : quote.Items.OrderBy(x => x.Order).Select(x => (Name: x.ProductName, Initial: x.DownPayment, Financed: x.FinancedAmount, Options: x.FinancingOptions));
        foreach (var group in groups)
        {
            if (group.Options is not { Count: > 0 } || (group.Options.Count == 1 && (quote.IsBundle || quote.Items.Count <= 1))) continue;
            foreach (var chunk in group.Options.OrderBy(x => x.TermMonths).Chunk(20))
            {
                var commands = new StringBuilder();
                DrawPanel(commands, 40, 50, 532, 700, "ALTERNATIVAS DE FINANCIACION");
                KeyValue(commands, 56, 705, "Cotizacion", quote.Number, 100, 40);
                commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 11 Tf 56 679 Td ({Escape(Shorten(group.Name, 65))}) Tj ET");
                KeyValue(commands, 56, 653, "Inicial completa", Money(group.Initial), 120, 30);
                KeyValue(commands, 315, 653, "Financiado", Money(group.Financed), 100, 25);
                commands.AppendLine("0.90 0.94 0.96 rg 55 602 502 28 re f");
                commands.AppendLine("0.08 0.10 0.14 rg BT /F2 10 Tf 68 612 Td (Numero de cuotas) Tj ET");
                commands.AppendLine("0.08 0.10 0.14 rg BT /F2 10 Tf 290 612 Td (Valor de cada cuota) Tj ET");
                var row = 584;
                foreach (var option in chunk)
                {
                    commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 11 Tf 68 {row} Td ({option.TermMonths} cuotas) Tj ET");
                    commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 11 Tf 290 {row} Td ({Escape(Money(option.MonthlyPayment))}) Tj ET");
                    row -= 24;
                }
                commands.AppendLine($"0.36 0.42 0.48 rg BT /F1 9 Tf 56 82 Td ({Escape("Tasa: " + Shorten(Value(quote.SalesPointRateName), 60))}) Tj ET");
                commands.AppendLine("0.36 0.42 0.48 rg BT /F1 9 Tf 56 64 Td (Son alternativas: el cliente elige un solo plazo para su credito.) Tj ET");
                yield return commands.ToString();
            }
        }
    }
}
