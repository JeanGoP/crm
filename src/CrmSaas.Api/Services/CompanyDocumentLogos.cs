using CrmSaas.Domain.Entities;

namespace CrmSaas.Api.Services;

public static class CompanyDocumentLogos
{
    public static (string? Primary, string? Secondary) ForQuote(
        Empresa? company, IEnumerable<string?> productCategories,
        IReadOnlyCollection<CategoriaProducto> categories, bool isBundle)
    {
        var applianceNames = categories.Where(x => x.CotizarComoPaquete)
            .Select(x => x.Nombre).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var kinds = productCategories.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => applianceNames.Contains(x!.Trim())).ToArray();
        var hasAppliances = kinds.Any(x => x) || isBundle;
        var hasMotorcycles = kinds.Any(x => !x) || !hasAppliances;
        var motorcycleLogo = company?.LogoDataUrl;
        var applianceLogo = company?.LogoElectrodomesticosDataUrl ?? motorcycleLogo;

        if (!hasMotorcycles) return (applianceLogo, null);
        if (!hasAppliances || string.Equals(motorcycleLogo, applianceLogo, StringComparison.Ordinal))
            return (motorcycleLogo, null);
        return (motorcycleLogo ?? applianceLogo, motorcycleLogo is null ? null : applianceLogo);
    }

    public static string? ForCreditApplication(Empresa? company, bool isAppliance) =>
        isAppliance ? company?.LogoElectrodomesticosDataUrl ?? company?.LogoDataUrl : company?.LogoDataUrl;
}
