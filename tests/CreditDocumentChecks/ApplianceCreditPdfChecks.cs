using System.Text;
using System.Text.RegularExpressions;
using CrmSaas.Api.Services;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;

static class ApplianceCreditPdfChecks
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static string Decode(byte[] bytes) => string.Join("\n", Regex.Matches(Encoding.ASCII.GetString(bytes), @"<([0-9A-F]+)> Tj")
        .Select(m => Encoding.Latin1.GetString(Convert.FromHexString(m.Groups[1].Value))));

    public static void Run(CreditApplicationDto original, string[] args)
    {
        Check(CreditPrintContext.IsApplianceCategory("Electrodomésticos") && CreditPrintContext.IsApplianceCategory("ELECTRODOMESTICOS"), "Accent/case category detection.");
        Check(!CreditPrintContext.IsApplianceCategory("Moto") && !CreditPrintContext.IsApplianceCategory(null), "Do not change vehicle format.");
        var product = new Producto { Nombre = "NEVERA DE DEMOSTRACIÓN 250 L", Referencia = "NEV-250", Categoria = "Electrodomésticos" };
        var washer = new Producto { Nombre = "LAVADORA DE DEMOSTRACIÓN 12 KG", Referencia = "LAV-012", Categoria = "Electrodomésticos" };
        var entity = new SolicitudCredito { EmpresaId = Guid.NewGuid(), ClienteId = Guid.NewGuid(), Producto = product, ProductoId = product.Id,
            ValorMoto = 2900000, CuotaInicial = 500000, PlazoMeses = 18 };
        var quote = new Cotizacion { EmpresaId = entity.EmpresaId, ClienteId = entity.ClienteId, ProductoId = product.Id, EsPaquete = true,
            PrecioProducto = 2900000, CuotaInicial = 500000, CuotaInicialPagadaHoy = 300000, PlazoMeses = 18, CuotaMensualEstimada = 180000, TipoCredito = "Crédito" };
        quote.Items.Add(new() { ProductoId = product.Id, Producto = product, PrecioProducto = 1700000, Orden = 0 });
        quote.Items.Add(new() { ProductoId = washer.Id, Producto = washer, PrecioProducto = 1200000, Orden = 1 });
        entity.CotizacionId = quote.Id;
        var context = CreditPrintContext.From(entity, quote);
        Check(context.IsAppliance && context.Items.Count == 2 && context.Items.Sum(i => i.Value) == 2900000, "Bundle includes all items once.");
        Check(context.Advance == 300000 && context.MonthlyPayment == 180000, "Use matching quotation conditions.");
        entity.PlazoMeses = 12;
        Check(CreditPrintContext.From(entity, quote).MonthlyPayment is null, "Never use a different term's installment.");
        entity.PlazoMeses = 18;
        quote.EsPaquete = false;
        Check(CreditPrintContext.From(entity, quote).Items.Count == 1, "Comparative quote prints only the chosen product.");
        quote.EsPaquete = true;
        quote.EmpresaId = Guid.NewGuid();
        Check(CreditPrintContext.From(entity, quote).Items.Count == 1 && CreditPrintContext.From(entity, quote).Advance is null, "Reject cross-company data.");
        quote.EmpresaId = entity.EmpresaId;
        quote.ClienteId = Guid.NewGuid();
        Check(CreditPrintContext.From(entity, quote).Items.Count == 1, "Reject unrelated customer.");
        var details = original.FormDetails! with { CompanyTaxId = "NIT DE PRUEBA", CompanyLocation = "Montelíbano - Córdoba", PurchaseSupport = "Recibo de demostración 001",
            AdvancePayment = 300000, MonthlyPayment = 180000, BusinessType = "Crédito directo" };
        var person = original.CoDebtors!.First();
        var sample = original with { Number = "SOL-ELECTRO-DEMO", CreatedAt = new(2026, 9, 25), MotorcycleValue = 2900000, DownPayment = 500000,
            ProductName = product.Nombre, TermMonths = 18, FormDetails = details,
            CoDebtors = [person with { FormDetails = person.FormDetails! with { BirthDate = new(1988, 2, 10) } }] };
        Check(CreditFormDetails.Read(CreditFormDetails.Save(details)) == details, "Appliance fields round-trip without a database migration.");
        var bytes = SimplePdfGenerator.CreditApplication(sample, "EMPRESA DE DEMOSTRACIÓN", "solicitud-credito", printContext: context);
        var text = Decode(bytes);
        Check(text.Contains("ANTICIPO\n$ 500.000"), "Anticipo comes from the application initial payment, not legacy details or quote advances.");
        var editedInitial = Decode(SimplePdfGenerator.CreditApplication(sample with { DownPayment = 750000 }, "Empresa", "solicitud-credito", printContext: context));
        Check(editedInitial.Contains("ANTICIPO\n$ 750.000"), "Editing initial payment updates the printed advance.");
        var zeroInitial = Decode(SimplePdfGenerator.CreditApplication(sample with { DownPayment = 0 }, "Empresa", "solicitud-credito", printContext: context));
        Check(zeroInitial.Contains("ANTICIPO\n$ 0"), "A zero initial payment must not fall back to an old advance.");
        var withoutPrintContext = Decode(SimplePdfGenerator.CreditApplication(sample, "Empresa", "solicitud-credito", printContext: context with { Advance = null }));
        Check(withoutPrintContext.Contains("ANTICIPO\n$ 500.000"), "Advance is available without quotation advance data.");
        foreach (var required in new[] { "ELECTRODOMÉSTICOS", "DATOS DEL DEUDOR", "DATOS DEL DEUDOR SOLIDARIO 1", "REFERENCIAS FAMILIARES", "DATOS DE LA COMPRA",
            "NEV-250", "LAV-012", "$ 2.900.000", "ANTICIPO", "18", "$ 180.000", "38", "Recibo de demostración 001", "FIRMAS Y HUELLAS" })
            Check(text.Contains(required), "Missing appliance PDF field: " + required);
        foreach (var forbidden in new[] { "CHASIS", "Matrícula", "SOAT", "Cilindraje", "CASANOVA", "900499748" })
            Check(!text.Contains(forbidden), "Do not copy vehicle fields or another company's data: " + forbidden);
        Check(Regex.Matches(Encoding.ASCII.GetString(bytes), @"/Type /Page /").Count == 2, "One co-debtor template fits two pages including signatures.");
        var withoutQuote = CreditPrintContext.From(entity, null);
        Check(withoutQuote.IsAppliance && withoutQuote.Items.Count == 1, "Appliance works without a quote.");
        var legacy = SimplePdfGenerator.CreditApplication(original, "Empresa", "solicitud-credito", printContext: context with { IsAppliance = false });
        Check(Decode(legacy).Contains("CHASIS-DEMO-0001"), "Vehicle format retained.");
        var stress = SimplePdfGenerator.CreditApplication(sample with { CoDebtors = Enumerable.Range(1, 4).Select(i => person with { Name = "DEUDOR SOLIDARIO " + i }).ToArray() },
            "Empresa", "solicitud-credito", printContext: context with { Items = Enumerable.Range(1, 22).Select(i => new CreditPurchaseLine("ARTÍCULO DE PRUEBA " + i + " " + new string('X', 130), "COD-" + i, 100000)).ToArray() });
        Check(Decode(stress).Contains("COD-22") && Decode(stress).Contains("CODEUDOR 4 / DEUDOR SOLIDARIO"), "No omitted articles/co-debtors during pagination.");
        if (args.Length > 0)
        {
            File.WriteAllBytes(Path.Combine(args[0], "solicitud-electrodomesticos-muestra.pdf"), bytes);
            File.WriteAllBytes(Path.Combine(args[0], "solicitud-electrodomesticos-extensa.pdf"), stress);
        }
        Console.WriteLine("OK: appliance-specific credit PDF, grouped purchase, category, terms, tenant/customer isolation, optional fields and pagination.");
    }
}
