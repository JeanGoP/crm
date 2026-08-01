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
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Items)
            .ThenInclude(x => x.Producto)
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
        if (string.IsNullOrWhiteSpace(NormalizePhoneDigits(dto.PhoneNumber))) throw new ValidationException("El telefono del cliente es obligatorio.");

        var financialSettings = await GetFinancialSettingsAsync(cancellationToken);
        var salesPoint = await GetCurrentSalesPointAsync(cancellationToken);
        var requirementProfile = dto.RequirementProfileId.HasValue
            ? await db.PerfilesRequisito.FirstOrDefaultAsync(x => x.Id == dto.RequirementProfileId.Value && x.Activo, cancellationToken)
                ?? throw new KeyNotFoundException("Perfil de requisitos no encontrado o inactivo.")
            : await GetDefaultRequirementProfileAsync(cancellationToken);
        var requestedItems = NormalizeQuoteItems(dto);
        if (requestedItems.Count == 0) throw new ValidationException("Debe seleccionar al menos un producto para cotizar.");
        if (requestedItems.Count > 4) throw new ValidationException("Puede comparar maximo 4 productos por cotizacion.");

        var productIds = requestedItems.Select(x => x.ProductId).Distinct().ToArray();
        var products = await db.Productos
            .Include(x => x.PreciosPorSede)
            .Where(x => productIds.Contains(x.Id) && x.Activo)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (products.Count != productIds.Length) throw new KeyNotFoundException("Uno de los productos no existe o esta inactivo.");
        var now = ColombiaTime.Now;
        var promotions = await GetActivePromotionsAsync(now, cancellationToken);

        var calculatedItems = requestedItems.Select((item, index) =>
        {
            if (item.DownPayment < 0) throw new ValidationException("La cuota inicial no puede ser negativa.");
            if (item.Insurance < 0) throw new ValidationException("El seguro no puede ser negativo.");
            if (item.AdministrativeFees < 0) throw new ValidationException("Los gastos administrativos no pueden ser negativos.");
            if (item.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");
            if (item.MonthlyInterestRate < 0) throw new ValidationException("La tasa mensual no puede ser negativa.");
            var product = products[item.ProductId];
            var productPrice = ResolveProductPrice(product, salesPoint);
            var insurance = item.Insurance > 0 ? item.Insurance : product.Soat;
            var administrativeFees = item.AdministrativeFees > 0 ? item.AdministrativeFees : product.Matricula + product.Impuestos;
            var promotion = ResolvePromotion(product, salesPoint, promotions, productPrice);
            var simulation = CalculateSimulation(promotion.DiscountedProductPrice, item.DownPayment, insurance, administrativeFees, item.TermMonths, item.MonthlyInterestRate, financialSettings, salesPoint);
            return new { Source = item, Product = product, ProductPrice = productPrice, Promotion = promotion, Simulation = simulation, Order = index + 1 };
        }).ToList();
        var primary = calculatedItems[0];
        var product = primary.Product;
        var quoteCategories = calculatedItems
            .Select(x => x.Product.Categoria.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var quoteCategory = quoteCategories.Count == 1 ? quoteCategories[0] : null;
        var quoteAsBundle = calculatedItems.Count > 1
            && !string.IsNullOrWhiteSpace(quoteCategory)
            && await db.CategoriasProducto.AnyAsync(x => x.Nombre == quoteCategory && x.Activa && x.CotizarComoPaquete, cancellationToken);
        var quoteProductPrice = quoteAsBundle ? calculatedItems.Sum(x => x.ProductPrice) : primary.ProductPrice;
        var quotePromotionDiscount = quoteAsBundle ? calculatedItems.Sum(x => x.Promotion.DiscountAmount) : primary.Promotion.DiscountAmount;
        var quotePromotion = quoteAsBundle ? null : primary.Promotion.Promotion;
        var quotePromotionName = quoteAsBundle && quotePromotionDiscount > 0 ? "Promociones aplicadas" : primary.Promotion.Promotion?.Nombre;
        var simulation = quoteAsBundle
            ? CalculateSimulation(
                calculatedItems.Sum(x => x.Promotion.DiscountedProductPrice),
                primary.Simulation.DownPayment,
                calculatedItems.Sum(x => x.Simulation.Insurance),
                calculatedItems.Sum(x => x.Simulation.AdministrativeFees),
                primary.Simulation.TermMonths,
                primary.Simulation.MonthlyInterestRate,
                financialSettings,
                salesPoint)
            : primary.Simulation;
        var initialStage = await db.EtapasNegocio
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No hay etapas activas en el pipeline para crear la oportunidad.");

        var fullName = $"{firstNames} {lastNames}".Trim();
        var phone = FormatPhone(dto.PhoneCountryCode, dto.PhoneNumber);
        var productName = quoteAsBundle ? $"{quoteCategory} ({calculatedItems.Count} articulos)" : ProductName(product);
        var normalizedIdentification = NormalizeIdentification(dto.IdentificationNumber);
        var customerReference = CustomerReference(fullName, normalizedIdentification, dto.PhoneNumber);
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
            if (!string.IsNullOrWhiteSpace(firstNames))
            {
                customer.Nombre = firstNames;
                customer.Nombres = firstNames;
                customer.PrimerNombre = firstName;
                customer.SegundoNombre = middleName;
            }
            if (!string.IsNullOrWhiteSpace(lastNames))
            {
                customer.Apellidos = lastNames;
                customer.PrimerApellido = lastName;
                customer.SegundoApellido = secondLastName;
            }
            customer.TipoIdentificacion = dto.IdentificationType;
            customer.NumeroIdentificacion = normalizedIdentification ?? dto.IdentificationNumber;
            customer.IndicativoTelefono = string.IsNullOrWhiteSpace(dto.PhoneCountryCode) ? customer.IndicativoTelefono : dto.PhoneCountryCode.Trim();
            customer.Telefono = string.IsNullOrWhiteSpace(phone) ? customer.Telefono : phone;
            customer.Estado = EstadoCliente.Activo;
            customer.Etiquetas = MergeTags(customer.Etiquetas, "cotizacion");
        }

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
            PuntoVentaId = salesPoint?.Id,
            PerfilRequisitoId = requirementProfile?.Id,
            PerfilRequisito = requirementProfile,
            NombreSede = salesPoint?.Nombre,
            MarcaSede = salesPoint?.MarcaPrincipal,
            ModalidadEntregaSede = salesPoint?.ModalidadEntrega,
            TasaFactorMensualSede = salesPoint?.TasaFactorMensual,
            PlazoMaximoMesesSede = salesPoint?.PlazoMaximoMeses,
            VigenciaCotizacionDiasSede = salesPoint?.VigenciaCotizacionDias,
            CondicionesSede = salesPoint?.CondicionesComerciales,
            PromocionId = quotePromotion?.Id,
            Promocion = quotePromotion,
            NombrePromocion = quotePromotionName,
            DescuentoPromocion = quotePromotionDiscount,
            PrecioProducto = quoteProductPrice,
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
            ValidaHasta = now.AddDays(salesPoint?.VigenciaCotizacionDias ?? 7),
            Observaciones = dto.Notes
        };
        foreach (var item in calculatedItems)
        {
            quote.Items.Add(new CotizacionItem
            {
                ProductoId = item.Product.Id,
                Orden = item.Order,
                PrecioProducto = item.ProductPrice,
                PromocionId = item.Promotion.Promotion?.Id,
                Promocion = item.Promotion.Promotion,
                NombrePromocion = item.Promotion.Promotion?.Nombre,
                DescuentoPromocion = item.Promotion.DiscountAmount,
                CuotaInicial = item.Simulation.DownPayment,
                Seguro = item.Simulation.Insurance,
                GastosAdministrativos = item.Simulation.AdministrativeFees,
                PlazoMeses = item.Simulation.TermMonths,
                TasaInteresMensual = item.Simulation.MonthlyInterestRate,
                ValorFinanciado = item.Simulation.FinancedAmount,
                CuotaMensualEstimada = item.Simulation.MonthlyPayment,
                TotalPagarEstimado = item.Simulation.TotalPayment,
                TipoCredito = item.Simulation.CreditType,
                UsoConfiguracionFinancieraEmpresa = item.Simulation.UsedCompanyFinancialSettings,
                CodigoBodegaInventario = Clean(item.Source.InventoryWarehouseCode),
                NombreBodegaInventario = Clean(item.Source.InventoryWarehouseName),
                PresentacionInventario = Clean(item.Source.InventoryPresentation),
                NumeroSerieInventario = Clean(item.Source.InventorySerialNumber),
                NumeroMotorInventario = Clean(item.Source.InventoryEngineNumber),
                NumeroChasisInventario = Clean(item.Source.InventoryChassisNumber)
            });
        }
        var deal = new Negocio
        {
            Titulo = calculatedItems.Count > 1 ? $"{customerReference} - Comparativo {calculatedItems.Count} productos" : $"{customerReference} - {productName}",
            ClienteId = customer.Id,
            EtapaNegocioId = initialStage.Id,
            Valor = quoteProductPrice,
            ProbabilidadCierre = initialStage.ProbabilidadPredeterminada,
            FechaEstimadaCierre = now.AddDays(15),
            Estado = EstadoNegocio.Abierto
        };
        var followUp = new Actividad
        {
            Titulo = AutomaticFollowUpTitle,
            Descripcion = $"Cotizacion {number}: contactar a {customerReference} para resolver dudas y avanzar la venta de {productName}.",
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
        foreach (var item in quote.Items)
        {
            item.Producto = products[item.ProductoId];
        }

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

        var product = await db.Productos
            .Include(x => x.PreciosPorSede)
            .FirstOrDefaultAsync(x => x.Id == dto.ProductId && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado o inactivo.");
        var financialSettings = await GetFinancialSettingsAsync(cancellationToken);
        var salesPoint = await GetCurrentSalesPointAsync(cancellationToken);
        var productPrice = ResolveProductPrice(product, salesPoint);
        var promotion = ResolvePromotion(product, salesPoint, await GetActivePromotionsAsync(ColombiaTime.Now, cancellationToken), productPrice);
        var insurance = dto.Insurance > 0 ? dto.Insurance : product.Soat;
        var administrativeFees = dto.AdministrativeFees > 0 ? dto.AdministrativeFees : product.Matricula + product.Impuestos;
        var simulation = CalculateSimulation(promotion.DiscountedProductPrice, dto.DownPayment, insurance, administrativeFees, dto.TermMonths, dto.MonthlyInterestRate, financialSettings, salesPoint);

        return Ok(new QuoteSimulationResultDto(
            simulation.DownPayment,
            simulation.Insurance,
            simulation.AdministrativeFees,
            simulation.TermMonths,
            simulation.MonthlyInterestRate,
            promotion.Promotion?.Id,
            promotion.Promotion?.Nombre,
            promotion.DiscountAmount,
            promotion.DiscountedProductPrice,
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
            .Include(x => x.PuntoVenta)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Promocion)
            .Include(x => x.Items)
            .ThenInclude(x => x.Producto)
            .ThenInclude(x => x!.Fotos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cotizacion no encontrada.");
        var company = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantContext.EmpresaId, cancellationToken);
        var dto = ToDto(quote);
        var quotePhoto = ResolveQuotePhoto(quote);
        var image = quotePhoto is null
            ? null
            : new QuotePdfImage(quotePhoto.Datos, quotePhoto.ContentType, quotePhoto.NombreArchivo);
        var brandLogo = ToPdfImage(quote.PuntoVenta?.LogoMarcaDataUrl, "logo-marca.png");
        var companyLogo = ToPdfImage(company?.LogoDataUrl, "logo-empresa.png");
        var bytes = SimplePdfGenerator.Quote(
            dto,
            company?.Nombre ?? "Empresa",
            image,
            companyLogo,
            brandLogo,
            quote.Cliente?.Telefono,
            quote.Cliente?.Direccion,
            quote.UsuarioCreacion);
        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        return File(bytes, "application/pdf", $"{quote.Numero}.pdf");
    }

    private static ProductoFoto? ResolveQuotePhoto(Cotizacion quote)
    {
        static IEnumerable<ProductoFoto> OrderedPhotos(Producto? product) =>
            (product?.Fotos ?? [])
                .OrderByDescending(x => x.EsPrincipalCotizacion)
                .ThenBy(x => x.Orden);

        var itemPhoto = quote.Items
            .OrderBy(x => x.Orden)
            .SelectMany(item => OrderedPhotos(item.Producto))
            .FirstOrDefault(IsPdfSupportedPhoto);

        return itemPhoto ?? OrderedPhotos(quote.Producto).FirstOrDefault(IsPdfSupportedPhoto);
    }

    private static bool IsPdfSupportedPhoto(ProductoFoto photo) =>
        IsJpeg(photo.Datos) || IsPng(photo.Datos);

    private static bool IsJpeg(byte[] data) =>
        data.Length > 4 &&
        data[0] == 0xFF &&
        data[1] == 0xD8;

    private static bool IsPng(byte[] data) =>
        data.Length > 8 &&
        data[0] == 137 &&
        data[1] == 80 &&
        data[2] == 78 &&
        data[3] == 71 &&
        data[4] == 13 &&
        data[5] == 10 &&
        data[6] == 26 &&
        data[7] == 10;

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
            ? Math.Max(DiscountedPrice(x.PrecioProducto, x.DescuentoPromocion) + x.Seguro + x.GastosAdministrativos - x.CuotaInicial, 0)
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
            x.Producto?.FichaTecnica,
            x.PuntoVentaId,
            x.NombreSede,
            x.MarcaSede,
            x.ModalidadEntregaSede,
            x.CondicionesSede,
            x.PerfilRequisitoId,
            x.PerfilRequisito?.Nombre,
            x.PromocionId,
            x.NombrePromocion,
            x.DescuentoPromocion,
            DiscountedPrice(x.PrecioProducto, x.DescuentoPromocion),
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
            x.Observaciones,
            QuoteItems(x).ToList());
    }

    private static IReadOnlyCollection<CreateQuoteItemDto> NormalizeQuoteItems(CreateQuoteDto dto)
    {
        if (dto.Items is { Count: > 0 })
        {
            return dto.Items.Where(x => x.ProductId != Guid.Empty).ToList();
        }

        return dto.ProductId == Guid.Empty
            ? []
            : [new CreateQuoteItemDto(dto.ProductId, dto.DownPayment, dto.Insurance, dto.AdministrativeFees, dto.TermMonths, dto.MonthlyInterestRate)];
    }

    private static IEnumerable<QuoteItemDto> QuoteItems(Cotizacion quote)
    {
        if (quote.Items.Count > 0)
        {
            return quote.Items.OrderBy(x => x.Orden).Select(x => ToItemDto(x));
        }

        return
        [
            new QuoteItemDto(
                quote.Id,
                quote.ProductoId,
                quote.Producto is null ? "Producto" : ProductName(quote.Producto),
                quote.PrecioProducto,
                quote.PromocionId,
                quote.NombrePromocion,
                quote.DescuentoPromocion,
                DiscountedPrice(quote.PrecioProducto, quote.DescuentoPromocion),
                quote.CuotaInicial,
                quote.Seguro,
                quote.GastosAdministrativos,
                quote.PlazoMeses <= 0 ? 24 : quote.PlazoMeses,
                quote.TasaInteresMensual,
                quote.ValorFinanciado,
                quote.CuotaMensualEstimada,
                quote.TotalPagarEstimado,
                quote.TipoCredito,
                quote.UsoConfiguracionFinancieraEmpresa,
                1)
        ];
    }

    private static QuoteItemDto ToItemDto(CotizacionItem item)
    {
        var productName = item.Producto is null ? "Producto" : ProductName(item.Producto);
        var termMonths = item.PlazoMeses <= 0 ? 24 : item.PlazoMeses;
        var financedAmount = item.ValorFinanciado <= 0 && item.CuotaMensualEstimada <= 0
            ? Math.Max(DiscountedPrice(item.PrecioProducto, item.DescuentoPromocion) + item.Seguro + item.GastosAdministrativos - item.CuotaInicial, 0)
            : item.ValorFinanciado;
        var totalPayment = item.TotalPagarEstimado <= 0 ? item.CuotaInicial + financedAmount : item.TotalPagarEstimado;
        return new QuoteItemDto(
            item.Id,
            item.ProductoId,
            productName,
            item.PrecioProducto,
            item.PromocionId,
            item.NombrePromocion,
            item.DescuentoPromocion,
            DiscountedPrice(item.PrecioProducto, item.DescuentoPromocion),
            item.CuotaInicial,
            item.Seguro,
            item.GastosAdministrativos,
            termMonths,
            item.TasaInteresMensual,
            financedAmount,
            item.CuotaMensualEstimada,
            totalPayment,
            item.TipoCredito,
            item.UsoConfiguracionFinancieraEmpresa,
            item.Orden,
            item.CodigoBodegaInventario,
            item.NombreBodegaInventario,
            item.PresentacionInventario,
            item.NumeroSerieInventario,
            item.NumeroMotorInventario,
            item.NumeroChasisInventario);
    }

    private async Task<ConfiguracionFinancieraEmpresa?> GetFinancialSettingsAsync(CancellationToken cancellationToken) =>
        await db.ConfiguracionesFinancierasEmpresa.FirstOrDefaultAsync(x => x.Activa, cancellationToken);

    private async Task<PuntoVenta?> GetCurrentSalesPointAsync(CancellationToken cancellationToken)
    {
        var currentUser = await db.Usuarios
            .Include(x => x.PuntoVenta)
            .FirstOrDefaultAsync(x => x.Email == tenantContext.UsuarioActual && x.Activo, cancellationToken);
        if (currentUser?.PuntoVenta is { Activa: true } salesPoint)
        {
            return salesPoint;
        }

        return await db.PuntosVenta
            .Where(x => x.Activa)
            .OrderByDescending(x => x.Codigo == "PRINCIPAL")
            .ThenBy(x => x.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PerfilRequisito?> GetDefaultRequirementProfileAsync(CancellationToken cancellationToken) =>
        await db.PerfilesRequisito
            .Where(x => x.Activo)
            .OrderByDescending(x => x.Codigo == "EMPLEADO")
            .ThenBy(x => x.Nombre)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<IReadOnlyCollection<Promocion>> GetActivePromotionsAsync(DateTime now, CancellationToken cancellationToken) =>
        await db.Promociones
            .Where(x => x.Activa && x.VigenteDesde.Date <= now.Date && x.VigenteHasta.Date >= now.Date)
            .ToListAsync(cancellationToken);

    private static PromotionApplication ResolvePromotion(Producto product, PuntoVenta? salesPoint, IReadOnlyCollection<Promocion> promotions, decimal productPrice)
    {
        var candidates = promotions
            .Where(x => AppliesToProduct(x, product) && AppliesToSalesPoint(x, salesPoint))
            .Select(x =>
            {
                var discount = PromotionDiscount(productPrice, x);
                var specificity = (x.ProductoId.HasValue ? 8 : 0)
                    + (!string.IsNullOrWhiteSpace(x.Marca) ? 4 : 0)
                    + (!string.IsNullOrWhiteSpace(x.Color) ? 2 : 0)
                    + (x.PuntoVentaId.HasValue ? 1 : 0);
                return new { Promotion = x, Discount = discount, Specificity = specificity };
            })
            .Where(x => x.Discount > 0)
            .OrderByDescending(x => x.Specificity)
            .ThenByDescending(x => x.Discount)
            .ThenBy(x => x.Promotion.Nombre)
            .FirstOrDefault();

        return candidates is null
            ? new PromotionApplication(null, 0, productPrice)
            : new PromotionApplication(candidates.Promotion, candidates.Discount, DiscountedPrice(productPrice, candidates.Discount));
    }

    private static decimal ResolveProductPrice(Producto product, PuntoVenta? salesPoint)
    {
        if (salesPoint is not null && IsApplianceCategory(product.Categoria))
        {
            var salesPointPrice = product.PreciosPorSede
                .Where(x => x.Activo && x.PuntoVentaId == salesPoint.Id)
                .OrderByDescending(x => x.VigenteDesde ?? DateTime.MinValue)
                .FirstOrDefault();
            if (salesPointPrice is not null) return salesPointPrice.Precio;
        }

        return product.Precio;
    }

    private static bool AppliesToProduct(Promocion promotion, Producto product)
    {
        if (promotion.ProductoId.HasValue && promotion.ProductoId.Value != product.Id) return false;
        if (!string.IsNullOrWhiteSpace(promotion.Marca) && !promotion.Marca.Trim().Equals(product.Marca?.Trim(), StringComparison.OrdinalIgnoreCase)) return false;
        if (!string.IsNullOrWhiteSpace(promotion.Color) && !promotion.Color.Trim().Equals(product.Color?.Trim(), StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    private static bool AppliesToSalesPoint(Promocion promotion, PuntoVenta? salesPoint) =>
        !promotion.PuntoVentaId.HasValue || (salesPoint is not null && promotion.PuntoVentaId.Value == salesPoint.Id);

    private static bool IsApplianceCategory(string category) =>
        (category ?? string.Empty).Contains("electrodom", StringComparison.OrdinalIgnoreCase);

    private static decimal PromotionDiscount(decimal productPrice, Promocion promotion)
    {
        var discount = promotion.TipoDescuento.Equals("Porcentaje", StringComparison.OrdinalIgnoreCase)
            ? productPrice * (promotion.ValorDescuento / 100)
            : promotion.ValorDescuento;
        return Math.Min(Math.Max(Math.Round(discount, 0, MidpointRounding.AwayFromZero), 0), productPrice);
    }

    private static decimal DiscountedPrice(decimal productPrice, decimal discount) => Math.Max(productPrice - Math.Max(discount, 0), 0);

    private static CreditSimulation CalculateSimulation(decimal productPrice, decimal downPayment, decimal insurance, decimal administrativeFees, int termMonths, decimal monthlyInterestRate, ConfiguracionFinancieraEmpresa? financialSettings, PuntoVenta? salesPoint)
    {
        var normalizedInsurance = Math.Max(insurance, 0);
        var normalizedAdministrativeFees = Math.Max(administrativeFees, 0);
        var totalToFinance = productPrice + normalizedInsurance + normalizedAdministrativeFees;
        var normalizedDownPayment = Math.Min(downPayment, totalToFinance);
        var financedAmount = Math.Max(totalToFinance - normalizedDownPayment, 0);
        var normalizedTermMonths = Math.Max(termMonths, 1);

        if (financialSettings is { UsarTablaMontelibano: true, Activa: true })
        {
            normalizedTermMonths = Math.Min(normalizedTermMonths, salesPoint?.PlazoMaximoMeses > 0 ? salesPoint.PlazoMaximoMeses : financialSettings.PlazoMaximoMeses);
            var creditType = financedAmount <= financialSettings.SalarioMinimoVigente * 2 ? "Bajo monto" : "Consumo";
            var annualRate = creditType == "Bajo monto" ? financialSettings.TasaBajoMontoEa : financialSettings.TasaConsumoEa;
            var legalMonthlyRate = AnnualEffectiveToMonthly(annualRate);
            var factorMonthlyRate = (salesPoint?.TasaFactorMensual > 0 ? salesPoint.TasaFactorMensual : financialSettings.TasaFactorMensual) / 100;
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

    private sealed record PromotionApplication(Promocion? Promotion, decimal DiscountAmount, decimal DiscountedProductPrice);

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }

    private static string FormatPhone(string? countryCode, string? phoneNumber)
    {
        var normalizedNumber = NormalizePhoneDigits(phoneNumber);
        if (string.IsNullOrWhiteSpace(normalizedNumber)) return string.Empty;
        var normalizedCode = string.IsNullOrWhiteSpace(countryCode) ? "+57" : countryCode.Trim();
        if (!normalizedCode.StartsWith("+")) normalizedCode = "+" + normalizedCode;
        return $"{normalizedCode} {normalizedNumber}".Trim();
    }

    private static string CustomerReference(string? fullName, string? identification, string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(fullName)) return fullName.Trim();
        if (!string.IsNullOrWhiteSpace(identification)) return $"Documento {identification.Trim()}";
        var normalizedPhone = NormalizePhoneDigits(phoneNumber);
        if (!string.IsNullOrWhiteSpace(normalizedPhone)) return $"Telefono {normalizedPhone}";
        return "Cliente sin nombre";
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string NormalizePhoneDigits(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());
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
