using AutoMapper;
using CrmSaas.Application.Abstractions;
using CrmSaas.Application.DTOs;
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

public sealed class CustomerService(ICrmDbContext db, IMapper mapper) : ICustomerService
{
    public async Task<IReadOnlyCollection<CustomerDto>> GetAsync(CancellationToken cancellationToken) =>
        await db.Clientes
            .OrderBy(x => x.Nombre)
            .Select(x => new CustomerDto(x.Id, (x.Nombres + " " + x.Apellidos).Trim() == string.Empty ? x.Nombre : (x.Nombres + " " + x.Apellidos).Trim(), x.Nombres, x.Apellidos, x.EmpresaCliente, x.Email, x.Telefono, x.Estado, x.Etiquetas))
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
            .Select(x => new LeadDto(x.Id, (x.Nombres + " " + x.Apellidos).Trim() == string.Empty ? x.Nombre : (x.Nombres + " " + x.Apellidos).Trim(), x.Nombres, x.Apellidos, x.Email, x.Telefono, x.Fuente, x.Calificacion, x.Convertido, x.ClienteId))
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
        await db.Actividades.OrderBy(x => x.FechaProgramada).Select(x => new ActivityDto(x.Id, x.Titulo, x.Descripcion, x.Tipo, x.Estado, x.FechaProgramada, x.RecordatorioEn, x.ClienteId, x.NegocioId, x.UsuarioAsignadoId)).ToListAsync(cancellationToken);

    public async Task<ActivityDto> CreateAsync(UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Actividad>(dto);
        db.Actividades.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task<ActivityDto> UpdateAsync(Guid id, UpsertActivityDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.Actividades.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Actividad no encontrada.");
        mapper.Map(dto, entity);
        await db.SaveChangesAsync(cancellationToken);
        return CrmDtoMapper.ToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Actividades.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Actividad no encontrada.");
        db.Actividades.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}

file static class CrmDtoMapper
{
    public static CustomerDto ToDto(Cliente x) => new(x.Id, DisplayName(x.Nombres, x.Apellidos, x.Nombre), x.Nombres, x.Apellidos, x.EmpresaCliente, x.Email, x.Telefono, x.Estado, x.Etiquetas);
    public static LeadDto ToDto(Prospecto x) => new(x.Id, DisplayName(x.Nombres, x.Apellidos, x.Nombre), x.Nombres, x.Apellidos, x.Email, x.Telefono, x.Fuente, x.Calificacion, x.Convertido, x.ClienteId);
    public static DealStageDto ToDto(EtapaNegocio x) => new(x.Id, x.Nombre, x.Orden, x.ProbabilidadPredeterminada, x.Activa);
    public static DealDto ToDto(Negocio x) => new(x.Id, x.Titulo, x.ClienteId, x.EtapaNegocioId, x.Valor, x.ProbabilidadCierre, x.FechaEstimadaCierre, x.Estado);
    public static ActivityDto ToDto(Actividad x) => new(x.Id, x.Titulo, x.Descripcion, x.Tipo, x.Estado, x.FechaProgramada, x.RecordatorioEn, x.ClienteId, x.NegocioId, x.UsuarioAsignadoId);
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

        return new DashboardDto(
            await openDeals.SumAsync(x => x.Valor, cancellationToken),
            await openDeals.SumAsync(x => x.Valor * (x.ProbabilidadCierre / 100), cancellationToken),
            await db.Clientes.CountAsync(x => x.Estado == EstadoCliente.Activo, cancellationToken),
            await db.Prospectos.CountAsync(x => !x.Convertido, cancellationToken),
            await db.Actividades.CountAsync(x => x.Estado == EstadoActividad.Pendiente, cancellationToken),
            recent);
    }
}
