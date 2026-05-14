using AutoMapper;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;

namespace CrmSaas.Application.Mapping;

public sealed class CrmMappingProfile : Profile
{
    public CrmMappingProfile()
    {
        CreateMap<UpsertCustomerDto, Cliente>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.FirstNames.Trim()))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.FirstNames.Trim()))
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.LastNames.Trim()))
            .ForMember(dest => dest.TipoIdentificacion, opt => opt.MapFrom(src => src.IdentificationType))
            .ForMember(dest => dest.NumeroIdentificacion, opt => opt.MapFrom(src => src.IdentificationNumber))
            .ForMember(dest => dest.EmpresaCliente, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.IndicativoTelefono, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.PhoneCountryCode) ? "+57" : src.PhoneCountryCode))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.FechaNacimiento, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.Ocupacion, opt => opt.MapFrom(src => src.Occupation))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Etiquetas, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Notes));

        CreateMap<UpsertLeadDto, Prospecto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.FirstNames.Trim()))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.FirstNames.Trim()))
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.LastNames.Trim()))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Fuente, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.Calificacion, opt => opt.MapFrom(src => src.Rating));

        CreateMap<UpsertDealStageDto, EtapaNegocio>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Orden, opt => opt.MapFrom(src => src.Order))
            .ForMember(dest => dest.ProbabilidadPredeterminada, opt => opt.MapFrom(src => src.DefaultProbability))
            .ForMember(dest => dest.Activa, opt => opt.MapFrom(src => src.Active));

        CreateMap<UpsertDealDto, Negocio>()
            .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.EtapaNegocioId, opt => opt.MapFrom(src => src.StageId))
            .ForMember(dest => dest.ProbabilidadCierre, opt => opt.MapFrom(src => src.CloseProbability))
            .ForMember(dest => dest.FechaEstimadaCierre, opt => opt.MapFrom(src => src.EstimatedCloseDate))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status));

        CreateMap<UpsertActivityDto, Actividad>()
            .ForMember(dest => dest.Titulo, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.FechaProgramada, opt => opt.MapFrom(src => src.ScheduledAt))
            .ForMember(dest => dest.RecordatorioEn, opt => opt.MapFrom(src => src.ReminderAt))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.NegocioId, opt => opt.MapFrom(src => src.DealId))
            .ForMember(dest => dest.UsuarioAsignadoId, opt => opt.MapFrom(src => src.AssignedUserId));
    }

}
