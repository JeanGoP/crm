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
        if (dto.Insurance < 0) throw new ValidationException("El seguro no puede ser negativo.");
        if (dto.AdministrativeFees < 0) throw new ValidationException("Los gastos administrativos no pueden ser negativos.");
        if (dto.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");
        if (dto.MonthlyInterestRate < 0) throw new ValidationException("La tasa mensual no puede ser negativa.");

        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado o inactivo.");
        var financialSettings = await GetFinancialSettingsAsync(cancellationToken);
        var initialStage = await db.EtapasNegocio
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No hay etapas activas en el pipeline para crear la oportunidad.");

        var fullName = $"{firstNames} {lastNames}".Trim();
        var phone = FormatPhone(dto.PhoneCountryCode, dto.PhoneNumber);
        var productName = ProductName(product);
        var simulation = CalculateSimulation(product.Precio, dto.DownPayment, dto.Insurance, dto.AdministrativeFees, dto.TermMonths, dto.MonthlyInterestRate, financialSettings);
        var normalizedIdentification = NormalizeIdentification(dto.IdentificationNumber);
        var customer = string.IsNullOrWhiteSpace(normalizedIdentification)
            ? null
            : await db.Clientes
                .Where(x => x.NumeroIdentificacion != null)
                .FirstOrDefaultAsync(x => x.NumeroIdentificacion!
                    .Replace(".", "")
                    .Replace("-", "")
                    .Replace(" ", "") == normalizedIdentification, cancellationToken);

        if (customer is null)
        {
            customer = new Cliente
            {
                Nombre = firstNames,
                Nombres = firstNames,
                Apellidos = lastNames,
                PrimerNombre = firstName,
                SegundoNombre = middleName,
                PrimerApellido = lastName,
                SegundoApellido = secondLastName,
                TipoIdentificacion = dto.IdentificationType,
                NumeroIdentificacion = normalizedIdentification ?? dto.IdentificationNumber,
                Email = string.Empty,
                IndicativoTelefono = string.IsNullOrWhiteSpace(dto.PhoneCountryCode) ? "+57" : dto.PhoneCountryCode.Trim(),
                Telefono = phone,
                Estado = EstadoCliente.Activo,
                Etiquetas = "cotizacion"
            };
            db.Clientes.Add(customer);
        }
        else
        {
            customer.Nombre = firstNames;
            customer.Nombres = firstNames;
            customer.Apellidos = lastNames;
            customer.PrimerNombre = firstName;
            customer.SegundoNombre = middleName;
            customer.PrimerApellido = lastName;
            customer.SegundoApellido = secondLastName;
            customer.TipoIdentificacion = dto.IdentificationType;
            customer.NumeroIdentificacion = normalizedIdentification ?? dto.IdentificationNumber;
            customer.IndicativoTelefono = string.IsNullOrWhiteSpace(dto.PhoneCountryCode) ? customer.IndicativoTelefono : dto.PhoneCountryCode.Trim();
            customer.Telefono = string.IsNullOrWhiteSpace(phone) ? customer.Telefono : phone;
            customer.Estado = EstadoCliente.Activo;
            customer.Etiquetas = MergeTags(customer.Etiquetas, "cotizacion");
        }

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
            Seguro = simulation.Insurance,
            GastosAdministrativos = simulation.AdministrativeFees,
            PlazoMeses = simulation.TermMonths,
            TasaInteresMensual = simulation.MonthlyInterestRate,
            ValorFinanciado = simulation.FinancedAmount,
            CuotaMensualEstimada = simulation.MonthlyPayment,
            TotalPagarEstimado = simulation.TotalPayment,
            TipoCredito = simulation.CreditType,
            UsoConfiguracionFinancieraEmpresa = simulation.UsedCompanyFinancialSettings,
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

    [HttpPost("simulate")]
    public async Task<ActionResult<QuoteSimulationResultDto>> Simulate(QuoteSimulationDto dto, CancellationToken cancellationToken)
    {
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar un producto.");
        if (dto.DownPayment < 0) throw new ValidationException("La cuota inicial no puede ser negativa.");
        if (dto.Insurance < 0) throw new ValidationException("El seguro no puede ser negativo.");
        if (dto.AdministrativeFees < 0) throw new ValidationException("Los gastos administrativos no pueden ser negativos.");
        if (dto.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");

        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado o inactivo.");
        var financialSettings = await GetFinancialSettingsAsync(cancellationToken);
        var simulation = CalculateSimulation(product.Precio, dto.DownPayment, dto.Insurance, dto.AdministrativeFees, dto.TermMonths, dto.MonthlyInterestRate, financialSettings);

        return Ok(new QuoteSimulationResultDto(
            simulation.DownPayment,
            simulation.Insurance,
            simulation.AdministrativeFees,
            simulation.TermMonths,
            simulation.MonthlyInterestRate,
            simulation.FinancedAmount,
            simulation.MonthlyPayment,
            simulation.TotalPayment,
            simulation.CreditType,
            simulation.UsedCompanyFinancialSettings));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var quote = await db.Cotizaciones
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .ThenInclude(x => x!.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cotizacion no encontrada.");
        var company = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantContext.EmpresaId, cancellationToken);
        var dto = ToDto(quote);
        var quotePhoto = quote.Producto?.Fotos
            .OrderByDescending(x => x.EsPrincipalCotizacion)
            .ThenBy(x => x.Orden)
            .FirstOrDefault();
        var image = quotePhoto is null
            ? null
            : new QuotePdfImage(quotePhoto.Datos, quotePhoto.ContentType, quotePhoto.NombreArchivo);
        var logo = ToPdfImage(company?.LogoDataUrl, "logo-empresa.png");
        var bytes = SimplePdfGenerator.Quote(
            dto,
            company?.Nombre ?? "Empresa",
            image,
            logo,
            quote.Cliente?.Telefono,
            quote.Cliente?.Direccion,
            quote.UsuarioCreacion);
        return File(bytes, "application/pdf", $"{quote.Numero}.pdf");
    }

    private static QuotePdfImage? ToPdfImage(string? dataUrl, string fileName)
    {
        if (string.IsNullOrWhiteSpace(dataUrl)) return null;
        var comma = dataUrl.IndexOf(',');
        if (comma <= 0) return null;
        var header = dataUrl[..comma];
        var contentTypeEnd = header.IndexOf(';');
        if (!header.StartsWith("data:", StringComparison.OrdinalIgnoreCase) || contentTypeEnd <= 5) return null;
        var contentType = header[5..contentTypeEnd];
        try
        {
            return new QuotePdfImage(Convert.FromBase64String(dataUrl[(comma + 1)..]), contentType, fileName);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static QuoteDto ToDto(Cotizacion x)
    {
        var productName = x.Producto is null
            ? "Producto"
            : ProductName(x.Producto);
        var termMonths = x.PlazoMeses <= 0 ? 24 : x.PlazoMeses;
        var financedAmount = x.ValorFinanciado <= 0 && x.CuotaMensualEstimada <= 0
            ? Math.Max(x.PrecioProducto + x.Seguro + x.GastosAdministrativos - x.CuotaInicial, 0)
            : x.ValorFinanciado;
        var totalPayment = x.TotalPagarEstimado <= 0 ? x.CuotaInicial + financedAmount : x.TotalPagarEstimado;
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
            x.Seguro,
            x.GastosAdministrativos,
            termMonths,
            x.TasaInteresMensual,
            financedAmount,
            x.CuotaMensualEstimada,
            totalPayment,
            x.TipoCredito,
            x.UsoConfiguracionFinancieraEmpresa,
            x.FechaCotizacion,
            x.ValidaHasta,
            x.Observaciones);
    }

    private async Task<ConfiguracionFinancieraEmpresa?> GetFinancialSettingsAsync(CancellationToken cancellationToken) =>
        await db.ConfiguracionesFinancierasEmpresa.FirstOrDefaultAsync(x => x.Activa, cancellationToken);

    private static CreditSimulation CalculateSimulation(decimal productPrice, decimal downPayment, decimal insurance, decimal administrativeFees, int termMonths, decimal monthlyInterestRate, ConfiguracionFinancieraEmpresa? financialSettings)
    {
        var normalizedInsurance = Math.Max(insurance, 0);
        var normalizedAdministrativeFees = Math.Max(administrativeFees, 0);
        var totalToFinance = productPrice + normalizedInsurance + normalizedAdministrativeFees;
        var normalizedDownPayment = Math.Min(downPayment, totalToFinance);
        var financedAmount = Math.Max(totalToFinance - normalizedDownPayment, 0);
        var normalizedTermMonths = Math.Max(termMonths, 1);

        if (financialSettings is { UsarTablaMontelibano: true, Activa: true })
        {
            normalizedTermMonths = Math.Min(normalizedTermMonths, financialSettings.PlazoMaximoMeses);
            var creditType = financedAmount <= financialSettings.SalarioMinimoVigente * 2 ? "Bajo monto" : "Consumo";
            var annualRate = creditType == "Bajo monto" ? financialSettings.TasaBajoMontoEa : financialSettings.TasaConsumoEa;
            var legalMonthlyRate = AnnualEffectiveToMonthly(annualRate);
            var factorMonthlyRate = financialSettings.TasaFactorMensual / 100;
            var paymentBase = normalizedTermMonths > 3
                ? Payment(financedAmount, normalizedTermMonths, factorMonthlyRate)
                : financedAmount / normalizedTermMonths;
            var configuredMonthlyPayment = RoundTo(paymentBase, financialSettings.RedondeoCuota);
            var configuredTotalPayment = normalizedDownPayment + (configuredMonthlyPayment * normalizedTermMonths);

            return new CreditSimulation(
                normalizedDownPayment,
                normalizedInsurance,
                normalizedAdministrativeFees,
                normalizedTermMonths,
                Math.Round(legalMonthlyRate * 100, 3, MidpointRounding.AwayFromZero),
                financedAmount,
                configuredMonthlyPayment,
                configuredTotalPayment,
                creditType,
                true);
        }

        var monthlyRate = monthlyInterestRate / 100;
        var monthlyPayment = financedAmount == 0
            ? 0
            : monthlyRate == 0
                ? financedAmount / normalizedTermMonths
                : Payment(financedAmount, normalizedTermMonths, monthlyRate);
        monthlyPayment = Math.Round(monthlyPayment, 0, MidpointRounding.AwayFromZero);
        var totalPayment = normalizedDownPayment + (monthlyPayment * normalizedTermMonths);
        return new CreditSimulation(normalizedDownPayment, normalizedInsurance, normalizedAdministrativeFees, normalizedTermMonths, monthlyInterestRate, financedAmount, monthlyPayment, totalPayment, "Manual", false);
    }

    private static decimal AnnualEffectiveToMonthly(decimal annualEffectivePercent) =>
        (decimal)Math.Pow(1 + (double)(annualEffectivePercent / 100), 1d / 12d) - 1;

    private static decimal Payment(decimal amount, int termMonths, decimal monthlyRate) =>
        amount * monthlyRate / (1 - (decimal)Math.Pow(1 + (double)monthlyRate, -termMonths));

    private static decimal RoundTo(decimal value, int multiple)
    {
        if (multiple <= 1) return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        return Math.Round(value / multiple, 0, MidpointRounding.AwayFromZero) * multiple;
    }

    private sealed record CreditSimulation(
        decimal DownPayment,
        decimal Insurance,
        decimal AdministrativeFees,
        int TermMonths,
        decimal MonthlyInterestRate,
        decimal FinancedAmount,
        decimal MonthlyPayment,
        decimal TotalPayment,
        string CreditType,
        bool UsedCompanyFinancialSettings);

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
    private static string? NormalizeIdentification(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private static string MergeTags(string? current, string tag)
    {
        var tags = (current ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (!tags.Any(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase))) tags.Add(tag);
        return string.Join(", ", tags);
    }

    private static IReadOnlyList<string> Split(string? value) => (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private static string Join(params string?[] values) => string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
    private static string? Join(IEnumerable<string> values)
    {
        var joined = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}
