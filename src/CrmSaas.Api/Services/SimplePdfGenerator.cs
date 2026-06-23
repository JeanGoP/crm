using System.Globalization;
using System.IO.Compression;
using System.Text;
using CrmSaas.Domain.Common;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Api.Services;

public sealed record QuotePdfImage(byte[] Data, string ContentType, string FileName);

public static class SimplePdfGenerator
{
    private static readonly CultureInfo ColombianCulture = CultureInfo.GetCultureInfo("es-CO");

    public static byte[] Quote(QuoteDto quote, string companyName, QuotePdfImage? productImage = null, QuotePdfImage? companyLogo = null, string? customerPhone = null, string? customerAddress = null, string? advisor = null)
    {
        return CreateQuotePdf(quote, companyName, productImage, companyLogo, customerPhone, customerAddress, advisor);
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
        $"Resultado estudio: {Value(app.StudyResult)}",
        $"Valor aprobado: {Money(app.AnalystApprovedAmount ?? app.MotorcycleValue)}",
        $"Cuota inicial aprobada: {Money(app.ApprovedDownPayment ?? app.DownPayment)}",
        $"Plazo aprobado: {app.ApprovedTermMonths ?? app.TermMonths} meses",
        $"Cuota mensual aprobada: {(app.ApprovedMonthlyPayment.HasValue ? Money(app.ApprovedMonthlyPayment.Value) : "Pendiente")}",
        $"Ingresos reportados: {Money(app.MonthlyIncome)}",
        $"Codeudor: {Value(app.CoDebtorName)}",
        $"Aprobacion condicionada a codeudor: {(app.RequiresCoDebtorForApproval ? "Si" : "No")}",
        "",
        "DECISION",
        $"Estado: {CreditStatus(app.Status)}",
        $"Usuario decision: {Value(app.DecisionUser)}",
        $"Observacion: {Value(app.DecisionNotes)}",
        $"Paso 0 RUNT/SIMIT: {(app.RuntChecked && app.SimitChecked ? "Consultado" : "Pendiente")}",
        $"Identidad validada: {(app.IdentityValidated ? "Si" : "No")}",
        "",
        "CONDICIONES FINALES",
        string.IsNullOrWhiteSpace(app.FinalConditions)
            ? "La aprobacion esta sujeta a validacion final de documentos, firma de contratos, pago de cuota inicial, disponibilidad del producto y cumplimiento de las politicas internas de entrega."
            : app.FinalConditions,
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
    private static string MoneyPlain(decimal? value) => value.HasValue ? value.Value.ToString("N0", ColombianCulture) : "N/A";
    private static string Date(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "N/A";
    private static string Value(string? value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();
    private static string Value(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static decimal EstimatePaymentForTerm(decimal financedAmount, int termMonths, decimal monthlyInterestRate)
    {
        if (financedAmount <= 0 || termMonths <= 0) return 0;
        var rate = monthlyInterestRate / 100m;
        if (rate <= 0) return Math.Round(financedAmount / termMonths, 0);

        var rateDouble = (double)rate;
        var amountDouble = (double)financedAmount;
        var payment = amountDouble * rateDouble / (1 - Math.Pow(1 + rateDouble, -termMonths));
        return Math.Round((decimal)payment, 0);
    }

    private static string CreditStatus(EstadoSolicitudCredito status) => status switch
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

    private static byte[] CreateQuotePdf(QuoteDto quote, string companyName, QuotePdfImage? productImage, QuotePdfImage? companyLogo, string? customerPhone, string? customerAddress, string? advisor)
    {
        var logoImage = TryCreatePdfImage(companyLogo);
        var pdfImage = TryCreatePdfImage(productImage);

        var content = QuotePageContent(quote, companyName, logoImage is not null, pdfImage is not null, customerPhone, customerAddress, advisor);
        var objects = new List<PdfObject>
        {
            new("<< /Type /Catalog /Pages 2 0 R >>"),
            new(string.Empty),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>")
        };

        var xObjects = new List<string>();
        AddImageObject(objects, xObjects, "Logo", logoImage);
        AddImageObject(objects, xObjects, "Product", pdfImage);

        var pageNumber = objects.Count + 1;
        var contentNumber = pageNumber + 1;
        var xObjectResources = xObjects.Count > 0 ? $" /XObject << {string.Join(" ", xObjects)} >>" : string.Empty;
        var resources = $"<< /Font << /F1 3 0 R /F2 4 0 R >>{xObjectResources} >>";
        objects.Add(new($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources {resources} /Contents {contentNumber} 0 R >>"));
        objects.Add(new($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>", Encoding.ASCII.GetBytes(content)));
        objects[1] = new($"<< /Type /Pages /Kids [{pageNumber} 0 R] /Count 1 >>");

        return BuildPdf(objects, $"{quote.Number}.pdf");
    }

    private static void AddImageObject(List<PdfObject> objects, List<string> xObjects, string name, PdfImageData? image)
    {
        if (image is null) return;

        var objectNumber = objects.Count + 1;
        var decodeParms = string.IsNullOrWhiteSpace(image.DecodeParms) ? string.Empty : $" /DecodeParms {image.DecodeParms}";
        objects.Add(new($"<< /Type /XObject /Subtype /Image /Width {image.Width} /Height {image.Height} /ColorSpace /{image.ColorSpace} /BitsPerComponent 8 /Filter /{image.Filter}{decodeParms} /Length {image.Data.Length} >>", image.Data));
        xObjects.Add($"/{name} {objectNumber} 0 R");
    }

    private static string QuotePageContent(QuoteDto quote, string companyName, bool includeLogo, bool includeProductImage, string? customerPhone, string? customerAddress, string? advisor)
    {
        var customerName = $"{quote.CustomerFirstName} {quote.CustomerMiddleName} {quote.CustomerLastName} {quote.CustomerSecondLastName}"
            .Replace("  ", " ")
            .Trim();
        if (string.IsNullOrWhiteSpace(customerName)) customerName = $"{quote.CustomerFirstNames} {quote.CustomerLastNames}".Trim();

        var commands = new StringBuilder();
        commands.AppendLine("0.98 0.99 1 rg 0 0 612 792 re f");
        commands.AppendLine("1 1 1 rg 30 30 552 732 re f");
        commands.AppendLine("0.84 0.88 0.92 RG 1 w 30 30 552 732 re S");

        commands.AppendLine("0.082 0.373 0.459 rg 30 705 552 57 re f");
        if (includeLogo)
        {
            commands.AppendLine("q 112 0 0 48 46 711 cm /Logo Do Q");
        }
        else
        {
            commands.AppendLine($"1 1 1 rg BT /F2 18 Tf 48 735 Td ({Escape(companyName)}) Tj ET");
        }
        commands.AppendLine($"1 1 1 rg BT /F2 12 Tf 392 738 Td ({Escape(quote.Number)}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F1 9 Tf 392 722 Td (Fecha: {Escape(Date(quote.QuoteDate))}) Tj ET");

        DrawPanel(commands, 46, 575, 250, 110, "CLIENTE");
        KeyValue(commands, 62, 653, "Nombre", customerName, 70, 166);
        KeyValue(commands, 62, 631, "Identificacion", $"{IdentificationType(quote.IdentificationType)} {Value(quote.IdentificationNumber)}", 70, 166);
        KeyValue(commands, 62, 609, "Telefono", Value(customerPhone), 70, 166);
        KeyValue(commands, 62, 587, "Direccion", Value(customerAddress), 70, 166);

        DrawPanel(commands, 316, 575, 250, 110, "PRODUCTO");
        KeyValue(commands, 332, 653, "Producto", quote.ProductName, 74, 156);
        KeyValue(commands, 332, 631, "Precio base", Money(quote.ProductPrice), 74, 156);
        KeyValue(commands, 332, 609, "Descuento", quote.PromotionDiscount > 0 ? Money(quote.PromotionDiscount) : "N/A", 74, 156);
        KeyValue(commands, 332, 587, "Precio final", Money(quote.DiscountedProductPrice), 74, 156);

        if (includeProductImage)
        {
            commands.AppendLine("q 138 0 0 82 237 480 cm /Product Do Q");
        }

        var quoteItems = quote.Items.Count > 0 ? quote.Items.OrderBy(x => x.Order).ToList() : [];
        if (quoteItems.Count > 1)
        {
            DrawComparison(commands, 46, 332, quoteItems);
        }
        else
        {
            DrawPanel(commands, 46, 332, 250, 214, "CUOTAS APROXIMADAS");
            KeyValue(commands, 62, 512, "Precio final", Money(quote.DiscountedProductPrice), 82, 140);
            KeyValue(commands, 62, 490, "Cuota inicial", Money(quote.DownPayment), 82, 140);
            if (quote.PromotionDiscount > 0)
            {
                KeyValue(commands, 62, 468, "Promocion", Shorten(Value(quote.PromotionName), 22), 82, 140);
            }
            commands.AppendLine("0.90 0.94 0.96 rg 62 458 216 22 re f");
            commands.AppendLine("0.09 0.11 0.15 rg BT /F2 8 Tf 76 466 Td (PLAZO) Tj ET");
            commands.AppendLine("0.09 0.11 0.15 rg BT /F2 8 Tf 154 466 Td (CUOTA) Tj ET");
            var terms = new[] { 1, 6, 12, 18, 24, 30, 36 };
            var termY = 440;
            foreach (var term in terms)
            {
                var payment = term == quote.TermMonths ? quote.EstimatedMonthlyPayment : EstimatePaymentForTerm(quote.FinancedAmount, term, quote.MonthlyInterestRate);
                var isSelected = term == quote.TermMonths;
                if (isSelected) commands.AppendLine($"0.90 0.97 0.94 rg 62 {termY - 5} 216 20 re f");
                commands.AppendLine($"0.08 0.10 0.14 rg BT /F{(isSelected ? "2" : "1")} 10 Tf 80 {termY} Td ({term} meses) Tj ET");
                commands.AppendLine($"0.08 0.10 0.14 rg BT /F{(isSelected ? "2" : "1")} 10 Tf 154 {termY} Td ({Escape(Money(payment))}) Tj ET");
                termY -= 22;
            }

            DrawPanel(commands, 316, 332, 250, 214, "RESUMEN DEL CREDITO");
            var creditBase = Math.Max(quote.DiscountedProductPrice - quote.DownPayment, 0);
            CreditRow(commands, 332, 512, "Valor a financiar", Money(creditBase));
            CreditRow(commands, 332, 488, "SOAT / Seguro", Money(quote.Insurance));
            CreditRow(commands, 332, 464, "Gastos / Matricula", Money(quote.AdministrativeFees));
            CreditRow(commands, 332, 440, "Otros", Money(0));
            commands.AppendLine("0.082 0.373 0.459 rg 332 396 218 38 re f");
            commands.AppendLine($"1 1 1 rg BT /F2 10 Tf 346 419 Td (Total credito) Tj ET");
            commands.AppendLine($"1 1 1 rg BT /F2 12 Tf 446 419 Td ({Escape(Money(quote.FinancedAmount))}) Tj ET");
            commands.AppendLine($"1 1 1 rg BT /F1 9 Tf 346 404 Td (Cuota seleccionada: {quote.TermMonths} meses - {Escape(Money(quote.EstimatedMonthlyPayment))}) Tj ET");
            KeyValue(commands, 332, 370, "Total estimado", Money(quote.EstimatedTotalPayment), 88, 130);
            KeyValue(commands, 332, 350, "Marca / tasa", $"{Value(quote.SalesPointBrand, "Marca")} - {quote.MonthlyInterestRate:N3}%", 88, 130);
        }

        DrawPanel(commands, 46, 190, 520, 112, "REQUISITOS GENERALES");
        RequirementColumn(commands, 62, 266, "Empleados", new[] { "Fotocopia Cedula", "Carta laboral o dos ultimas colillas", "Recibo de servicio publico" });
        RequirementColumn(commands, 232, 266, "Independientes", new[] { "Fotocopia Cedula", "Certificado de ingresos", "Camara de comercio o extractos" });
        RequirementColumn(commands, 402, 266, "Pensionados", new[] { "Fotocopia Cedula", "Dos ultimas colillas", "Recibo de servicio publico" });

        DrawPanel(commands, 46, 98, 250, 64, "ASESOR");
        KeyValue(commands, 62, 132, "Nombre", Value(advisor, "Asesor comercial"), 52, 166);
        KeyValue(commands, 62, 112, "Contacto", Value(customerPhone), 52, 166);

        DrawPanel(commands, 316, 98, 250, 64, "OBSERVACIONES");
        Paragraph(commands, 332, 134, Value(quote.Notes, Value(quote.SalesPointCommercialTerms, "Cotizacion sujeta a aprobacion final y disponibilidad del producto.")), 46, 8, 3);

        var legal = "Autorizacion de tratamiento de datos: con la firma o aceptacion de esta cotizacion, el cliente autoriza el uso de sus datos para gestion comercial, estudio de credito, seguimiento, cobranza e informacion relacionada con productos y servicios.";
        Paragraph(commands, 46, 72, legal, 116, 7, 2);
        return commands.ToString();
    }

    private static void LabelValue(StringBuilder commands, int x, int y, string label, string value, int labelSize, int valueSize, int valueWidth)
    {
        commands.AppendLine($"0.09 0.09 0.09 rg BT /F2 {labelSize} Tf {x} {y} Td ({Escape(label)}) Tj ET");
        commands.AppendLine($"0.09 0.09 0.09 rg BT /F1 {valueSize} Tf {x + valueWidth} {y} Td ({Escape(Shorten(value, 28))}) Tj ET");
    }

    private static void DrawPanel(StringBuilder commands, int x, int y, int width, int height, string title)
    {
        commands.AppendLine($"1 1 1 rg {x} {y} {width} {height} re f");
        commands.AppendLine($"0.84 0.88 0.92 RG 0.8 w {x} {y} {width} {height} re S");
        commands.AppendLine($"0.082 0.373 0.459 rg BT /F2 10 Tf {x + 14} {y + height - 20} Td ({Escape(title)}) Tj ET");
    }

    private static void DrawComparison(StringBuilder commands, int x, int y, IReadOnlyCollection<QuoteItemDto> items)
    {
        DrawPanel(commands, x, y, 520, 214, "COMPARATIVO DE ARTICULOS");
        commands.AppendLine($"0.90 0.94 0.96 rg {x + 16} {y + 158} 488 24 re f");
        var headers = new[] { "Producto", "Precio", "Inicial", "Financiado", "Plazo", "Cuota" };
        var columns = new[] { x + 24, x + 160, x + 232, x + 304, x + 386, x + 438 };
        for (var i = 0; i < headers.Length; i++)
        {
            commands.AppendLine($"0.09 0.11 0.15 rg BT /F2 7 Tf {columns[i]} {y + 168} Td ({headers[i]}) Tj ET");
        }

        var rowY = y + 138;
        foreach (var item in items.Take(4))
        {
            commands.AppendLine($"0.84 0.88 0.92 RG 0.4 w {x + 16} {rowY - 8} 488 24 re S");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 8 Tf {columns[0]} {rowY} Td ({Escape(Shorten(item.ProductName, 26))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[1]} {rowY} Td ({Escape(Money(item.DiscountedProductPrice))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[2]} {rowY} Td ({Escape(Money(item.DownPayment))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[3]} {rowY} Td ({Escape(Money(item.FinancedAmount))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[4]} {rowY} Td ({item.TermMonths}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 8 Tf {columns[5]} {rowY} Td ({Escape(Money(item.EstimatedMonthlyPayment))}) Tj ET");
            rowY -= 30;
        }

        var bestPayment = items.OrderBy(x => x.EstimatedMonthlyPayment).First();
        var bestPrice = items.OrderBy(x => x.DiscountedProductPrice).First();
        commands.AppendLine("0.082 0.373 0.459 rg " + (x + 16) + " " + (y + 18) + " 488 34 re f");
        commands.AppendLine($"1 1 1 rg BT /F2 9 Tf {x + 30} {y + 38} Td (Menor cuota: {Escape(Shorten(bestPayment.ProductName, 28))} - {Escape(Money(bestPayment.EstimatedMonthlyPayment))}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F1 8 Tf {x + 30} {y + 24} Td (Menor precio: {Escape(Shorten(bestPrice.ProductName, 28))} - {Escape(Money(bestPrice.DiscountedProductPrice))}) Tj ET");
    }

    private static void KeyValue(StringBuilder commands, int x, int y, string label, string value, int labelWidth, int valueMaxChars)
    {
        commands.AppendLine($"0.36 0.42 0.48 rg BT /F1 8 Tf {x} {y} Td ({Escape(label.ToUpperInvariant())}) Tj ET");
        commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 9 Tf {x + labelWidth} {y} Td ({Escape(Shorten(value, valueMaxChars))}) Tj ET");
    }

    private static void CreditRow(StringBuilder commands, int x, int y, string label, string value)
    {
        commands.AppendLine($"0.36 0.42 0.48 rg BT /F1 8 Tf {x} {y} Td ({Escape(label.ToUpperInvariant())}) Tj ET");
        commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 9 Tf {x + 122} {y} Td ({Escape(Shorten(value, 18))}) Tj ET");
    }

    private static void RequirementColumn(StringBuilder commands, int x, int y, string title, IReadOnlyCollection<string> lines)
    {
        commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 9 Tf {x} {y} Td ({Escape(title)}) Tj ET");
        var lineY = y - 18;
        foreach (var line in lines)
        {
            commands.AppendLine($"0.22 0.26 0.32 rg BT /F1 7 Tf {x} {lineY} Td ({Escape(Shorten(line, 34))}) Tj ET");
            lineY -= 13;
        }
    }

    private static void RequirementBlock(StringBuilder commands, int x, int y, string title, IReadOnlyCollection<string> lines)
    {
        commands.AppendLine($"0.08 0.08 0.08 rg BT /F2 10 Tf {x} {y} Td ({Escape(title)}) Tj ET");
        var lineY = y - 18;
        foreach (var line in lines)
        {
            commands.AppendLine($"0.10 0.10 0.10 rg BT /F1 8 Tf {x} {lineY} Td ({Escape(line)}) Tj ET");
            lineY -= 13;
        }
    }

    private static void DrawSection(StringBuilder commands, int x, int y, int width, int height, string title)
    {
        commands.AppendLine("1 1 1 rg " + x + " " + y + " " + width + " " + height + " re f");
        commands.AppendLine("0.86 0.89 0.93 RG 1 w " + x + " " + y + " " + width + " " + height + " re S");
        commands.AppendLine("0.082 0.373 0.459 rg BT /F2 11 Tf " + (x + 16) + " " + (y + height - 24) + " Td (" + Escape(title) + ") Tj ET");
    }

    private static void Text(StringBuilder commands, int x, int y, string label, string value)
    {
        commands.AppendLine("0.40 0.45 0.52 rg BT /F1 8 Tf " + x + " " + y + " Td (" + Escape(label.ToUpperInvariant()) + ") Tj ET");
        commands.AppendLine("0.08 0.10 0.14 rg BT /F2 10 Tf " + x + " " + (y - 13) + " Td (" + Escape(Shorten(value, 37)) + ") Tj ET");
    }

    private static void FinancialRow(StringBuilder commands, int x, int y, string label1, string value1, string label2, string value2, string label3, string value3)
    {
        Text(commands, x, y, label1, value1);
        Text(commands, x + 170, y, label2, value2);
        Text(commands, x + 350, y, label3, value3);
    }

    private static void Paragraph(StringBuilder commands, int x, int y, string value, int maxChars, int fontSize, int maxLines = 3)
    {
        var lineY = y;
        foreach (var line in WrapText(value, maxChars).Take(maxLines))
        {
            commands.AppendLine($"0.16 0.20 0.27 rg BT /F1 {fontSize} Tf {x} {lineY} Td ({Escape(line)}) Tj ET");
            lineY -= 16;
        }
    }

    private static IEnumerable<string> WrapText(string value, int maxChars)
    {
        var current = value.Trim();
        while (current.Length > maxChars)
        {
            var split = current.LastIndexOf(' ', maxChars);
            if (split <= 0) split = maxChars;
            yield return current[..split].TrimEnd();
            current = current[split..].TrimStart();
        }
        if (!string.IsNullOrWhiteSpace(current)) yield return current;
    }

    private static string Shorten(string value, int max) => value.Length <= max ? value : value[..Math.Max(0, max - 1)] + "…";

    private static bool IsJpeg(QuotePdfImage? image) =>
        image is not null &&
        (image.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
         image.ContentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)) &&
        image.Data.Length > 4 &&
        image.Data[0] == 0xFF &&
        image.Data[1] == 0xD8;

    private sealed record PdfImageData(byte[] Data, int Width, int Height, string ColorSpace, string Filter, string? DecodeParms);

    private static PdfImageData? TryCreatePdfImage(QuotePdfImage? image)
    {
        if (IsJpeg(image))
        {
            var size = TryReadJpegSize(image!.Data);
            return size is null
                ? null
                : new PdfImageData(image.Data, size.Value.Width, size.Value.Height, "DeviceRGB", "DCTDecode", null);
        }

        if (image is not null && image.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
        {
            return TryCreatePngPdfImage(image.Data);
        }

        return null;
    }

    private static PdfImageData? TryCreatePngPdfImage(byte[] data)
    {
        if (data.Length < 33 || !data.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return null;

        var offset = 8;
        int width = 0, height = 0, bitDepth = 0, colorType = 0, interlace = 0;
        byte[]? palette = null;
        byte[]? transparency = null;
        var compressed = new MemoryStream();

        while (offset + 8 <= data.Length)
        {
            var length = ReadBigEndianInt(data, offset);
            var type = Encoding.ASCII.GetString(data, offset + 4, 4);
            var chunkStart = offset + 8;
            if (length < 0 || chunkStart + length > data.Length) return null;

            if (type == "IHDR")
            {
                width = ReadBigEndianInt(data, chunkStart);
                height = ReadBigEndianInt(data, chunkStart + 4);
                bitDepth = data[chunkStart + 8];
                colorType = data[chunkStart + 9];
                interlace = data[chunkStart + 12];
            }
            else if (type == "IDAT")
            {
                compressed.Write(data, chunkStart, length);
            }
            else if (type == "PLTE")
            {
                palette = data.Skip(chunkStart).Take(length).ToArray();
            }
            else if (type == "tRNS")
            {
                transparency = data.Skip(chunkStart).Take(length).ToArray();
            }
            else if (type == "IEND")
            {
                break;
            }

            offset = chunkStart + length + 4;
        }

        if (width <= 0 || height <= 0 || bitDepth != 8 || interlace != 0 || compressed.Length == 0) return null;

        var sourceChannels = colorType switch
        {
            0 => 1,
            2 => 3,
            3 => 1,
            4 => 2,
            6 => 4,
            _ => 0
        };
        if (sourceChannels == 0) return null;
        if (colorType == 3 && (palette is null || palette.Length < 3)) return null;

        compressed.Position = 0;
        using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        zlib.CopyTo(raw);
        var rawBytes = raw.ToArray();
        var stride = width * sourceChannels;
        var expected = (stride + 1) * height;
        if (rawBytes.Length < expected) return null;

        var unfiltered = UnfilterPng(rawBytes, width, height, sourceChannels);
        const int targetChannels = 3;
        using var imageRows = new MemoryStream();
        for (var row = 0; row < height; row++)
        {
            imageRows.WriteByte(0);
            var rowStart = row * stride;
            for (var col = 0; col < width; col++)
            {
                var pixelStart = rowStart + col * sourceChannels;
                var (r, g, b) = PngPixelToRgb(unfiltered, pixelStart, colorType, palette, transparency);
                imageRows.WriteByte(r);
                imageRows.WriteByte(g);
                imageRows.WriteByte(b);
            }
        }

        using var encoded = new MemoryStream();
        using (var compressor = new ZLibStream(encoded, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            imageRows.Position = 0;
            imageRows.CopyTo(compressor);
        }

        var decodeParms = $"<< /Predictor 15 /Colors {targetChannels} /BitsPerComponent 8 /Columns {width} >>";
        return new PdfImageData(encoded.ToArray(), width, height, "DeviceRGB", "FlateDecode", decodeParms);
    }

    private static (byte R, byte G, byte B) PngPixelToRgb(byte[] data, int pixelStart, int colorType, byte[]? palette, byte[]? transparency)
    {
        return colorType switch
        {
            0 => (data[pixelStart], data[pixelStart], data[pixelStart]),
            2 => (data[pixelStart], data[pixelStart + 1], data[pixelStart + 2]),
            3 => PalettePixelToRgb(data[pixelStart], palette!, transparency),
            4 => BlendWithWhite(data[pixelStart], data[pixelStart], data[pixelStart], data[pixelStart + 1]),
            6 => BlendWithWhite(data[pixelStart], data[pixelStart + 1], data[pixelStart + 2], data[pixelStart + 3]),
            _ => (255, 255, 255)
        };
    }

    private static (byte R, byte G, byte B) PalettePixelToRgb(byte index, byte[] palette, byte[]? transparency)
    {
        var paletteIndex = index * 3;
        if (paletteIndex + 2 >= palette.Length) return (255, 255, 255);
        var alpha = transparency is not null && index < transparency.Length ? transparency[index] : (byte)255;
        return BlendWithWhite(palette[paletteIndex], palette[paletteIndex + 1], palette[paletteIndex + 2], alpha);
    }

    private static (byte R, byte G, byte B) BlendWithWhite(byte r, byte g, byte b, byte alpha)
    {
        if (alpha == 255) return (r, g, b);
        if (alpha == 0) return (255, 255, 255);
        return (
            (byte)((r * alpha + 255 * (255 - alpha)) / 255),
            (byte)((g * alpha + 255 * (255 - alpha)) / 255),
            (byte)((b * alpha + 255 * (255 - alpha)) / 255)
        );
    }

    private static byte[] UnfilterPng(byte[] rawBytes, int width, int height, int channels)
    {
        var stride = width * channels;
        var output = new byte[stride * height];
        var input = 0;

        for (var row = 0; row < height; row++)
        {
            var filter = rawBytes[input++];
            var current = row * stride;
            var previous = current - stride;
            for (var col = 0; col < stride; col++)
            {
                var raw = rawBytes[input++];
                var left = col >= channels ? output[current + col - channels] : 0;
                var up = row > 0 ? output[previous + col] : 0;
                var upperLeft = row > 0 && col >= channels ? output[previous + col - channels] : 0;
                var value = filter switch
                {
                    0 => raw,
                    1 => raw + left,
                    2 => raw + up,
                    3 => raw + ((left + up) / 2),
                    4 => raw + Paeth(left, up, upperLeft),
                    _ => raw
                };
                output[current + col] = unchecked((byte)value);
            }
        }

        return output;
    }

    private static int Paeth(int left, int up, int upperLeft)
    {
        var p = left + up - upperLeft;
        var pa = Math.Abs(p - left);
        var pb = Math.Abs(p - up);
        var pc = Math.Abs(p - upperLeft);
        if (pa <= pb && pa <= pc) return left;
        return pb <= pc ? up : upperLeft;
    }

    private static int ReadBigEndianInt(byte[] data, int offset) =>
        (data[offset] << 24) + (data[offset + 1] << 16) + (data[offset + 2] << 8) + data[offset + 3];

    private static (int Width, int Height)? TryReadJpegSize(byte[] data)
    {
        var index = 2;
        while (index + 9 < data.Length)
        {
            if (data[index] != 0xFF) { index++; continue; }
            var marker = data[index + 1];
            var length = (data[index + 2] << 8) + data[index + 3];
            if (length < 2 || index + length + 2 > data.Length) return null;
            if (marker is >= 0xC0 and <= 0xC3 or >= 0xC5 and <= 0xC7 or >= 0xC9 and <= 0xCB or >= 0xCD and <= 0xCF)
            {
                var height = (data[index + 5] << 8) + data[index + 6];
                var width = (data[index + 7] << 8) + data[index + 8];
                return (width, height);
            }
            index += length + 2;
        }
        return null;
    }

    private sealed record PdfObject(string Header, byte[]? Stream = null);

    private static byte[] BuildPdf(IReadOnlyList<PdfObject> objects, string title)
    {
        using var output = new MemoryStream();
        WriteAscii(output, "%PDF-1.4\n");
        WriteAscii(output, $"% {Escape(title)}\n");
        var offsets = new List<long> { 0 };

        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(output.Position);
            WriteAscii(output, $"{i + 1} 0 obj\n");
            WriteAscii(output, objects[i].Header);
            if (objects[i].Stream is not null)
            {
                WriteAscii(output, "\nstream\n");
                output.Write(objects[i].Stream);
                WriteAscii(output, "\nendstream");
            }
            WriteAscii(output, "\nendobj\n");
        }

        var xrefOffset = output.Position;
        WriteAscii(output, "xref\n");
        WriteAscii(output, $"0 {objects.Count + 1}\n");
        WriteAscii(output, "0000000000 65535 f \n");
        foreach (var offset in offsets.Skip(1)) WriteAscii(output, $"{offset:0000000000} 00000 n \n");
        WriteAscii(output, "trailer\n");
        WriteAscii(output, $"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        WriteAscii(output, "startxref\n");
        WriteAscii(output, xrefOffset.ToString(CultureInfo.InvariantCulture));
        WriteAscii(output, "\n%%EOF");
        return output.ToArray();
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string IdentificationType(TipoIdentificacionColombia type) => type switch
    {
        TipoIdentificacionColombia.CedulaCiudadania => "CC",
        TipoIdentificacionColombia.CedulaExtranjeria => "CE",
        TipoIdentificacionColombia.Nit => "NIT",
        TipoIdentificacionColombia.Pasaporte => "Pasaporte",
        TipoIdentificacionColombia.TarjetaIdentidad => "TI",
        TipoIdentificacionColombia.PermisoProteccionTemporal => "PPT",
        _ => type.ToString()
    };
}
