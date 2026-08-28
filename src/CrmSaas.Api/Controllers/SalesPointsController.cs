using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/sales-points")]
public sealed class SalesPointsController(CrmDbContext db) : ControllerBase
{
    private const int MaxLogoDataUrlLength = 300000;
    private static readonly string[] AllowedLogoPrefixes =
    [
        "data:image/png;base64,",
        "data:image/jpeg;base64,",
        "data:image/webp;base64,"
    ];

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SalesPointDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.PuntosVenta
            .Include(x => x.Tasas)
            .OrderByDescending(x => x.Activa)
            .ThenBy(x => x.Ciudad)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<SalesPointDto>> Create(UpsertSalesPointDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var code = NormalizeCode(dto.Code);
        if (await db.PuntosVenta.AnyAsync(x => x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe una sede con ese codigo." });
        }

        var rates = NormalizeRates(dto);
        var entity = new PuntoVenta();
        Apply(entity, dto, code, rates);
        AddRates(entity, rates);
        db.PuntosVenta.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<SalesPointDto>> Update(Guid id, UpsertSalesPointDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync<ActionResult<SalesPointDto>>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var entity = await db.PuntosVenta.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Sede no encontrada.");
            var code = NormalizeCode(dto.Code);
            if (await db.PuntosVenta.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
            {
                return BadRequest(new { detail = "Ya existe una sede con ese codigo." });
            }

            var rates = NormalizeRates(dto);
            Apply(entity, dto, code, rates);
            var existingRates = await db.TasasPuntosVenta
                .Where(x => x.PuntoVentaId == id)
                .ToListAsync(cancellationToken);
            var submittedIds = rates.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
            if (submittedIds.Any(rateId => existingRates.All(x => x.Id != rateId)))
            {
                throw new KeyNotFoundException("Una de las tasas no pertenece a la sede.");
            }

            foreach (var rate in rates)
            {
                var existing = rate.Id.HasValue ? existingRates.First(x => x.Id == rate.Id.Value) : null;
                if (existing is null)
                {
                    db.TasasPuntosVenta.Add(CreateRate(id, rate));
                }
                else
                {
                    ApplyRate(existing, rate);
                }
            }

            var omittedRates = existingRates.Where(x => !submittedIds.Contains(x.Id)).ToList();
            var omittedRateIds = omittedRates.Select(x => x.Id).ToArray();
            var usedRateIds = await db.Cotizaciones
                .Where(x => x.TasaPuntoVentaId.HasValue && omittedRateIds.Contains(x.TasaPuntoVentaId.Value))
                .Select(x => x.TasaPuntoVentaId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
            foreach (var omittedRate in omittedRates)
            {
                if (usedRateIds.Contains(omittedRate.Id)) omittedRate.Activa = false;
                else db.TasasPuntosVenta.Remove(omittedRate);
            }
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
            var updated = await db.PuntosVenta
                .AsNoTracking()
                .Include(x => x.Tasas)
                .SingleAsync(x => x.Id == id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ToDto(updated));
        });
    }

    private static void Apply(PuntoVenta entity, UpsertSalesPointDto dto, string code, IReadOnlyCollection<UpsertSalesPointRateDto> rates)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.Ciudad = dto.City.Trim();
        entity.Direccion = Normalize(dto.Address);
        entity.Telefono = Normalize(dto.Phone);
        entity.MarcaPrincipal = dto.MainBrand.Trim();
        entity.LogoMarcaDataUrl = NormalizeLogo(dto.BrandLogoDataUrl);
        var defaultRate = rates.FirstOrDefault(x => x.Active) ?? rates.First();
        entity.TasaFactorMensual = defaultRate.FactorMonthlyRate;
        entity.PlazoMaximoMeses = defaultRate.MaxTermMonths;
        entity.VigenciaCotizacionDias = dto.QuoteValidityDays;
        entity.ModalidadEntrega = NormalizeDeliveryMode(dto.DeliveryMode);
        entity.TiempoSoatDias = dto.SoatDays;
        entity.TiempoMatriculaDias = dto.RegistrationDays;
        entity.ProveedorSoat = Normalize(dto.SoatProvider);
        entity.TramitadorMatricula = Normalize(dto.RegistrationAgent);
        entity.CondicionesComerciales = Normalize(dto.CommercialTerms);
        entity.BodegasInventarioExterno = NormalizeWarehouseCodes(dto.ExternalInventoryWarehouseCodes);
        entity.Activa = dto.Active;
    }

    private static void Validate(UpsertSalesPointDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new InvalidOperationException("El nombre de la sede es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Code)) throw new InvalidOperationException("El codigo de la sede es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.City)) throw new InvalidOperationException("La ciudad de la sede es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.MainBrand)) throw new InvalidOperationException("La marca principal es obligatoria.");
        if (dto.FactorMonthlyRate < 0) throw new InvalidOperationException("La tasa mensual no puede ser negativa.");
        if (dto.MaxTermMonths <= 0) throw new InvalidOperationException("El plazo maximo debe ser mayor a cero.");
        if (dto.QuoteValidityDays <= 0) throw new InvalidOperationException("La vigencia de cotizacion debe ser mayor a cero.");
        if (dto.SoatDays < 0 || dto.RegistrationDays < 0) throw new InvalidOperationException("Los tiempos de tramite no pueden ser negativos.");
        var rates = NormalizeRates(dto);
        if (rates.Any(x => string.IsNullOrWhiteSpace(x.Name))) throw new InvalidOperationException("Todas las tasas deben tener nombre.");
        if (rates.Any(x => x.FactorMonthlyRate < 0)) throw new InvalidOperationException("Las tasas mensuales no pueden ser negativas.");
        if (rates.Any(x => x.MaxTermMonths <= 0)) throw new InvalidOperationException("El plazo maximo de cada tasa debe ser mayor a cero.");
        if (rates.GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1)) throw new InvalidOperationException("No puede repetir el nombre de una tasa dentro de la misma sede.");
        if (rates.Where(x => x.Id.HasValue).GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("No puede enviar una tasa repetida.");
        if (!rates.Any(x => x.Active)) throw new InvalidOperationException("La sede debe tener al menos una tasa activa.");
    }

    private static SalesPointDto ToDto(PuntoVenta x) => new(
        x.Id,
        x.Nombre,
        x.Codigo,
        x.Ciudad,
        x.Direccion,
        x.Telefono,
        x.MarcaPrincipal,
        x.LogoMarcaDataUrl,
        x.TasaFactorMensual,
        x.PlazoMaximoMeses,
        x.VigenciaCotizacionDias,
        x.ModalidadEntrega,
        x.TiempoSoatDias,
        x.TiempoMatriculaDias,
        x.ProveedorSoat,
        x.TramitadorMatricula,
        x.CondicionesComerciales,
        x.BodegasInventarioExterno,
        x.Tasas
            .OrderByDescending(rate => rate.Activa)
            .ThenBy(rate => rate.Nombre)
            .Select(rate => new SalesPointRateDto(rate.Id, rate.Nombre, rate.TasaFactorMensual, rate.PlazoMaximoMeses, rate.Activa))
            .ToList(),
        x.Activa);

    private static IReadOnlyCollection<UpsertSalesPointRateDto> NormalizeRates(UpsertSalesPointDto dto) =>
        dto.Rates is { Count: > 0 }
            ? dto.Rates.ToList()
            : [new UpsertSalesPointRateDto(null, "Tasa general", dto.FactorMonthlyRate, dto.MaxTermMonths, true)];

    private static void AddRates(PuntoVenta salesPoint, IEnumerable<UpsertSalesPointRateDto> rates)
    {
        foreach (var rate in CreateRates(salesPoint.Id, rates))
        {
            salesPoint.Tasas.Add(rate);
        }
    }

    private static IEnumerable<TasaPuntoVenta> CreateRates(Guid salesPointId, IEnumerable<UpsertSalesPointRateDto> rates) =>
        rates.Select(rate => CreateRate(salesPointId, rate));

    private static TasaPuntoVenta CreateRate(Guid salesPointId, UpsertSalesPointRateDto rate)
    {
        var entity = new TasaPuntoVenta { PuntoVentaId = salesPointId };
        ApplyRate(entity, rate);
        return entity;
    }

    private static void ApplyRate(TasaPuntoVenta entity, UpsertSalesPointRateDto rate)
    {
        entity.Nombre = rate.Name.Trim();
        entity.TasaFactorMensual = rate.FactorMonthlyRate;
        entity.PlazoMaximoMeses = rate.MaxTermMonths;
        entity.Activa = rate.Active;
    }

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant().Replace(" ", "-");
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeWarehouseCodes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var codes = value
            .Split([',', ';', '|', '\r', '\n', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return codes.Length == 0 ? null : string.Join(",", codes);
    }

    private static string NormalizeDeliveryMode(string value) =>
        string.Equals(value, "Completa", StringComparison.OrdinalIgnoreCase) ? "Completa" : "ConSoat";

    private static string? NormalizeLogo(string? logoDataUrl)
    {
        if (string.IsNullOrWhiteSpace(logoDataUrl))
        {
            return null;
        }

        var logo = logoDataUrl.Trim();
        if (logo.Length > MaxLogoDataUrlLength)
        {
            throw new InvalidOperationException("El logo de marca es demasiado grande. Usa una imagen menor a 300 KB.");
        }

        if (!AllowedLogoPrefixes.Any(prefix => logo.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("El logo de marca debe ser una imagen PNG, JPG o WebP.");
        }

        return logo;
    }
}
