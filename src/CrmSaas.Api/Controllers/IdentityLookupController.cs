using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrmSaas.Application.DTOs;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/identity")]
public sealed class IdentityLookupController(IHttpClientFactory httpClientFactory, IConfiguration configuration, CrmDbContext db) : ControllerBase
{
    private const string DefaultBaseUrl = "https://api.verifik.co";

    [HttpGet("colombia/cedula")]
    public async Task<ActionResult<ColombianIdentityLookupDto>> ColombianCedula([FromQuery] string documentNumber, CancellationToken cancellationToken)
    {
        var digits = new string((documentNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length is < 5 or > 10)
        {
            throw new ValidationException("La cedula debe tener entre 5 y 10 digitos.");
        }

        var existingCustomer = await db.Clientes
            .IgnoreQueryFilters()
            .Where(x => x.NumeroIdentificacion != null)
            .Where(x => x.NumeroIdentificacion!
                .Replace(".", "")
                .Replace("-", "")
                .Replace(" ", "") == digits)
            .OrderByDescending(x => x.FechaActualizacion ?? x.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingCustomer is not null)
        {
            var firstName = Clean(existingCustomer.PrimerNombre) ?? Split(existingCustomer.Nombres).ElementAtOrDefault(0);
            var middleName = Clean(existingCustomer.SegundoNombre) ?? Join(Split(existingCustomer.Nombres).Skip(1));
            var lastName = Clean(existingCustomer.PrimerApellido) ?? Split(existingCustomer.Apellidos).ElementAtOrDefault(0);
            var secondLastName = Clean(existingCustomer.SegundoApellido) ?? Join(Split(existingCustomer.Apellidos).Skip(1));
            var fullName = $"{Join(firstName, middleName)} {Join(lastName, secondLastName)}".Trim();

            return Ok(new ColombianIdentityLookupDto(
                existingCustomer.NumeroIdentificacion ?? digits,
                existingCustomer.TipoIdentificacion?.ToString(),
                firstName,
                middleName,
                lastName,
                secondLastName,
                string.IsNullOrWhiteSpace(fullName) ? existingCustomer.Nombre : fullName,
                existingCustomer.FechaNacimiento,
                null,
                null,
                null,
                null,
                null,
                "database"));
        }

        var token = configuration["Verifik:Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ValidationException("Verifik no esta configurado. Agrega la variable de entorno Verifik__Token para consultar identidades.");
        }

        var client = httpClientFactory.CreateClient();
        var baseUrl = (configuration["Verifik:BaseUrl"] ?? DefaultBaseUrl).TrimEnd('/');
        var endpoint = (configuration["Verifik:ColombianCedulaEndpoint"] ?? "basic").Trim().ToLowerInvariant();
        var content = endpoint == "premium"
            ? await SendPremiumWithBasicFallbackAsync(client, baseUrl, token, digits, cancellationToken)
            : await SendBasicAsync(client, baseUrl, token, digits, cancellationToken);

        if (content.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException("Verifik no encontro informacion para esta cedula.");
        }

        if (!content.IsSuccessStatusCode)
        {
            var error = TryDeserialize<VerifikErrorResponse>(content.Body);
            var message = string.IsNullOrWhiteSpace(error?.Message)
                ? "No se pudo consultar Verifik en este momento."
                : error.Message;
            throw new ValidationException($"Verifik respondio: {message}");
        }

        var result = TryDeserialize<VerifikIdentityResponse>(content.Body)
            ?? throw new InvalidOperationException("Verifik respondio con un formato no reconocido.");

        if (result.Data is null)
        {
            throw new KeyNotFoundException("Verifik no retorno datos de identidad para esta cedula.");
        }

        var names = NameParts.FromVerifik(result.Data);
        return Ok(new ColombianIdentityLookupDto(
            result.Data.DocumentNumber ?? digits,
            result.Data.DocumentType,
            names.FirstName,
            names.MiddleName,
            names.LastName,
            names.SecondLastName,
            result.Data.FullName,
            result.Data.DateOfBirth,
            result.Data.ExpeditionDate,
            result.Data.ExpeditionPlace?.Municipio,
            result.Data.ExpeditionPlace?.Departamento,
            result.Data.Gender,
            result.Data.IsAlive,
            "verifik"));
    }

    private static T? TryDeserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static async Task<VerifikHttpResult> SendPremiumWithBasicFallbackAsync(HttpClient client, string baseUrl, string token, string digits, CancellationToken cancellationToken)
    {
        var premium = await SendAsync(client, $"{baseUrl}/v2/co/cedula/premium?documentNumber={Uri.EscapeDataString(digits)}", token, cancellationToken);
        if ((int)premium.StatusCode < 500)
        {
            return premium;
        }

        return await SendBasicAsync(client, baseUrl, token, digits, cancellationToken);
    }

    private static Task<VerifikHttpResult> SendBasicAsync(HttpClient client, string baseUrl, string token, string digits, CancellationToken cancellationToken)
    {
        var url = $"{baseUrl}/v2/co/cedula?documentType=CC&documentNumber={Uri.EscapeDataString(digits)}";
        return SendAsync(client, url, token, cancellationToken);
    }

    private static async Task<VerifikHttpResult> SendAsync(HttpClient client, string url, string token, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new VerifikHttpResult(response.StatusCode, response.IsSuccessStatusCode, body);
    }

    private sealed record VerifikHttpResult(HttpStatusCode StatusCode, bool IsSuccessStatusCode, string Body);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IReadOnlyList<string> Split(string? value) => (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private static string Join(params string?[] values) => string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
    private static string? Join(IEnumerable<string> values)
    {
        var joined = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }

    private sealed class VerifikIdentityResponse
    {
        [JsonPropertyName("data")]
        public VerifikIdentityData? Data { get; set; }
    }

    private sealed class VerifikIdentityData
    {
        public string? DocumentNumber { get; set; }
        public string? DocumentType { get; set; }
        public string? FirstName { get; set; }
        public string? PrimerNombre { get; set; }
        public string? MiddleName { get; set; }
        public string? SecondName { get; set; }
        public string? SegundoNombre { get; set; }
        public string? LastName { get; set; }
        public string? PrimerApellido { get; set; }
        public string? SecondLastName { get; set; }
        public string? SegundoApellido { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? ExpeditionDate { get; set; }
        public VerifikExpeditionPlace? ExpeditionPlace { get; set; }
        public string? Gender { get; set; }
        public bool? IsAlive { get; set; }
    }

    private sealed class VerifikExpeditionPlace
    {
        public string? Municipio { get; set; }
        public string? Departamento { get; set; }
    }

    private sealed class VerifikErrorResponse
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }

    private sealed record NameParts(string? FirstName, string? MiddleName, string? LastName, string? SecondLastName)
    {
        public static NameParts FromVerifik(VerifikIdentityData data)
        {
            var firstName = Clean(data.PrimerNombre) ?? Clean(data.FirstName);
            var middleName = Clean(data.SegundoNombre) ?? Clean(data.SecondName) ?? Clean(data.MiddleName);
            var lastName = Clean(data.PrimerApellido) ?? Clean(data.LastName);
            var secondLastName = Clean(data.SegundoApellido) ?? Clean(data.SecondLastName);

            if (string.IsNullOrWhiteSpace(middleName) && !string.IsNullOrWhiteSpace(firstName))
            {
                var firstParts = Split(firstName);
                firstName = firstParts.ElementAtOrDefault(0);
                middleName = Join(firstParts.Skip(1));
            }

            if (string.IsNullOrWhiteSpace(secondLastName) && !string.IsNullOrWhiteSpace(lastName))
            {
                var lastParts = Split(lastName);
                lastName = lastParts.ElementAtOrDefault(0);
                secondLastName = Join(lastParts.Skip(1));
            }

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                var fullNameParts = Split(data.FullName);
                firstName ??= fullNameParts.ElementAtOrDefault(0);
                middleName ??= fullNameParts.Count > 3 ? string.Join(" ", fullNameParts.Skip(1).Take(fullNameParts.Count - 3)) : null;
                lastName ??= fullNameParts.Count > 1 ? fullNameParts.ElementAtOrDefault(fullNameParts.Count - 2) : null;
                secondLastName ??= fullNameParts.Count > 2 ? fullNameParts.ElementAtOrDefault(fullNameParts.Count - 1) : null;
            }

            return new NameParts(firstName, middleName, lastName, secondLastName);
        }

    }
}
