using System.Net;

namespace CrmSaas.Api.Services;

public static class ErpLoopback
{
    public static (Uri BaseUri, string Host) Resolve(IConfiguration configuration)
    {
        var url = configuration["ErpIntegration:BaseUrl"] ?? "http://127.0.0.1/ERP/";
        var host = configuration["ErpIntegration:HostHeader"] ?? "stecno.dyndns.org";
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttp || !uri.AbsolutePath.Equals("/ERP/", StringComparison.OrdinalIgnoreCase)
            || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0
            || !IPAddress.TryParse(uri.Host, out var address) || !IPAddress.IsLoopback(address)
            || string.IsNullOrWhiteSpace(host) || host.Contains('/') || host.Contains('\\'))
            throw new InvalidOperationException("La dirección interna del ERP no es válida.");
        return (uri, host);
    }
}
