using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Entities;
using FluentValidation;

namespace CrmSaas.Api.Services;

public static class CreditCoDebtorService
{
    public static CreditCoDebtorDto ToDto(CodeudorSolicitudCredito x) => new(x.Id, x.Nombre,
        x.Identificacion, x.Celular, x.Parentesco, x.IngresosMensuales, x.Referencia1Nombre,
        x.Referencia1Celular, x.Referencia1Relacion, x.Referencia2Nombre, x.Referencia2Celular,
        x.Referencia2Relacion, x.Activo);

    public static bool Apply(SolicitudCredito application, IReadOnlyCollection<CreditCoDebtorDto>? requested)
    {
        // Older clients may omit the collection. Never treat omission as deleting co-debtors.
        if (requested is null)
        {
            if (application.Codeudores.Count != 0 || string.IsNullOrWhiteSpace(application.CodeudorNombre))
            {
                MirrorPrimary(application);
                return false;
            }
            var legacy = new CodeudorSolicitudCredito
            {
                SolicitudCreditoId = application.Id, EmpresaId = application.EmpresaId,
                Nombre = application.CodeudorNombre, Identificacion = application.CodeudorIdentificacion ?? "",
                Celular = application.CodeudorCelular ?? "", Parentesco = application.CodeudorParentesco,
                IngresosMensuales = application.CodeudorIngresosMensuales ?? 0,
                Referencia1Nombre = application.CodeudorReferencia1Nombre, Referencia1Celular = application.CodeudorReferencia1Celular,
                Referencia1Relacion = application.CodeudorReferencia1Relacion, Referencia2Nombre = application.CodeudorReferencia2Nombre,
                Referencia2Celular = application.CodeudorReferencia2Celular, Referencia2Relacion = application.CodeudorReferencia2Relacion
            };
            application.Codeudores.Add(legacy);
            return true;
        }

        var ids = new HashSet<Guid>();
        var identifications = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in requested)
        {
            if (item.Id.HasValue && (!ids.Add(item.Id.Value) || !application.Codeudores.Any(x => x.Id == item.Id.Value)))
                throw new ValidationException("El codeudor no pertenece a esta solicitud o está repetido.");
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Identification) || string.IsNullOrWhiteSpace(item.Mobile))
                throw new ValidationException("Cada codeudor requiere nombre, identificación y celular.");
            if (!identifications.Add(item.Identification.Trim())) throw new ValidationException("No repita la identificación de un codeudor en la solicitud.");
            if (item.MonthlyIncome < 0) throw new ValidationException("Los ingresos del codeudor no pueden ser negativos.");
            if (new[] { item.Reference1Name, item.Reference1Mobile, item.Reference1Relationship, item.Reference2Name, item.Reference2Mobile, item.Reference2Relationship }.Any(string.IsNullOrWhiteSpace))
                throw new ValidationException($"Complete las dos referencias de {item.Name}.");
        }

        var before = application.Codeudores.OrderBy(x => x.Orden).Select(ToDto).ToArray();
        foreach (var existing in application.Codeudores) existing.Activo = false;
        var order = 0;
        foreach (var item in requested)
        {
            var entity = item.Id.HasValue ? application.Codeudores.Single(x => x.Id == item.Id.Value)
                : new CodeudorSolicitudCredito { SolicitudCreditoId = application.Id, EmpresaId = application.EmpresaId };
            if (!item.Id.HasValue) application.Codeudores.Add(entity);
            entity.Activo = true;
            entity.Orden = order++;
            entity.Nombre = item.Name.Trim();
            entity.Identificacion = item.Identification.Trim();
            entity.Celular = item.Mobile.Trim();
            entity.Parentesco = item.Relationship?.Trim();
            entity.IngresosMensuales = item.MonthlyIncome;
            entity.Referencia1Nombre = item.Reference1Name?.Trim();
            entity.Referencia1Celular = item.Reference1Mobile?.Trim();
            entity.Referencia1Relacion = item.Reference1Relationship?.Trim();
            entity.Referencia2Nombre = item.Reference2Name?.Trim();
            entity.Referencia2Celular = item.Reference2Mobile?.Trim();
            entity.Referencia2Relacion = item.Reference2Relationship?.Trim();
        }
        MirrorPrimary(application);
        return !before.SequenceEqual(application.Codeudores.OrderBy(x => x.Orden).Select(ToDto));
    }

    private static void MirrorPrimary(SolicitudCredito application)
    {
        // Keep older reports/integrations compatible, while the collection is authoritative.
        var first = application.Codeudores.Where(x => x.Activo).OrderBy(x => x.Orden).FirstOrDefault();
        application.CodeudorNombre = first?.Nombre;
        application.CodeudorIdentificacion = first?.Identificacion;
        application.CodeudorCelular = first?.Celular;
        application.CodeudorParentesco = first?.Parentesco;
        application.CodeudorIngresosMensuales = first?.IngresosMensuales;
        application.CodeudorReferencia1Nombre = first?.Referencia1Nombre;
        application.CodeudorReferencia1Celular = first?.Referencia1Celular;
        application.CodeudorReferencia1Relacion = first?.Referencia1Relacion;
        application.CodeudorReferencia2Nombre = first?.Referencia2Nombre;
        application.CodeudorReferencia2Celular = first?.Referencia2Celular;
        application.CodeudorReferencia2Relacion = first?.Referencia2Relacion;
    }
}
