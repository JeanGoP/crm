using System.Text;
using CrmSaas.Api.Services;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;

internal static class CompanyLogoChecks
{
    public static void Run(QuoteDto quote, string[] args)
    {
        static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
        var company = new Empresa { LogoDataUrl = "moto-logo", LogoElectrodomesticosDataUrl = "electro-logo" };
        var categories = new[]
        {
            new CategoriaProducto { Nombre = "Motos", CotizarComoPaquete = false },
            new CategoriaProducto { Nombre = "Línea hogar", CotizarComoPaquete = true }
        };
        Check(CompanyDocumentLogos.ForQuote(company, ["Motos"], categories, false) == ("moto-logo", null), "Cotización de moto usa su logo.");
        Check(CompanyDocumentLogos.ForQuote(company, ["LÍNEA HOGAR"], categories, false) == ("electro-logo", null), "Cotización de electrodomésticos usa su logo.");
        Check(CompanyDocumentLogos.ForQuote(company, ["Motos", "Línea hogar"], categories, false) == ("moto-logo", "electro-logo"), "Cotización mixta usa ambos logos.");
        Check(CompanyDocumentLogos.ForCreditApplication(company, false) == "moto-logo", "Solicitud de moto usa logo de motos.");
        Check(CompanyDocumentLogos.ForCreditApplication(company, true) == "electro-logo", "Solicitud de electrodomésticos usa logo de electrodomésticos.");
        company.LogoElectrodomesticosDataUrl = null;
        Check(CompanyDocumentLogos.ForCreditApplication(company, true) == "moto-logo", "Empresas existentes conservan el logo anterior mientras configuran el segundo.");

        if (args.Length > 1)
        {
            var image = new QuotePdfImage(File.ReadAllBytes(args[1]), "image/png", "logo.png");
            var mixedPdf = SimplePdfGenerator.Quote(quote, "Empresa de prueba", companyLogo: image, secondaryCompanyLogo: image);
            var raw = Encoding.ASCII.GetString(mixedPdf);
            Check(raw.Contains("/Logo ") && raw.Contains("/Logo2 ") && raw.Contains("/Logo2 Do"), "El PDF de cotización mixta dibuja ambos logos.");
            File.WriteAllBytes(Path.Combine(args[0], "cotizacion-mixta-logos.pdf"), mixedPdf);
        }
        Console.WriteLine("OK: logos por tipo de artículo, solicitudes de crédito y cotizaciones mixtas.");
    }
}
