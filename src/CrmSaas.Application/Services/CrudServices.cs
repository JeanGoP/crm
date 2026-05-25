using AutoMapper;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Common;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Application.Services;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerDto>> GetAsync(CancellationToken cancellationToken);
    Task<CustomerDto> CreateAsync(UpsertCustomerDto dto, CancellationToken cancellationToken);
    Task<CustomerDto> UpdateAsync(Guid id, UpsertCustomerDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface ILeadService
{
    Task<IReadOnlyCollection<LeadDto>> GetAsync(CancellationToken cancellationToken);
    Task<LeadDto> CreateAsync(UpsertLeadDto dto, CancellationToken cancellationToken);
    Task<LeadDto> UpdateAsync(Guid id, UpsertLeadDto dto, CancellationToken cancellationToken);
    Task<LeadDto> ConvertAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IPipelineService
{
    Task<IReadOnlyCollection<DealStageDto>> GetStagesAsync(CancellationToken cancellationToken);
    Task<DealStageDto> CreateStageAsync(UpsertDealStageDto dto, CancellationToken cancellationToken);
    Task<DealStageDto> UpdateStageAsync(Guid id, UpsertDealStageDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DealDto>> GetDealsAsync(CancellationToken cancellationToken);
    Task<DealDto> CreateDealAsync(UpsertDealDto dto, CancellationToken cancellationToken);
    Task<DealDto> UpdateDealAsync(Guid id, UpsertDealDto dto, CancellationToken cancellationToken);
    Task DeleteDealAsync(Guid id, CancellationToken cancellationToken);
}

public interface IActivityService
{
    Task<IReadOnlyCollection<ActivityDto>> GetAsync(CancellationToken cancellationToken);
    Task<ActivityDto> CreateAsync(UpsertActivityDto dto, CancellationToken cancellationToken);
    Task<ActivityDto> UpdateAsync(Guid id, UpsertActivityDto dto, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
}

public interface ICommercialReportService
{
    Task<CommercialReportsDto> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
}

public sealed class CustomerService(ICrmDbContext db, IMapper mapper) : ICustomerService
{
    public async Task<IReadOnlyCollection<CustomerDto>> GetAsync(CancellationToken cancellationToken) =>
        await db.Clientes
            .OrderBy(x => x.Nombre)
            .Select(x => new CustomerDto(
                x.Id,
                (x.Nombres + " " + x.Apellidos).Trim() == string.Empty ? x.Nombre : (x.Nombres + " " + x.Apellidos).Trim(),
                x.Nombres,
                x.Apellidos,
                x.PrimerNombre,
                x.SegundoNombre,
                x.PrimerApellido,
                x.SegundoApellido,
                x.TipoIdentificacion,
                x.NumeroIdentificacion,
                x.EmpresaCliente,
                x.Email,
                x.IndicativoTelefono,
                x.Telefono,
                x.Direccion,
                x.Ciudad,
                x.FechaNacimiento,
                x.Ocupacion,
                x.Estado,
                x.Etiquetas,
                x.Observaciones))
            .ToListAsync(cancellationToken);

    public async Task<CustomerDto> CreateAsync(UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Cliente>(dto);
        db.Clientes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpsertCustomerDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.Clientes.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Cliente no encontrado.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Clientes.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Cliente no encontrado.");
        db.Clientes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class LeadService(ICrmDbContext db, IMapper mapper) : ILeadService
{
    public async Task<IReadOnlyCollection<LeadDto>> GetAsync(CancellationToken cancellationToken) =>
        await db.Prospectos
            .OrderByDescending(x => x.FechaCreacion)
            .Select(x => new LeadDto(x.Id, (x.Nombres + " " + x.Apellidos).Trim() == string.Empty ? x.Nombre : (x.Nombres + " " + x.Apellidos).Trim(), x.Nombres, x.Apellidos, x.PrimerNombre, x.SegundoNombre, x.PrimerApellido, x.SegundoApellido, x.Email, x.Telefono, x.Fuente, x.Calificacion, x.Convertido, x.ClienteId))
            .ToListAsync(cancellationToken);

    public async Task<LeadDto> CreateAsync(UpsertLeadDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Prospecto>(dto);
        db.Prospectos.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<LeadDto> UpdateAsync(Guid id, UpsertLeadDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.Prospectos.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Prospecto no encontrado.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<LeadDto> ConvertAsync(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Prospectos.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Prospecto no encontrado.");
        if (lead.Convertido && lead.ClienteId.HasValue) return CrmDtoMapper.ToDto(lead);

        var customer = new Cliente
        {
            Nombre = string.IsNullOrWhiteSpace(lead.Nombres) ? lead.Nombre : lead.Nombres,
            Nombres = string.IsNullOrWhiteSpace(lead.Nombres) ? lead.Nombre : lead.Nombres,
            Apellidos = lead.Apellidos,
            PrimerNombre = lead.PrimerNombre,
            SegundoNombre = lead.SegundoNombre,
            PrimerApellido = lead.PrimerApellido,
            SegundoApellido = lead.SegundoApellido,
            Email = lead.Email,
            Telefono = lead.Telefono,
            Estado = EstadoCliente.Activo,
            Etiquetas = $"lead:{lead.Fuente}"
        };

        db.Clientes.Add(customer);
        lead.Convertido = true;
        lead.ClienteId = customer.Id;
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(lead);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Prospectos.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Prospecto no encontrado.");
        db.Prospectos.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class PipelineService(ICrmDbContext db, IMapper mapper) : IPipelineService
{
    public async Task<IReadOnlyCollection<DealStageDto>> GetStagesAsync(CancellationToken cancellationToken) =>
        await db.EtapasNegocio.OrderBy(x => x.Orden).Select(x => new DealStageDto(x.Id, x.Nombre, x.Orden, x.ProbabilidadPredeterminada, x.Activa)).ToListAsync(cancellationToken);

    public async Task<DealStageDto> CreateStageAsync(UpsertDealStageDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<EtapaNegocio>(dto);
        db.EtapasNegocio.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<DealStageDto> UpdateStageAsync(Guid id, UpsertDealStageDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.EtapasNegocio.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Etapa no encontrada.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<IReadOnlyCollection<DealDto>> GetDealsAsync(CancellationToken cancellationToken) =>
        await db.Negocios.OrderByDescending(x => x.FechaCreacion).Select(x => new DealDto(x.Id, x.Titulo, x.ClienteId, x.EtapaNegocioId, x.Valor, x.ProbabilidadCierre, x.FechaEstimadaCierre, x.Estado)).ToListAsync(cancellationToken);

    public async Task<DealDto> CreateDealAsync(UpsertDealDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Negocio>(dto);
        db.Negocios.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<DealDto> UpdateDealAsync(Guid id, UpsertDealDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.Negocios.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Negocio no encontrado.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task DeleteDealAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Negocios.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Negocio no encontrado.");
        db.Negocios.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ActivityService(ICrmDbContext db, IMapper mapper) : IActivityService
{
    public async Task<IReadOnlyCollection<ActivityDto>> GetAsync(CancellationToken cancellationToken) =>
        await ProjectActivities(db.Actividades.OrderBy(x => x.FechaProgramada))
            .ToListAsync(cancellationToken);

    public async Task<ActivityDto> CreateAsync(UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Actividad>(dto);
        db.Actividades.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await ProjectActivities(db.Actividades.Where(x => x.Id == entity.Id))
            .FirstAsync(cancellationToken);
    }

    public async Task<ActivityDto> UpdateAsync(Guid id, UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.Actividades.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Actividad no encontrada.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return await ProjectActivities(db.Actividades.Where(x => x.Id == entity.Id))
            .FirstAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Actividades.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Actividad no encontrada.");
        db.Actividades.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<ActivityDto> ProjectActivities(IQueryable<Actividad> query) =>
        query.Select(x => new ActivityDto(
            x.Id,
            x.Titulo,
            x.Descripcion,
            x.Tipo,
            x.Estado,
            x.FechaProgramada,
            x.RecordatorioEn,
            x.ClienteId,
            x.NegocioId,
            x.UsuarioAsignadoId,
            x.Cliente == null ? null : (x.Cliente.Nombres + " " + x.Cliente.Apellidos).Trim(),
            x.Negocio == null ? null : x.Negocio.Titulo));
}

file static class CrmDtoMapper
{
    public static CustomerDto ToDto(Cliente x) => new(
        x.Id,
        DisplayName(x.Nombres, x.Apellidos, x.Nombre),
        x.Nombres,
        x.Apellidos,
        x.PrimerNombre,
        x.SegundoNombre,
        x.PrimerApellido,
        x.SegundoApellido,
        x.TipoIdentificacion,
        x.NumeroIdentificacion,
        x.EmpresaCliente,
        x.Email,
        x.IndicativoTelefono,
        x.Telefono,
        x.Direccion,
        x.Ciudad,
        x.FechaNacimiento,
        x.Ocupacion,
        x.Estado,
        x.Etiquetas,
        x.Observaciones);
    public static LeadDto ToDto(Prospecto x) => new(x.Id, DisplayName(x.Nombres, x.Apellidos, x.Nombre), x.Nombres, x.Apellidos, x.PrimerNombre, x.SegundoNombre, x.PrimerApellido, x.SegundoApellido, x.Email, x.Telefono, x.Fuente, x.Calificacion, x.Convertido, x.ClienteId);
    public static DealStageDto ToDto(EtapaNegocio x) => new(x.Id, x.Nombre, x.Orden, x.ProbabilidadPredeterminada, x.Activa);
    public static DealDto ToDto(Negocio x) => new(x.Id, x.Titulo, x.ClienteId, x.EtapaNegocioId, x.Valor, x.ProbabilidadCierre, x.FechaEstimadaCierre, x.Estado);
    public static ActivityDto ToDto(Actividad x) => new(x.Id, x.Titulo, x.Descripcion, x.Tipo, x.Estado, x.FechaProgramada, x.RecordatorioEn, x.ClienteId, x.NegocioId, x.UsuarioAsignadoId, null, null);
    private static string DisplayName(string firstNames, string lastNames, string fallback)
    {
        var value = $"{firstNames} {lastNames}".Trim();
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}

public sealed class DashboardService(ICrmDbContext db) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken cancellationToken)
    {
        var openDeals = db.Negocios.Where(x => x.Estado == EstadoNegocio.Abierto);
        var recent = await db.Actividades
            .OrderByDescending(x => x.FechaCreacion)
            .Take(8)
            .Select(x => new RecentActivityDto(x.Titulo, x.FechaProgramada, x.Estado))
            .ToListAsync(cancellationToken);
        var today = ColombiaTime.Today;
        var tomorrow = today.AddDays(1);
        var overdueActivities = await db.Actividades
            .CountAsync(x => (x.Estado == EstadoActividad.Pendiente || x.Estado == EstadoActividad.EnProceso) && x.FechaProgramada < today, cancellationToken);
        var todayActivities = await db.Actividades
            .CountAsync(x => (x.Estado == EstadoActividad.Pendiente || x.Estado == EstadoActividad.EnProceso) && x.FechaProgramada >= today && x.FechaProgramada < tomorrow, cancellationToken);
        var alerts = await BuildAlertsAsync(today, tomorrow, cancellationToken);

        return new DashboardDto(
            await openDeals.SumAsync(x => x.Valor, cancellationToken),
            await openDeals.SumAsync(x => x.Valor * (x.ProbabilidadCierre / 100), cancellationToken),
            await db.Clientes.CountAsync(x => x.Estado == EstadoCliente.Activo, cancellationToken),
            await db.Prospectos.CountAsync(x => !x.Convertido, cancellationToken),
            await db.Actividades.CountAsync(x => x.Estado == EstadoActividad.Pendiente || x.Estado == EstadoActividad.EnProceso, cancellationToken),
            overdueActivities,
            todayActivities,
            recent,
            alerts);
    }

    private async Task<IReadOnlyCollection<CommercialAlertDto>> BuildAlertsAsync(DateTime today, DateTime tomorrow, CancellationToken cancellationToken)
    {
        var alerts = new List<CommercialAlertDto>();

        alerts.AddRange(await db.Actividades
            .Where(x => (x.Estado == EstadoActividad.Pendiente || x.Estado == EstadoActividad.EnProceso) && x.FechaProgramada < today)
            .OrderBy(x => x.FechaProgramada)
            .Take(5)
            .Select(x => new CommercialAlertDto(
                "Actividad",
                "error",
                "Actividad vencida",
                x.Titulo + " esta vencida y requiere seguimiento interno.",
                x.FechaProgramada,
                x.ClienteId == null ? "/actividades" : "/clientes/" + x.ClienteId))
            .ToListAsync(cancellationToken));

        alerts.AddRange(await db.Actividades
            .Where(x => (x.Estado == EstadoActividad.Pendiente || x.Estado == EstadoActividad.EnProceso) && x.FechaProgramada >= today && x.FechaProgramada < tomorrow)
            .OrderBy(x => x.FechaProgramada)
            .Take(5)
            .Select(x => new CommercialAlertDto(
                "Actividad",
                "warning",
                "Seguimiento para hoy",
                x.Titulo,
                x.FechaProgramada,
                x.ClienteId == null ? "/actividades" : "/clientes/" + x.ClienteId))
            .ToListAsync(cancellationToken));

        var pendingDocumentRequests = await db.SolicitudesCredito
            .Where(x => x.Estado == EstadoSolicitudCredito.DocumentosPendientes || x.Documentos.Any(d => d.Estado == EstadoDocumentoCredito.Pendiente || d.Estado == EstadoDocumentoCredito.Rechazado))
            .OrderBy(x => x.FechaCreacion)
            .Take(5)
            .Select(x => new
            {
                x.Numero,
                x.FechaCreacion,
                PendingDocuments = x.Documentos.Count(d => d.Estado == EstadoDocumentoCredito.Pendiente),
                RejectedDocuments = x.Documentos.Count(d => d.Estado == EstadoDocumentoCredito.Rechazado)
            })
            .ToListAsync(cancellationToken);

        alerts.AddRange(pendingDocumentRequests.Select(x =>
        {
            var documentSummary = x.PendingDocuments + x.RejectedDocuments == 0
                ? x.Numero + " requiere completar o validar documentos."
                : x.Numero + " tiene " + x.PendingDocuments + " documento(s) pendiente(s) y " + x.RejectedDocuments + " rechazado(s).";

            return new CommercialAlertDto(
                "Credito",
                "warning",
                "Documentos pendientes",
                documentSummary,
                x.FechaCreacion,
                "/solicitudes-credito");
        }));

        var studyLimit = today.AddDays(-2);
        var creditApplicationsInStudy = await db.SolicitudesCredito
            .Where(x => x.Estado == EstadoSolicitudCredito.EnEstudio && (x.FechaInicioEstudio ?? x.FechaActualizacion ?? x.FechaCreacion) <= studyLimit)
            .OrderBy(x => x.FechaInicioEstudio ?? x.FechaActualizacion ?? x.FechaCreacion)
            .Take(5)
            .Select(x => new
            {
                x.Numero,
                x.FechaInicioEstudio,
                x.FechaActualizacion,
                x.FechaCreacion
            })
            .ToListAsync(cancellationToken);

        alerts.AddRange(creditApplicationsInStudy.Select(x =>
        {
            var studyDate = (x.FechaInicioEstudio ?? x.FechaActualizacion ?? x.FechaCreacion).Date;
            var daysInStudy = Math.Max(1, (today - studyDate).Days);
            return new CommercialAlertDto(
                "Credito",
                "warning",
                "Credito en estudio",
                x.Numero + " lleva " + daysInStudy + " dia(s) en estudio. Revisar decision o solicitar informacion.",
                studyDate,
                "/solicitudes-credito");
        }));

        var quoteLimit = today.AddDays(-3);
        alerts.AddRange(await db.Cotizaciones
            .Where(x => x.FechaCotizacion <= quoteLimit && !db.Actividades.Any(a => a.ClienteId == x.ClienteId && a.FechaProgramada >= x.FechaCotizacion))
            .OrderByDescending(x => x.FechaCotizacion)
            .Take(5)
            .Select(x => new CommercialAlertDto(
                "Cotizacion",
                "info",
                "Cotizacion sin seguimiento",
                x.Numero + " no tiene actividad posterior registrada.",
                x.FechaCotizacion,
                "/clientes/" + x.ClienteId))
            .ToListAsync(cancellationToken));

        var staleDealLimit = today.AddDays(-7);
        var customerFollowUpLimit = today.AddDays(-7);
        var customersWithoutFollowUp = await db.Clientes
            .Where(x => x.Estado == EstadoCliente.Activo
                && x.FechaCreacion <= customerFollowUpLimit
                && !db.Actividades.Any(a => a.ClienteId == x.Id && a.Estado != EstadoActividad.Cancelada && a.FechaProgramada >= customerFollowUpLimit))
            .OrderBy(x => x.FechaCreacion)
            .Take(5)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Nombres,
                x.Apellidos,
                x.FechaCreacion
            })
            .ToListAsync(cancellationToken);

        alerts.AddRange(customersWithoutFollowUp.Select(x =>
        {
            var customerName = string.Join(" ", new[] { x.Nombres, x.Apellidos }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = x.Nombre;
            }

            return new CommercialAlertDto(
                "Cliente",
                "info",
                "Cliente sin seguimiento",
                customerName + " no tiene actividades recientes o futuras registradas.",
                x.FechaCreacion,
                "/clientes/" + x.Id);
        }));

        alerts.AddRange(await db.Negocios
            .Where(x => x.Estado == EstadoNegocio.Abierto && x.FechaCreacion <= staleDealLimit && !db.Actividades.Any(a => a.NegocioId == x.Id && a.FechaProgramada >= staleDealLimit))
            .OrderByDescending(x => x.Valor)
            .Take(5)
            .Select(x => new CommercialAlertDto(
                "Pipeline",
                "info",
                "Negocio sin actividad reciente",
                x.Titulo + " requiere seguimiento comercial.",
                x.FechaCreacion,
                x.ClienteId == null ? "/pipeline" : "/clientes/" + x.ClienteId))
            .ToListAsync(cancellationToken));

        return alerts
            .OrderBy(x => x.Severity == "error" ? 0 : x.Severity == "warning" ? 1 : 2)
            .ThenBy(x => x.CreatedAt)
            .Take(12)
            .ToList();
    }
}

public sealed class CommercialReportService(ICrmDbContext db) : ICommercialReportService
{
    public async Task<CommercialReportsDto> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var today = ColombiaTime.Today;
        var fromDate = (from ?? new DateTime(today.Year, today.Month, 1)).Date;
        var toDateExclusive = (to ?? today).Date.AddDays(1);

        if (toDateExclusive <= fromDate)
        {
            throw new ArgumentException("La fecha final debe ser mayor o igual a la fecha inicial.");
        }

        var quotes = db.Cotizaciones.Where(x => x.FechaCotizacion >= fromDate && x.FechaCotizacion < toDateExclusive);
        var creditApplications = db.SolicitudesCredito.Where(x => x.FechaCreacion >= fromDate && x.FechaCreacion < toDateExclusive);

        var totalQuotes = await quotes.CountAsync(cancellationToken);
        var quotesConvertedToCredit = await quotes.CountAsync(x => db.SolicitudesCredito.Any(s => s.CotizacionId == x.Id), cancellationToken);
        var approvedCredits = await creditApplications.CountAsync(x => x.Estado == EstadoSolicitudCredito.Aprobada || x.Estado == EstadoSolicitudCredito.Desembolsada, cancellationToken);
        var rejectedCredits = await creditApplications.CountAsync(x => x.Estado == EstadoSolicitudCredito.Rechazada, cancellationToken);
        var decidedCredits = approvedCredits + rejectedCredits;
        var approvedCreditAmount = await creditApplications
            .Where(x => x.Estado == EstadoSolicitudCredito.Aprobada || x.Estado == EstadoSolicitudCredito.Desembolsada)
            .SumAsync(x => x.ValorMoto, cancellationToken);

        var salesRows = await creditApplications
            .Where(x => x.Estado == EstadoSolicitudCredito.Aprobada || x.Estado == EstadoSolicitudCredito.Desembolsada)
            .GroupBy(x => x.UsuarioCreacion)
            .Select(x => new
            {
                SellerEmail = x.Key,
                ApprovedCredits = x.Count(),
                ApprovedAmount = x.Sum(s => s.ValorMoto)
            })
            .OrderByDescending(x => x.ApprovedAmount)
            .ToListAsync(cancellationToken);
        var sellerEmails = salesRows.Select(x => x.SellerEmail).ToArray();
        var sellerNames = await db.Usuarios
            .Where(x => sellerEmails.Contains(x.Email))
            .Select(x => new { x.Email, x.NombreCompleto })
            .ToDictionaryAsync(x => x.Email, x => x.NombreCompleto, cancellationToken);
        var quoteCountsBySeller = await quotes
            .GroupBy(x => x.UsuarioCreacion)
            .Select(x => new { SellerEmail = x.Key, Quotes = x.Count() })
            .ToDictionaryAsync(x => x.SellerEmail, x => x.Quotes, cancellationToken);
        var salesBySeller = salesRows
            .Select(x => new SalesBySellerDto(
                sellerNames.TryGetValue(x.SellerEmail, out var sellerName) ? sellerName : x.SellerEmail,
                quoteCountsBySeller.TryGetValue(x.SellerEmail, out var sellerQuotes) ? sellerQuotes : 0,
                x.ApprovedCredits,
                x.ApprovedAmount))
            .ToList();

        var convertedQuoteIds = await db.SolicitudesCredito
            .Where(x => x.CotizacionId.HasValue)
            .Select(x => x.CotizacionId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
        var quoteRows = await quotes
            .Select(x => new
            {
                x.Id,
                x.ValidaHasta,
                Amount = x.PrecioProducto
            })
            .ToListAsync(cancellationToken);
        var convertedQuoteSet = convertedQuoteIds.ToHashSet();
        var quotesByStatus = quoteRows
            .GroupBy(x => convertedQuoteSet.Contains(x.Id) ? "Convertida a credito" : x.ValidaHasta.Date < today ? "Vencida" : "Vigente")
            .Select(x => new QuotesByStatusDto(x.Key, x.Count(), x.Sum(q => q.Amount)))
            .OrderByDescending(x => x.Count)
            .ToList();

        var creditGroups = await creditApplications
            .GroupBy(x => x.Estado)
            .Select(x => new
            {
                Status = x.Key,
                Count = x.Count(),
                Amount = x.Sum(s => s.ValorMoto)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);
        var creditRows = creditGroups
            .Select(x => new CreditsByStatusDto(CreditStatusLabel(x.Status), x.Count, x.Amount))
            .ToList();

        var topQuotedProductGroups = await quotes
            .GroupBy(x => new { x.ProductoId, x.Producto!.Nombre, x.Producto.Marca, x.Producto.Modelo })
            .Select(x => new
            {
                x.Key.ProductoId,
                x.Key.Nombre,
                x.Key.Marca,
                x.Key.Modelo,
                QuoteCount = x.Count(),
                QuotedAmount = x.Sum(q => q.PrecioProducto)
            })
            .OrderByDescending(x => x.QuoteCount)
            .ThenByDescending(x => x.QuotedAmount)
            .Take(10)
            .ToListAsync(cancellationToken);
        var topQuotedProducts = topQuotedProductGroups
            .Select(x => new TopQuotedProductDto(x.ProductoId, x.Nombre, x.Marca, x.Modelo, x.QuoteCount, x.QuotedAmount))
            .ToList();

        var summary = new CommercialReportSummaryDto(
            totalQuotes,
            quotesConvertedToCredit,
            totalQuotes == 0 ? 0 : Math.Round((decimal)quotesConvertedToCredit / totalQuotes * 100, 2),
            approvedCredits,
            rejectedCredits,
            decidedCredits == 0 ? 0 : Math.Round((decimal)approvedCredits / decidedCredits * 100, 2),
            approvedCreditAmount);

        return new CommercialReportsDto(summary, salesBySeller, quotesByStatus, creditRows, topQuotedProducts);
    }

    private static string CreditStatusLabel(EstadoSolicitudCredito status) => status switch
    {
        EstadoSolicitudCredito.Borrador => "Cotizado",
        EstadoSolicitudCredito.DocumentosPendientes => "Documentos pendientes",
        EstadoSolicitudCredito.DocumentosRecibidos => "Credito en estudio",
        EstadoSolicitudCredito.EnEstudio => "Credito en estudio",
        EstadoSolicitudCredito.Aprobada => "Aprobado",
        EstadoSolicitudCredito.Rechazada => "Rechazado",
        EstadoSolicitudCredito.Desembolsada => "Entregado",
        EstadoSolicitudCredito.Interesado => "Interesado",
        EstadoSolicitudCredito.Desistida => "Desistido",
        _ => "Sin estado"
    };
}
