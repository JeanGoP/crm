using System.Security.Cryptography;
using System.Text;
using CrmSaas.Infrastructure.Auth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace CrmSaas.Api.Services;

public sealed class CrmErpTokenService(IOptions<JwtOptions> options)
{
    public string Create(string method, string path, string bodyHash)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds();
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var data = $"{expires}|{nonce}|{method.ToUpperInvariant()}|{path}|{bodyHash}";
        var signature = Sign(data);
        return $"{WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(data))}.{WebEncoders.Base64UrlEncode(signature)}";
    }

    public bool Verify(string? token, string method, string path, string bodyHash)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 1024) return false;
        var parts = token.Split('.');
        if (parts.Length != 2) return false;
        try
        {
            var data = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(parts[0]));
            var signature = WebEncoders.Base64UrlDecode(parts[1]);
            if (signature.Length != 32 || !CryptographicOperations.FixedTimeEquals(signature, Sign(data))) return false;
            var fields = data.Split('|');
            if (fields.Length != 5 || !long.TryParse(fields[0], out var expires)) return false;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return expires >= now && expires <= now + 60 && fields[1].Length == 32
                && fields[2] == method.ToUpperInvariant() && fields[3] == path && fields[4] == bodyHash;
        }
        catch (Exception error) when (error is FormatException or ArgumentException) { return false; }
    }

    private byte[] Sign(string data)
    {
        var key = options.Value.SigningKey;
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException("La clave de autenticación del CRM no está configurada.");
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(data));
    }

    public static string HashBody(string body) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
}
