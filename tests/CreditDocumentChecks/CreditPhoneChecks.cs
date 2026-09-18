using CrmSaas.Api.Services;
using CrmSaas.Domain.Entities;
using FluentValidation;

internal static class CreditPhoneChecks
{
    public static void Run()
    {
        static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        foreach (var text in new[] { "3001234567", "300 123-4567", "(300) 123.4567", "+57 (300) 123-4567", "57 3001234567", "0057 3001234567" })
            Check(CreditPhoneValidation.Normalize(text) == "3001234567", "Normaliza el mismo teléfono colombiano.");
        Check(CreditPhoneValidation.Normalize(null) == "" && CreditPhoneValidation.Normalize(" - () ") == "", "Vacíos no colisionan.");
        Check(CreditPhoneValidation.Normalize("5731234567") == "5731234567", "No recorta un número nacional por empezar con 57.");
        Check(CreditPhoneValidation.Normalize("+58 3001234567") != CreditPhoneValidation.Normalize("3001234567"), "No confunde países.");
        Check(CreditPhoneValidation.Normalize("+1 202-555-0100") == CreditPhoneValidation.Normalize("001 2025550100"), "Prefijo internacional equivalente.");

        static (SolicitudCredito App, List<Action<string>> Set) Fresh()
        {
            var app = new SolicitudCredito();
            var setters = new List<Action<string>> { x => app.Celular = x, x => app.Referencia1Celular = x, x => app.Referencia2Celular = x };
            for (var i = 0; i < 2; i++)
            {
                var person = new CodeudorSolicitudCredito { Nombre = i == 0 ? "Ana" : "Luis", Orden = i, Activo = true };
                app.Codeudores.Add(person);
                setters.Add(x => person.Celular = x);
                setters.Add(x => person.Referencia1Celular = x);
                setters.Add(x => person.Referencia2Celular = x);
            }
            for (var i = 0; i < setters.Count; i++) setters[i]((3000000000L + i).ToString());
            return (app, setters);
        }
        for (var i = 0; i < 9; i++) for (var j = i + 1; j < 9; j++)
        {
            var (app, set) = Fresh();
            CreditPhoneValidation.Validate(app);
            set[i]("+57 (310) 999-8877"); set[j]("0057 3109998877");
            var groups = CreditPhoneValidation.FindDuplicates(app);
            Check(groups.Count == 1 && groups[0].Fields.Count == 2, "Detecta los 36 cruces cliente/codeudores/referencias.");
            try { CreditPhoneValidation.Validate(app); throw new Exception("Debe bloquear el guardado."); }
            catch (ValidationException ex) { Check(groups[0].Fields.All(ex.Message.Contains), "Indica ambos campos en el error."); }
            set[j]("3119998877"); CreditPhoneValidation.Validate(app);
        }
        var (existing, _) = Fresh();
        existing.CodeudorNombre = "Ana"; existing.CodeudorCelular = existing.Codeudores.First().Celular;
        CreditPhoneValidation.Validate(existing); // Legacy mirror is not a second owner.
        CreditCoDebtorService.Apply(existing, null); // Omitted collection keeps existing co-debtors on edit.
        existing.Celular = "+57 " + existing.Codeudores.Last().Referencia2Celular;
        Check(CreditPhoneValidation.FindDuplicates(existing).Single().Fields.Any(x => x.Contains("Codeudor 2") && x.Contains("referencia 2")), "Editar con colección omitida revisa codeudores conservados.");
        existing.Codeudores.Last().Activo = false;
        CreditPhoneValidation.Validate(existing); // Retired historical contacts are not part of the active application.
        var legacy = new SolicitudCredito { Celular = "3001234567", CodeudorNombre = "Anterior", CodeudorCelular = "+57 3001234567" };
        Check(CreditPhoneValidation.FindDuplicates(legacy).Count == 1, "Compatibilidad con solicitud antigua.");
        Check(CreditPhoneValidation.FindDuplicates(new SolicitudCredito()).Count == 0, "Vacíos no son duplicados.");
        Console.WriteLine("OK: teléfonos únicos, 36 cruces, prefijos, editar/compatibilidad, retirados y mensajes detallados.");
    }
}
