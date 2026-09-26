using System.Globalization;
using System.Text;
using CrmSaas.Domain.Entities;

namespace CrmSaas.Api.Services;

public sealed record CreditPurchaseLine(string Name, string Code, decimal Value, int Quantity = 1);

// Read-only print data. Never changes the application's approved/requested conditions.
public sealed record CreditPrintContext(bool IsAppliance, IReadOnlyList<CreditPurchaseLine> Items,
    decimal? Advance = null, decimal? MonthlyPayment = null)
{
    public static bool IsApplianceCategory(string? category) => new string((category ?? "").Normalize(NormalizationForm.FormD)
        .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray())
        .Contains("electrodom", StringComparison.OrdinalIgnoreCase);

    public static CreditPrintContext From(SolicitudCredito application, Cotizacion? quote)
    {
        var product = application.Producto;
        var appliance = IsApplianceCategory(product?.Categoria);
        // A linked quote may only contribute data for the same tenant, customer and chosen product.
        var valid = quote is not null && quote.EmpresaId == application.EmpresaId && quote.ClienteId == application.ClienteId
            && quote.Id == application.CotizacionId
            && (quote.ProductoId == application.ProductoId || quote.Items.Any(i => i.ProductoId == application.ProductoId));
        if (!valid) quote = null;
        var selected = quote?.Items.OrderBy(i => i.Orden).FirstOrDefault(i => i.ProductoId == application.ProductoId);
        var lines = quote?.EsPaquete == true && quote.Items.Count > 0
            ? quote.Items.OrderBy(i => i.Orden).ThenBy(i => i.Id)
                .Select(i => new CreditPurchaseLine(i.Producto?.Nombre ?? "Artículo", i.Producto?.Referencia ?? "", i.PrecioProducto)).ToArray()
            : new[] { new CreditPurchaseLine(product?.Nombre ?? "Artículo", product?.Referencia ?? "", application.ValorMoto) };
        decimal? advance = null, payment = null;
        if (quote is not null)
        {
            var aggregate = quote.EsPaquete || selected is null;
            var price = aggregate ? quote.PrecioProducto : selected!.PrecioProducto;
            var initial = aggregate ? quote.CuotaInicial : selected!.CuotaInicial;
            if (application.CuotaInicial == initial) advance = aggregate ? quote.CuotaInicialPagadaHoy : selected!.CuotaInicialPagadaHoy;
            // Do not print an old estimate if the application changed its price or initial payment.
            if (application.ValorMoto == price && application.CuotaInicial == initial)
            {
                var options = aggregate
                    ? QuoteFinancingOptions.Read(quote.AlternativasPlazoJson, quote.TipoCredito, quote.PlazoMeses, quote.CuotaMensualEstimada, quote.TotalPagarEstimado)
                    : QuoteFinancingOptions.Read(selected!.AlternativasPlazoJson, selected.TipoCredito, selected.PlazoMeses, selected.CuotaMensualEstimada, selected.TotalPagarEstimado);
                payment = options.FirstOrDefault(o => o.TermMonths == application.PlazoMeses)?.MonthlyPayment;
            }
        }
        return new(appliance, lines, advance, payment);
    }
}
