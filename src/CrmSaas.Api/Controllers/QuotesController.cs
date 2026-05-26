using CrmSaas.Api.Services;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Common;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quotes")]
public sealed class QuotesController(CrmDbContext db, ITenantContext tenantContext) : ControllerBase
{
    private const string AutomaticFollowUpTitle = "Llamar al cliente mañana";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<QuoteDto>>> Get(CancellationToken cancellationToken)
    {
        var quotes = await db.Cotizaciones
            .Include(x => x.Producto)
            .OrderByDescending(x => x.FechaCotizacion)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
        return Ok(quotes);
    }

    [HttpPost]
    public async Task<ActionResult<QuoteDto>> Create(CreateQuoteDto dto, CancellationToken cancellationToken)
    {
        var firstName = Clean(dto.CustomerFirstName) ?? Split(dto.CustomerFirstNames).ElementAtOrDefault(0) ?? string.Empty;
        var middleName = Clean(dto.CustomerMiddleName) ?? Join(Split(dto.CustomerFirstNames).Skip(1));
        var lastName = Clean(dto.CustomerLastName) ?? Split(dto.CustomerLastNames).ElementAtOrDefault(0) ?? string.Empty;
        var secondLastName = Clean(dto.CustomerSecondLastName) ?? Join(Split(dto.CustomerLastNames).Skip(1));
        var firstNames = Join(firstName, middleName);
        var lastNames = Join(lastName, secondLastName);

        if (string.IsNullOrWhiteSpace(firstName)) throw new ValidationException("El primer nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ValidationException("El primer apellido del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber)) throw new ValidationException("El telefono del cliente es obligatorio.");
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar un producto para cotizar.");
        if (dto.DownPayment < 0) throw new ValidationException("La cuota inicial no puede ser negativa.");
        if (dto.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");
        if (dto.MonthlyInterestRate < 0) throw new ValidationException("La tasa mensual no puede ser negativa.");

        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado o inactivo.");
        var initialStage = await db.EtapasNegocio
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No hay etapas activas en el pipeline para crear la oportunidad.");

        var fullName = $"{firstNames} {lastNames}".Trim();
        var phone = FormatPhone(dto.PhoneCountryCode, dto.PhoneNumber);
        var productName = ProductName(product);
        var simulation = CalculateSimulation(product.Precio, dto.DownPayment, dto.TermMonths, dto.MonthlyInterestRate);
        var customer = new Cliente
        {
            Nombre = firstNames,
            Nombres = firstNames,
            Apellidos = lastNames,
            PrimerNombre = firstName,
            SegundoNombre = middleName,
            PrimerApellido = lastName,
            SegundoApellido = secondLastName,
            TipoIdentificacion = dto.IdentificationType,
            NumeroIdentificacion = dto.IdentificationNumber,
            Email = string.Empty,
            IndicativoTelefono = string.IsNullOrWhiteSpace(dto.PhoneCountryCode) ? "+57" : dto.PhoneCountryCode.Trim(),
            Telefono = phone,
            Estado = EstadoCliente.Activo,
            Etiquetas = "cotizacion"
        };
        db.Clientes.Add(customer);

        var now = ColombiaTime.Now;
        var number = $"COT-{now:yyyyMMddHHmmss}";
        var quote = new Cotizacion
        {
            Numero = number,
            TipoIdentificacion = dto.IdentificationType,
            NumeroIdentificacion = dto.IdentificationNumber,
            NombresCliente = firstNames,
            ApellidosCliente = lastNames,
            PrimerNombreCliente = firstName,
            SegundoNombreCliente = middleName,
            PrimerApellidoCliente = lastName,
            SegundoApellidoCliente = secondLastName,
            ClienteId = customer.Id,
            ProductoId = product.Id,
            PrecioProducto = product.Precio,
            CuotaInicial = simulation.DownPayment,
            PlazoMeses = dto.TermMonths,
            TasaInteresMensual = dto.MonthlyInterestRate,
            ValorFinanciado = simulation.FinancedAmount,
            CuotaMensualEstimada = simulation.MonthlyPayment,
            TotalPagarEstimado = simulation.TotalPayment,
            FechaCotizacion = now,
            ValidaHasta = now.AddDays(7),
            Observaciones = dto.Notes
        };
        var deal = new Negocio
        {
            Titulo = $"{fullName} - {productName}",
            ClienteId = customer.Id,
            EtapaNegocioId = initialStage.Id,
            Valor = product.Precio,
            ProbabilidadCierre = initialStage.ProbabilidadPredeterminada,
            FechaEstimadaCierre = now.AddDays(15),
            Estado = EstadoNegocio.Abierto
        };
        var followUp = new Actividad
        {
            Titulo = AutomaticFollowUpTitle,
            Descripcion = $"Cotizacion {number}: contactar a {fullName} para resolver dudas y avanzar la venta de {productName}.",
            Tipo = TipoActividad.Llamada,
            Estado = EstadoActividad.Pendiente,
            FechaProgramada = now.AddDays(1),
            RecordatorioEn = now.AddHours(20),
            ClienteId = customer.Id,
            NegocioId = deal.Id
        };

        db.Cotizaciones.Add(quote);
        db.Negocios.Add(deal);
        db.Actividades.Add(followUp);
        await db.SaveChangesAsync(cancellationToken);
        quote.Producto = product;

        return Ok(ToDto(quote));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var quote = await db.Cotizaciones.Include(x => x.Producto).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cotizacion no encontrada.");
        var company = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantContext.EmpresaId, cancellationToken);
        var dto = ToDto(quote);
        var bytes = SimplePdfGenerator.Quote(dto, company?.Nombre ?? "Empresa");
        return File(bytes, "application/pdf", $"{quote.Numero}.pdf");
    }

    private static QuoteDto ToDto(Cotizacion x)
    {
        var productName = x.Producto is null
            ? "Producto"
            : ProductName(x.Producto);
        var termMonths = x.PlazoMeses <= 0 ? 24 : x.PlazoMeses;
        var financedAmount = x.ValorFinanciado <= 0 && x.CuotaMensualEstimada <= 0 ? Math.Max(x.PrecioProducto - x.CuotaInicial, 0) : x.ValorFinanciado;
        var totalPayment = x.TotalPagarEstimado <= 0 ? x.PrecioProducto : x.TotalPagarEstimado;
        return new QuoteDto(
            x.Id,
            x.Numero,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.NombresCliente,
            x.ApellidosCliente,
            x.PrimerNombreCliente,
            x.SegundoNombreCliente,
            x.PrimerApellidoCliente,
            x.SegundoApellidoCliente,
            x.ClienteId,
            x.ProductoId,
            productName,
            x.PrecioProducto,
            x.CuotaInicial,
            termMonths,
            x.TasaInteresMensual,
            financedAmount,
            x.CuotaMensualEstimada,
            totalPayment,
            x.FechaCotizacion,
            x.ValidaHasta,
            x.Observaciones);
    }

    private static CreditSimulation CalculateSimulation(decimal productPrice, decimal downPayment, int termMonths, decimal monthlyInterestRate)
    {
        var normalizedDownPayment = Math.Min(downPayment, productPrice);
        var financedAmount = Math.Max(productPrice - normalizedDownPayment, 0);
        var monthlyRate = monthlyInterestRate / 100;
        var monthlyPayment = financedAmount == 0
            ? 0
            : monthlyRate == 0
                ? financedAmount / termMonths
                : financedAmount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -termMonths));
        monthlyPayment = Math.Round(monthlyPayment, 0, MidpointRounding.AwayFromZero);
        var totalPayment = normalizedDownPayment + (monthlyPayment * termMonths);
        return new CreditSimulation(normalizedDownPayment, financedAmount, monthlyPayment, totalPayment);
    }

    private sealed record CreditSimulation(decimal DownPayment, decimal FinancedAmount, decimal MonthlyPayment, decimal TotalPayment);

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static string FormatPhone(string? countryCode, string? phoneNumber)
    {
        var normalizedCode = string.IsNullOrWhiteSpace(countryCode) ? "+57" : countryCode.Trim();
        if (!normalizedCode.StartsWith("+")) normalizedCode = "+" + normalizedCode;
        var normalizedNumber = new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        return $"{normalizedCode} {normalizedNumber}".Trim();
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static IReadOnlyList<string> Split(string? value) => (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private static string Join(params string?[] values) => string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
    private static string? Join(IEnumerable<string> values)
    {
        var joined = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}
