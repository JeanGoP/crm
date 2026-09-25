using System.Text;
using System.Text.RegularExpressions;
using CrmSaas.Api.Services;
using CrmSaas.Application.DTOs;

static class CreditSignaturePdfChecks
{
    public static void Run(CreditApplicationDto original, string[] args)
    {
        var details = new CreditFormDetailsDto
        {
            IdentificationType = "Cédula de ciudadanía", HousingType = "Propia", MaritalStatus = "Casado(a)",
            Email = "cliente@example.com", Employer = "Comercial de ejemplo", JobTitle = "Auxiliar administrativo",
            WorkPhone = "604 555 0100", WorkAddress = "Calle 10 # 20-30", WorkEmail = "empleo@example.com",
            LocationReference = "Frente al parque central", Reference1Address = "Calle 12 # 30-40", Reference2Address = "Carrera 15 # 20-50",
            Zone = "Urbana", Advisor = "Asesor de demostración", SalesPoint = "Sede de prueba", BusinessType = "Crédito",
            VehicleType = "Motocicleta", VehicleBrand = "Marca de ejemplo", VehicleLine = "Línea 125", VehicleVariant = "Estándar",
            VehicleModel = "2026", VehicleColor = "Negro", VehicleEngineCc = "125", VehiclePlate = "Por asignar",
            VehicleChassis = "CHASIS-DEMO-0001", VehicleEngine = "MOTOR-DEMO-0001", VehicleNotes = "Datos ficticios para revisar el formato.",
            ExtraPayment = 500000, ExtraPaymentCount = 2, MonthlyPayment = 295000, Soat = 300000, Registration = 200000, TotalCredit = 5500000,
            Address = "Carrera 25 # 10-20", City = "Montería", Occupation = "Empleado(a)"
        };
        var debtor = new CreditCoDebtorDto(Guid.NewGuid(), "ANA MARÍA EJEMPLO GÓMEZ", "1000000002", "3005550102", "Familiar", 3200000,
            "CARLOS EJEMPLO", "3005550103", "Amigo", "ELENA EJEMPLO", "3005550104", "Familiar", true, details with { Email = "codeudor@example.com" });
        var sample = original with
        {
            Number = "SOL-DEMO-20260924", CreatedAt = new(2026, 9, 24), CustomerName = "JUAN ANDRÉS EJEMPLO PÉREZ",
            IdentificationType = CrmSaas.Domain.Enums.TipoIdentificacionColombia.CedulaCiudadania,
            IdentificationNumber = "1000000001", Mobile = "3005550101", Address = "Calle 20 # 10-15", City = "Montelíbano",
            BirthDate = new(1990, 5, 15), Occupation = "Empleado", MonthlyIncome = 2800000, DownPayment = 1000000,
            MotorcycleValue = 6500000, ProductName = "Motocicleta de demostración 125", TermMonths = 24, FirstDueDate = new(2026, 10, 24),
            Reference1Name = "LUISA EJEMPLO", Reference1Mobile = "3005550105", Reference1Relationship = "Amiga",
            Reference2Name = "PEDRO EJEMPLO", Reference2Mobile = "3005550106", Reference2Relationship = "Hermano",
            Notes = "MUESTRA CON DATOS FICTICIOS. No corresponde a una operación real.", FormDetails = details,
            CoDebtors = [debtor, debtor with { Id = Guid.NewGuid(), Name = "MARIO EJEMPLO LÓPEZ", Identification = "1000000003", Mobile = "3005550107",
                Reference1Name = "ROSA EJEMPLO", Reference1Mobile = "3005550108", Reference2Name = "JORGE EJEMPLO", Reference2Mobile = "3005550109" }]
        };
        var bytes = SimplePdfGenerator.CreditApplication(sample, "EMPRESA DE DEMOSTRACIÓN", "solicitud-credito");
        var text = Decode(bytes);
        foreach (var expected in new[] { sample.CustomerName, debtor.Name, "MARIO EJEMPLO LÓPEZ", "24/10/2026", "CHASIS-DEMO-0001", "AUTORIZACIÓN DE TRATAMIENTO", "FIRMAS Y HUELLAS", "codeudor@example.com", "Calle 12 # 30-40" })
            if (!text.Contains(expected)) throw new Exception("Credit signature PDF missing: " + expected);
        var many = sample with { CoDebtors = Enumerable.Range(1, 8).Select(i => debtor with { Name = "CODEUDOR PRUEBA " + i }).Append(debtor with { Name = "RETIRADO-NO-IMPRIMIR", Active = false }).ToArray(), FormDetails = details with { VehicleNotes = string.Join(" ", Enumerable.Repeat("Detalle extenso", 150)) } };
        var manyBytes = SimplePdfGenerator.CreditApplication(many, "Empresa de prueba", "solicitud-credito", "data:image/png;base64,invalido");
        var manyText = Decode(manyBytes);
        if (manyText.Contains("RETIRADO-NO-IMPRIMIR") || !manyText.Contains("CODEUDOR PRUEBA 8") || !manyText.Contains("CODEUDOR 8 / DEUDOR SOLIDARIO"))
            throw new Exception("All active co-debtors must have identity and signature sections.");
        var empty = SimplePdfGenerator.CreditApplication(sample with { FormDetails = null, CoDebtors = [], Notes = null }, "Empresa", "solicitud-credito");
        if (empty.Length < 1000) throw new Exception("Optional missing details must still print.");
        if (args.Length > 0)
        {
            File.WriteAllBytes(Path.Combine(args[0], "solicitud-credito-muestra.pdf"), bytes);
            File.WriteAllBytes(Path.Combine(args[0], "solicitud-credito-ocho-codeudores.pdf"), manyBytes);
            File.WriteAllBytes(Path.Combine(args[0], "solicitud-credito-sin-detalles.pdf"), empty);
        }
        Console.WriteLine("OK: signature PDF, new form data, active co-debtors, pagination and optional details.");
    }

    private static string Decode(byte[] pdf) => string.Join("\n", Regex.Matches(Encoding.ASCII.GetString(pdf), @"<([0-9A-F]+)> Tj")
        .Select(m => Encoding.Latin1.GetString(Convert.FromHexString(m.Groups[1].Value))));
}
