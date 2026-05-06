using CrmSaas.Application.DTOs;
using CrmSaas.Application.Services;
using CrmSaas.Domain.Entities;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService service, IValidator<UpsertCustomerDto> validator, CrmDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CustomerDto>>> Get(CancellationToken cancellationToken) => Ok(await service.GetAsync(cancellationToken));

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<Customer360Dto>> Summary(Guid id, CancellationToken cancellationToken)
    {
        var customer = await db.Clientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");

        var quotes = await db.Cotizaciones
            .Include(x => x.Producto)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCotizacion)
            .ToListAsync(cancellationToken);
        var creditApplications = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);
        var deals = await db.Negocios
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => new DealDto(x.Id, x.Titulo, x.ClienteId, x.EtapaNegocioId, x.Valor, x.ProbabilidadCierre, x.FechaEstimadaCierre, x.Estado))
            .ToListAsync(cancellationToken);
        var activities = await db.Actividades
            .Where(x => x.ClienteId == id)
            .OrderByDescending(x => x.FechaProgramada)
            .Select(x => new ActivityDto(x.Id, x.Titulo, x.Descripcion, x.Tipo, x.Estado, x.FechaProgramada, x.RecordatorioEn, x.ClienteId, x.NegocioId, x.UsuarioAsignadoId))
            .ToListAsync(cancellationToken);

        return Ok(new Customer360Dto(
            ToCustomerDto(customer),
            quotes.Select(ToQuoteDto).ToList(),
            creditApplications.Select(ToCreditApplicationDto).ToList(),
            deals,
            activities));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        var created = await service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);
        return Ok(await service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static CustomerDto ToCustomerDto(Cliente x)
    {
        var displayName = $"{x.Nombres} {x.Apellidos}".Trim();
        return new CustomerDto(x.Id, string.IsNullOrWhiteSpace(displayName) ? x.Nombre : displayName, x.Nombres, x.Apellidos, x.EmpresaCliente, x.Email, x.Telefono, x.Estado, x.Etiquetas);
    }

    private static QuoteDto ToQuoteDto(Cotizacion x)
    {
        var productName = x.Producto is null ? "Moto" : $"{x.Producto.Marca} {x.Producto.Modelo} {x.Producto.Referencia}".Trim();
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

    private static CreditApplicationDto ToCreditApplicationDto(SolicitudCredito x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Moto" : $"{x.Producto.Marca} {x.Producto.Modelo} {x.Producto.Referencia}".Trim();
        return new CreditApplicationDto(
            x.Id,
            x.Numero,
            x.ClienteId,
            customerName,
            x.ProductoId,
            productName,
            x.CotizacionId,
            x.NegocioId,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.FechaNacimiento,
            x.Celular,
            x.Direccion,
            x.Ciudad,
            x.Ocupacion,
            x.IngresosMensuales,
            x.CuotaInicial,
            x.PlazoMeses,
            x.ValorMoto,
            x.Estado,
            x.Observaciones,
            x.Documentos.OrderBy(d => d.Tipo).Select(d => new CreditDocumentDto(
                d.Id,
                d.Tipo,
                d.Nombre,
                d.Estado,
                d.FechaRecepcion,
                d.Observaciones,
                !string.IsNullOrWhiteSpace(d.RutaArchivo),
                d.NombreArchivo,
                d.ContentType,
                d.TamanoBytes,
                d.FechaCarga)).ToList());
    }
}
