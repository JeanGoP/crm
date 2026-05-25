using AutoMapper;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;

namespace CrmSaas.Application.Mapping;

public sealed class CrmMappingProfile : Profile
{
    public CrmMappingProfile()
    {
        CreateMap<UpsertCustomerDto, Cliente>()
            .ForMember(dest => dest.PrimerNombre, opt => opt.MapFrom(src => NameParts.FirstName(src.FirstName, src.FirstNames)))
            .ForMember(dest => dest.SegundoNombre, opt => opt.MapFrom(src => NameParts.MiddleName(src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.PrimerApellido, opt => opt.MapFrom(src => NameParts.LastName(src.LastName, src.LastNames)))
            .ForMember(dest => dest.SegundoApellido, opt => opt.MapFrom(src => NameParts.SecondLastName(src.SecondLastName, src.LastNames)))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => NameParts.FirstNames(src.FirstName, src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => NameParts.FirstNames(src.FirstName, src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => NameParts.LastNames(src.LastName, src.SecondLastName, src.LastNames)))
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
            .ForMember(dest => dest.PrimerNombre, opt => opt.MapFrom(src => NameParts.FirstName(src.FirstName, src.FirstNames)))
            .ForMember(dest => dest.SegundoNombre, opt => opt.MapFrom(src => NameParts.MiddleName(src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.PrimerApellido, opt => opt.MapFrom(src => NameParts.LastName(src.LastName, src.LastNames)))
            .ForMember(dest => dest.SegundoApellido, opt => opt.MapFrom(src => NameParts.SecondLastName(src.SecondLastName, src.LastNames)))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => NameParts.FirstNames(src.FirstName, src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => NameParts.FirstNames(src.FirstName, src.MiddleName, src.FirstNames)))
            .ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => NameParts.LastNames(src.LastName, src.SecondLastName, src.LastNames)))
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

file static class NameParts
{
    public static string FirstNames(string? firstName, string? middleName, string fallback) =>
        Join(FirstName(firstName, fallback), MiddleName(middleName, fallback));

    public static string LastNames(string? lastName, string? secondLastName, string fallback) =>
        Join(LastName(lastName, fallback), SecondLastName(secondLastName, fallback));

    public static string FirstName(string? value, string fallback) => Clean(value) ?? Split(fallback).ElementAtOrDefault(0) ?? string.Empty;
    public static string? MiddleName(string? value, string fallback) => Clean(value) ?? Join(Split(fallback).Skip(1));
    public static string LastName(string? value, string fallback) => Clean(value) ?? Split(fallback).ElementAtOrDefault(0) ?? string.Empty;
    public static string? SecondLastName(string? value, string fallback) => Clean(value) ?? Join(Split(fallback).Skip(1));

    private static IReadOnlyList<string> Split(string? value) => (value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Join(params string?[] values) => string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim()));
    private static string? Join(IEnumerable<string> values)
    {
        var joined = string.Join(" ", values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        return string.IsNullOrWhiteSpace(joined) ? null : joined;
    }
}
