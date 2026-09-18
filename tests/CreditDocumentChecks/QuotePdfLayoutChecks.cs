using System.Text;
using System.Text.RegularExpressions;
using CrmSaas.Api.Services;
using CrmSaas.Application.DTOs;

internal static class QuotePdfLayoutChecks
{
    public static void Run(QuoteDto basis, string[] args)
    {
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        static string Read(byte[] pdf) => string.Join("\n", Regex.Matches(Encoding.ASCII.GetString(pdf), @"<([0-9A-F]+)> Tj")
            .Select(m => Encoding.Latin1.GetString(Convert.FromHexString(m.Groups[1].Value))));
        static int PageCount(byte[] pdf) => Regex.Matches(Encoding.ASCII.GetString(pdf), @"/Type /Page\b").Count;
        var terms = new[] { 12, 18, 24 }.Select(t => new QuoteFinancingOptionDto(t, 5000000m / t, 6500000)).ToArray();
        var item = basis.Items.First() with { ProductName = "BOXER CT100 KS (SIN ENCENDIDO)", ProductPrice = 6500000,
            PromotionDiscount = 0, DiscountedProductPrice = 6500000, Insurance = 0, AdministrativeFees = 0,
            DownPayment = 1500000, InitialPaymentPaidToday = 1000000, InitialPaymentBalance = 0, FinancedAmount = 5000000,
            InitialPaymentSchedule = [new(new DateTime(2026, 10, 15), 500000)],
            CreditStartDate = new DateTime(2026, 10, 15), FinancingOptions = terms, TermMonths = 12, CreditType = "Manual" };
        var single = basis with { Number = "COT-20260917-001", ProductName = item.ProductName, IsBundle = false,
            CustomerFirstName = "MARÍA", CustomerMiddleName = "JOSÉ", CustomerLastName = "MUÑOZ", CustomerSecondLastName = "PÉREZ",
            IdentificationNumber = "1000000000", SalesPointName = "Sede principal", SalesPointRateName = "Tasa comercial",
            QuoteDate = new DateTime(2026, 9, 17), CreditType = "Manual", ProductPrice = 6500000, DiscountedProductPrice = 6500000,
            PromotionDiscount = 0, Insurance = 0, AdministrativeFees = 0,
            DownPayment = 1500000, InitialPaymentPaidToday = 1000000, FinancedAmount = 5000000, FinancingOptions = terms,
            InitialPaymentSchedule = item.InitialPaymentSchedule, CreditStartDate = item.CreditStartDate, Items = [item], Notes = null,
            SalesPointCommercialTerms = null };
        QuotePdfImage? logo = args.Length > 1 ? new(File.ReadAllBytes(args[1]), "image/png", "empresa.png") : null;
        byte[] Generate(QuoteDto q) => SimplePdfGenerator.Quote(q, "Moteros de la Sabana", companyLogo: logo,
            brandLogo: logo, productImage: logo, customerPhone: "300 000 0000", advisor: "Asesor comercial");
        var pdf = Generate(single);
        var text = Read(pdf);
        Check(text.Contains("MARÍA JOSÉ MUÑOZ PÉREZ") && text.Contains("COTIZACIÓN"), "Acentos preservados en PDF.");
        Check(text.Contains("$ 1.000.000") && text.Contains("$ 500.000") && text.Contains("$ 1.500.000") && text.Contains("$ 5.000.000"), "Inicial, extra, inicial completa y financiado sin recalcular.");
        Check(terms.All(t => text.Contains(t.TermMonths + " cuotas")), "Todas las alternativas aparecen.");
        Check(text.Contains("PROPIEDAD RAÍZ") && text.Contains("COMERCIANTE") && text.Contains("EMPLEADO") && text.Contains("DÍA EN QUE SE"), "Requisitos y vigencia diaria.");
        Check(!Encoding.ASCII.GetString(pdf).Contains("/BrandLogo") && !Encoding.ASCII.GetString(pdf).Contains("/Product"), "Solo logo de empresa.");
        Check(!text.Contains("SIGUENOS") && !text.Contains("ENCUESTA") && !text.Contains("cada una"), "Sin QR ficticios ni textos eliminados.");
        Check(PageCount(pdf) == 1, "Cotización habitual cabe en una página.");
        var cash = single with { CreditType = "Contado", IsBundle = true, Items = [item] };
        var cashPdf = Generate(cash);
        Check(!Read(cashPdf).Contains("Financiado") && !Read(cashPdf).Contains("cuotas") && !Read(cashPdf).Contains("REQUISITOS"), "Contado sin condiciones de crédito.");
        var comparative = single with { InitialPaymentSchedule = [], Items = Enumerable.Range(1, 4)
            .Select(i => item with { ProductName = "BOXER modelo " + i, Order = i, InitialPaymentSchedule = [], InitialPaymentPaidToday = 1500000 }).ToArray() };
        var comparativePdf = Generate(comparative);
        Check(PageCount(comparativePdf) == 1, "Comparativo de cuatro artículos sin extra cabe en una página.");
        Check(Enumerable.Range(1, 4).All(i => Read(comparativePdf).Contains("BOXER modelo " + i)), "Comparativo conserva todos los artículos.");
        var bundle = single with { IsBundle = true, Items = Enumerable.Range(1, 4).Select(i => item with {
            ProductName = "Electrodoméstico " + i, Order = i, ProductPrice = 1625000, DiscountedProductPrice = 1625000,
            DownPayment = 0, InitialPaymentPaidToday = 0, InitialPaymentSchedule = [], FinancingOptions = [] }).ToArray() };
        var bundlePdf = Generate(bundle);
        Check(Read(bundlePdf).Contains("Todos los artículos") && !Read(bundlePdf).Contains("paquete"), "Financiación global sin palabra paquete.");
        Check(Regex.Matches(Read(bundlePdf), "15/10/2026").Count == 2, "Un solo plan global y su fecha de inicio.");
        var many = single with { FinancingOptions = Enumerable.Range(1, 40).Select(t => new QuoteFinancingOptionDto(t, 5000000m/t, 6500000)).ToArray(),
            Notes = string.Join(" ", Enumerable.Repeat("Observación extensa para comprobar saltos de página.", 100)),
            Items = Enumerable.Range(1, 25).Select(i => item with { ProductName = "Artículo extenso de prueba " + i, Order = i }).ToArray(), IsBundle = true };
        var manyPdf = Generate(many);
        Check(Enumerable.Range(1, 40).All(t => Read(manyPdf).Contains(t + " cuotas")), "Cuarenta plazos sin recortes.");
        Check(Read(manyPdf).Contains("Artículo extenso de prueba 25") && PageCount(manyPdf) > 1, "Paginación de todos los artículos.");
        var legacy = single with { Items = [], FinancingOptions = null, TermMonths = 18, EstimatedMonthlyPayment = 321000 };
        Check(Read(Generate(legacy)).Contains("$ 321.000"), "Cotizaciones antiguas usan cuota guardada, no una simulación nueva.");
        var different = comparative with { Items = [item with { FinancingOptions = [terms[0]] }, item with { FinancingOptions = [terms[2]], Order = 2 }] };
        Check(Read(Generate(different)).Contains("-"), "Plazos no elegidos por un artículo no se inventan.");
        if (args.Length > 0)
        {
            foreach (var sample in new[] { ("layout-single", pdf), ("layout-cash", cashPdf), ("layout-comparative", comparativePdf), ("layout-global", bundlePdf), ("layout-overflow", manyPdf) })
                File.WriteAllBytes(Path.Combine(args[0], sample.Item1 + ".pdf"), sample.Item2);
        }
        Console.WriteLine("OK: nuevo PDF, logo único, acentos, contado, comparativo, inicial global, extra, históricos y paginación.");
    }
}
