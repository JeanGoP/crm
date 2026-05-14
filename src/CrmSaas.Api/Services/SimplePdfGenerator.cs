using System.Globalization;
using System.Text;
using CrmSaas.Domain.Common;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Api.Services;

public static class SimplePdfGenerator
{
    private static readonly CultureInfo ColombianCulture = CultureInfo.GetCultureInfo("es-CO");

    public static byte[] Quote(QuoteDto quote, string companyName)
    {
        var lines = new List<string>
        {
            "COTIZACION COMERCIAL",
            $"Numero: {quote.Number}",
            $"Empresa: {companyName}",
            $"Fecha: {Date(quote.QuoteDate)}",
            $"Valida hasta: {Date(quote.ValidUntil)}",
            "",
            "DATOS DEL CLIENTE",
            $"Tipo identificacion: {quote.IdentificationType}",
            $"Identificacion: {Value(quote.IdentificationNumber)}",
            $"Nombres: {quote.CustomerFirstNames}",
            $"Apellidos: {quote.CustomerLastNames}",
            "",
            "PRODUCTO COTIZADO",
            $"Producto: {quote.ProductName}",
            $"Valor comercial: {Money(quote.ProductPrice)}",
            "",
            "SIMULACION DE CREDITO",
            $"Cuota inicial: {Money(quote.DownPayment)}",
            $"Valor financiado: {Money(quote.FinancedAmount)}",
            $"Plazo: {quote.TermMonths} meses",
            $"Tasa mensual: {quote.MonthlyInterestRate:N2}%",
            $"Cuota mensual estimada: {Money(quote.EstimatedMonthlyPayment)}",
            $"Total estimado a pagar: {Money(quote.EstimatedTotalPayment)}",
            "",
            "CONDICIONES",
            "La presente cotizacion es informativa y esta sujeta a validacion comercial, disponibilidad del producto, estudio de credito, aprobacion de la entidad financiadora y politicas internas vigentes.",
            $"Observaciones: {Value(quote.Notes)}",
            "",
            "FIRMAS",
            "Asesor comercial: ______________________________",
            "Cliente: ________________________________________"
        };

        return CreatePdf(lines, $"{quote.Number}.pdf");
    }

    public static byte[] CreditApplication(CreditApplicationDto application, string companyName, string template)
    {
        var normalized = template.Trim().ToLowerInvariant();
        var lines = normalized switch
        {
            "solicitud-credito" => CreditRequest(application, companyName),
            "autorizacion-datos" => DataAuthorization(application, companyName),
            "carta-aprobacion" => ApprovalLetter(application, companyName),
            "orden-entrega" => DeliveryOrder(application, companyName),
            _ => throw new ArgumentOutOfRangeException(nameof(template), "Plantilla no soportada.")
        };

        return CreatePdf(lines, $"{application.Number}-{normalized}.pdf");
    }

    public static string CreditTemplateFileName(CreditApplicationDto application, string template)
    {
        var suffix = template.Trim().ToLowerInvariant() switch
        {
            "solicitud-credito" => "solicitud-credito",
            "autorizacion-datos" => "autorizacion-tratamiento-datos",
            "carta-aprobacion" => "carta-aprobacion",
            "orden-entrega" => "orden-entrega",
            _ => "documento"
        };
        return $"{application.Number}-{suffix}.pdf";
    }

    private static List<string> CreditRequest(CreditApplicationDto app, string companyName) =>
    [
        "SOLICITUD DE CREDITO",
        $"Numero solicitud: {app.Number}",
        $"Empresa: {companyName}",
        $"Fecha generacion: {Date(ColombiaTime.Now)}",
        $"Estado actual: {CreditStatus(app.Status)}",
        "",
        "DATOS DEL SOLICITANTE",
        $"Cliente: {app.CustomerName}",
        $"Tipo identificacion: {app.IdentificationType}",
        $"Identificacion: {app.IdentificationNumber}",
        $"Fecha nacimiento: {Date(app.BirthDate)}",
        $"Celular / WhatsApp: {app.Mobile}",
        $"Direccion: {Value(app.Address)}",
        $"Ciudad: {Value(app.City)}",
        $"Ocupacion: {Value(app.Occupation)}",
        $"Ingresos mensuales: {Money(app.MonthlyIncome)}",
        "",
        "PRODUCTO Y CONDICIONES SOLICITADAS",
        $"Producto: {app.ProductName}",
        $"Valor producto: {Money(app.MotorcycleValue)}",
        $"Cuota inicial: {Money(app.DownPayment)}",
        $"Plazo: {app.TermMonths} meses",
        $"Cotizacion relacionada: {Value(app.QuoteId?.ToString())}",
        "",
        "CODEUDOR",
        $"Nombre: {Value(app.CoDebtorName)}",
        $"Identificacion: {Value(app.CoDebtorIdentification)}",
        $"Celular: {Value(app.CoDebtorMobile)}",
        $"Relacion: {Value(app.CoDebtorRelationship)}",
        $"Ingresos mensuales: {Money(app.CoDebtorMonthlyIncome)}",
        "",
        "REFERENCIAS PERSONALES",
        $"Referencia 1: {Value(app.Reference1Name)} - {Value(app.Reference1Mobile)} - {Value(app.Reference1Relationship)}",
        $"Referencia 2: {Value(app.Reference2Name)} - {Value(app.Reference2Mobile)} - {Value(app.Reference2Relationship)}",
        "",
        "DOCUMENTOS",
        ..app.Documents.OrderBy(x => x.Type).Select(x => $"{x.Name}: {DocumentStatus(x.Status)}"),
        "",
        "OBSERVACIONES",
        Value(app.Notes),
        "",
        "FIRMAS",
        "Solicitante: ____________________________________",
        "Codeudor: _______________________________________",
        "Asesor: _________________________________________"
    ];

    private static List<string> DataAuthorization(CreditApplicationDto app, string companyName) =>
    [
        "AUTORIZACION DE TRATAMIENTO DE DATOS PERSONALES",
        $"Empresa responsable: {companyName}",
        $"Solicitud: {app.Number}",
        $"Fecha: {Date(ColombiaTime.Now)}",
        "",
        "TITULAR",
        $"Nombre: {app.CustomerName}",
        $"Identificacion: {app.IdentificationType} {app.IdentificationNumber}",
        $"Celular: {app.Mobile}",
        "",
        "AUTORIZACION",
        "Autorizo de manera previa, expresa e informada a la empresa responsable para recolectar, almacenar, consultar, usar, circular, actualizar y suprimir mis datos personales con finalidades comerciales, administrativas, contractuales, financieras, de gestion de credito, servicio al cliente y cumplimiento legal.",
        "Autorizo la consulta y verificacion de la informacion suministrada ante centrales de riesgo, entidades financieras, proveedores de informacion, referencias personales, codeudores y terceros autorizados, cuando sea necesario para analizar la solicitud de credito.",
        "Declaro que conozco mi derecho a consultar, actualizar, rectificar y solicitar la supresion de mis datos personales conforme a la normatividad colombiana aplicable.",
        "",
        "ALCANCE",
        $"Producto asociado: {app.ProductName}",
        $"Valor producto: {Money(app.MotorcycleValue)}",
        $"Codeudor registrado: {Value(app.CoDebtorName)}",
        "",
        "FIRMA DEL TITULAR",
        "Nombre: __________________________________________",
        "Identificacion: ___________________________________",
        "Firma: ___________________________________________",
        "Huella: __________________________________________"
    ];

    private static List<string> ApprovalLetter(CreditApplicationDto app, string companyName) =>
    [
        "CARTA DE APROBACION DE CREDITO",
        $"Empresa: {companyName}",
        $"Solicitud: {app.Number}",
        $"Fecha aprobacion: {Date(app.ApprovedAt ?? ColombiaTime.Now)}",
        "",
        "CLIENTE",
        $"Nombre: {app.CustomerName}",
        $"Identificacion: {app.IdentificationType} {app.IdentificationNumber}",
        $"Celular: {app.Mobile}",
        "",
        "CONDICIONES APROBADAS",
        $"Producto: {app.ProductName}",
        $"Valor producto: {Money(app.MotorcycleValue)}",
        $"Cuota inicial: {Money(app.DownPayment)}",
        $"Plazo aprobado: {app.TermMonths} meses",
        $"Ingresos reportados: {Money(app.MonthlyIncome)}",
        $"Codeudor: {Value(app.CoDebtorName)}",
        "",
        "DECISION",
        $"Estado: {CreditStatus(app.Status)}",
        $"Usuario decision: {Value(app.DecisionUser)}",
        $"Observacion: {Value(app.DecisionNotes)}",
        "",
        "CONDICIONES",
        "La aprobacion esta sujeta a validacion final de documentos, firma de contratos, pago de cuota inicial, disponibilidad del producto y cumplimiento de las politicas internas de entrega.",
        "",
        "FIRMAS",
        "Autorizado por: __________________________________",
        "Cliente: _________________________________________"
    ];

    private static List<string> DeliveryOrder(CreditApplicationDto app, string companyName) =>
    [
        "ORDEN DE ENTREGA",
        $"Empresa: {companyName}",
        $"Solicitud: {app.Number}",
        $"Fecha orden: {Date(ColombiaTime.Now)}",
        "",
        "CLIENTE",
        $"Nombre: {app.CustomerName}",
        $"Identificacion: {app.IdentificationType} {app.IdentificationNumber}",
        $"Celular: {app.Mobile}",
        $"Direccion: {Value(app.Address)}",
        $"Ciudad: {Value(app.City)}",
        "",
        "PRODUCTO A ENTREGAR",
        $"Producto: {app.ProductName}",
        $"Valor: {Money(app.MotorcycleValue)}",
        $"Estado credito: {CreditStatus(app.Status)}",
        $"Fecha aprobacion: {Date(app.ApprovedAt)}",
        $"Fecha desembolso: {Date(app.DisbursedAt)}",
        "",
        "CHECKLIST DE ENTREGA",
        "Producto inspeccionado: [  ]",
        "Documentos firmados: [  ]",
        "Pago/cuota inicial confirmado: [  ]",
        "Cliente recibe a satisfaccion: [  ]",
        "",
        "OBSERVACIONES DE ENTREGA",
        "________________________________________________________________",
        "________________________________________________________________",
        "",
        "FIRMAS",
        "Entrega: _________________________________________",
        "Recibe cliente: __________________________________",
        "Cedula: __________________________________________"
    ];

    private static byte[] CreatePdf(IReadOnlyList<string> sourceLines, string title)
    {
        var pages = Paginate(sourceLines);
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            string.Empty,
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var pageObjectNumbers = new List<int>();
        foreach (var page in pages)
        {
            var content = PageContent(page);
            var pageNumber = objects.Count + 1;
            var contentNumber = pageNumber + 1;
            pageObjectNumbers.Add(pageNumber);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentNumber} 0 R >>");
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream");
        }

        objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", pageObjectNumbers.Select(x => $"{x} 0 R"))}] /Count {pageObjectNumbers.Count} >>";

        var pdf = new StringBuilder("%PDF-1.4\n");
        pdf.AppendLine($"% {Escape(title)}");
        var offsets = new List<int> { 0 };
        foreach (var obj in objects.Select((value, index) => new { value, number = index + 1 }))
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.AppendLine($"{obj.number} 0 obj");
            pdf.AppendLine(obj.value);
            pdf.AppendLine("endobj");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.AppendLine("xref");
        pdf.AppendLine($"0 {objects.Count + 1}");
        pdf.AppendLine("0000000000 65535 f ");
        foreach (var offset in offsets.Skip(1)) pdf.AppendLine($"{offset:0000000000} 00000 n ");
        pdf.AppendLine("trailer");
        pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        pdf.AppendLine("startxref");
        pdf.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
        pdf.AppendLine("%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static List<List<string>> Paginate(IReadOnlyList<string> sourceLines)
    {
        var wrapped = sourceLines.SelectMany(Wrap).ToList();
        const int linesPerPage = 31;
        var pages = new List<List<string>>();
        for (var i = 0; i < wrapped.Count; i += linesPerPage)
        {
            pages.Add(wrapped.Skip(i).Take(linesPerPage).ToList());
        }
        return pages.Count == 0 ? [[]] : pages;
    }

    private static IEnumerable<string> Wrap(string line)
    {
        const int max = 92;
        if (string.IsNullOrWhiteSpace(line))
        {
            yield return string.Empty;
            yield break;
        }

        var current = line.Trim();
        while (current.Length > max)
        {
            var split = current.LastIndexOf(' ', max);
            if (split <= 0) split = max;
            yield return current[..split].TrimEnd();
            current = current[split..].TrimStart();
        }
        yield return current;
    }

    private static string PageContent(IReadOnlyList<string> lines)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 11 Tf");
        content.AppendLine("50 760 Td");
        foreach (var line in lines)
        {
            if (IsHeading(line)) content.AppendLine("/F1 14 Tf");
            content.AppendLine($"({Escape(line)}) Tj");
            content.AppendLine("0 -22 Td");
            if (IsHeading(line)) content.AppendLine("/F1 11 Tf");
        }
        content.AppendLine("ET");
        return content.ToString();
    }

    private static bool IsHeading(string line) =>
        !string.IsNullOrWhiteSpace(line) &&
        line.Length <= 44 &&
        line.All(c => !char.IsLetter(c) || char.IsUpper(c)) &&
        !line.Contains(':');

    private static string Money(decimal? value) => value.HasValue ? value.Value.ToString("C0", ColombianCulture) : "N/A";
    private static string Date(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "N/A";
    private static string Value(string? value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();

    private static string CreditStatus(EstadoSolicitudCredito status) => status switch
    {
        EstadoSolicitudCredito.Borrador => "Borrador",
        EstadoSolicitudCredito.DocumentosPendientes => "Documentos pendientes",
        EstadoSolicitudCredito.DocumentosRecibidos => "Documentos recibidos",
        EstadoSolicitudCredito.EnEstudio => "En estudio",
        EstadoSolicitudCredito.Aprobada => "Aprobada",
        EstadoSolicitudCredito.Rechazada => "Rechazada",
        EstadoSolicitudCredito.Desembolsada => "Desembolsada",
        _ => status.ToString()
    };

    private static string DocumentStatus(EstadoDocumentoCredito status) => status switch
    {
        EstadoDocumentoCredito.Pendiente => "Pendiente",
        EstadoDocumentoCredito.Recibido => "Recibido",
        EstadoDocumentoCredito.Validado => "Validado",
        EstadoDocumentoCredito.Rechazado => "Rechazado",
        _ => status.ToString()
    };

    private static string Escape(string value) => value
        .Replace("\\", "\\\\")
        .Replace("(", "\\(")
        .Replace(")", "\\)")
        .Normalize(NormalizationForm.FormD)
        .Where(c => c < 128)
        .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c), sb => sb.ToString());
}
