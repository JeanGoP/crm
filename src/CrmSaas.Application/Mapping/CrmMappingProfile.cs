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
            .ForMember(dest => dest.EmpresaCliente, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Etiquetas, opt => opt.MapFrom(src => src.Tags));

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
            .ForMember(dest => dest.UsuarioAsignadoId, opt => opt.MapFrom(src => src.AssignedUserId));
    }

}
