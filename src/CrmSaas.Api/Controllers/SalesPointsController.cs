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
            .OrderByDescending(x => x.Activa)
            .ThenBy(x => x.Ciudad)
            .ThenBy(x => x.Nombre)
            .Select(x => new SalesPointDto(
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
                x.Activa))
            .ToListAsync(cancellationToken);

        return Ok(rows);
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

        var entity = new PuntoVenta();
        Apply(entity, dto, code);
        db.PuntosVenta.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<SalesPointDto>> Update(Guid id, UpsertSalesPointDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.PuntosVenta.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Sede no encontrada.");
        var code = NormalizeCode(dto.Code);
        if (await db.PuntosVenta.AnyAsync(x => x.Id != id && x.Codigo == code, cancellationToken))
        {
            return BadRequest(new { detail = "Ya existe una sede con ese codigo." });
        }

        Apply(entity, dto, code);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(entity));
    }

    private static void Apply(PuntoVenta entity, UpsertSalesPointDto dto, string code)
    {
        entity.Nombre = dto.Name.Trim();
        entity.Codigo = code;
        entity.Ciudad = dto.City.Trim();
        entity.Direccion = Normalize(dto.Address);
        entity.Telefono = Normalize(dto.Phone);
        entity.MarcaPrincipal = dto.MainBrand.Trim();
        entity.LogoMarcaDataUrl = NormalizeLogo(dto.BrandLogoDataUrl);
        entity.TasaFactorMensual = dto.FactorMonthlyRate;
        entity.PlazoMaximoMeses = dto.MaxTermMonths;
        entity.VigenciaCotizacionDias = dto.QuoteValidityDays;
        entity.ModalidadEntrega = NormalizeDeliveryMode(dto.DeliveryMode);
        entity.TiempoSoatDias = dto.SoatDays;
        entity.TiempoMatriculaDias = dto.RegistrationDays;
        entity.ProveedorSoat = Normalize(dto.SoatProvider);
        entity.TramitadorMatricula = Normalize(dto.RegistrationAgent);
        entity.CondicionesComerciales = Normalize(dto.CommercialTerms);
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
        x.Activa);

    private static string NormalizeCode(string value) => value.Trim().ToUpperInvariant().Replace(" ", "-");
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
