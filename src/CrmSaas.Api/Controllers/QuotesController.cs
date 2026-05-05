using CrmSaas.Api.Services;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
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
        if (string.IsNullOrWhiteSpace(dto.CustomerFirstNames)) throw new ValidationException("Los nombres del cliente son obligatorios.");
        if (string.IsNullOrWhiteSpace(dto.CustomerLastNames)) throw new ValidationException("Los apellidos del cliente son obligatorios.");
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar una moto para cotizar.");

        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId && x.Activo, cancellationToken)
            ?? throw new KeyNotFoundException("Moto no encontrada o inactiva.");
        var initialStage = await db.EtapasNegocio
            .Where(x => x.Activa)
            .OrderBy(x => x.Orden)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No hay etapas activas en el pipeline para crear la oportunidad.");

        var fullName = $"{dto.CustomerFirstNames.Trim()} {dto.CustomerLastNames.Trim()}".Trim();
        var productName = $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
        var customer = new Cliente
        {
            Nombre = dto.CustomerFirstNames.Trim(),
            Nombres = dto.CustomerFirstNames.Trim(),
            Apellidos = dto.CustomerLastNames.Trim(),
            Email = string.Empty,
            Estado = EstadoCliente.Activo,
            Etiquetas = "cotizacion"
        };
        db.Clientes.Add(customer);

        var number = $"COT-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var quote = new Cotizacion
        {
            Numero = number,
            TipoIdentificacion = dto.IdentificationType,
            NumeroIdentificacion = dto.IdentificationNumber,
            NombresCliente = dto.CustomerFirstNames.Trim(),
            ApellidosCliente = dto.CustomerLastNames.Trim(),
            ClienteId = customer.Id,
            ProductoId = product.Id,
            PrecioProducto = product.Precio,
            FechaCotizacion = DateTime.UtcNow,
            ValidaHasta = DateTime.UtcNow.AddDays(7),
            Observaciones = dto.Notes
        };
        var deal = new Negocio
        {
            Titulo = $"{fullName} - {productName}",
            ClienteId = customer.Id,
            EtapaNegocioId = initialStage.Id,
            Valor = product.Precio,
            ProbabilidadCierre = initialStage.ProbabilidadPredeterminada,
            FechaEstimadaCierre = DateTime.UtcNow.AddDays(15),
            Estado = EstadoNegocio.Abierto
        };

        db.Cotizaciones.Add(quote);
        db.Negocios.Add(deal);
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
            ? "Moto"
            : $"{x.Producto.Marca} {x.Producto.Modelo} {x.Producto.Referencia}".Trim();
        return new QuoteDto(x.Id, x.Numero, x.TipoIdentificacion, x.NumeroIdentificacion, x.NombresCliente, x.ApellidosCliente, x.ClienteId, x.ProductoId, productName, x.PrecioProducto, x.FechaCotizacion, x.ValidaHasta, x.Observaciones);
    }
}
