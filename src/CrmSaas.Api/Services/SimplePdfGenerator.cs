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

    public static byte[] Quote(QuoteDto quote, string companyName, QuotePdfImage? productImage = null, QuotePdfImage? companyLogo = null, QuotePdfImage? brandLogo = null, string? customerPhone = null, string? customerAddress = null, string? advisor = null)
    {
        return CreateQuotePdf(quote, companyName, productImage, companyLogo, brandLogo, customerPhone, customerAddress, advisor);
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

    private static byte[] CreateQuotePdf(QuoteDto quote, string companyName, QuotePdfImage? productImage, QuotePdfImage? companyLogo, QuotePdfImage? brandLogo, string? customerPhone, string? customerAddress, string? advisor)
    {
        var logoImage = TryCreatePdfImage(companyLogo);
        var brandLogoImage = TryCreatePdfImage(brandLogo);
        var pdfImage = TryCreatePdfImage(productImage);

        var content = QuotePageContent(quote, companyName, logoImage is not null, brandLogoImage is not null, pdfImage is not null, customerPhone, customerAddress, advisor);
        var objects = new List<PdfObject>
        {
            new("<< /Type /Catalog /Pages 2 0 R >>"),
            new(string.Empty),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>")
        };

        var xObjects = new List<string>();
        AddImageObject(objects, xObjects, "Logo", logoImage);
        AddImageObject(objects, xObjects, "BrandLogo", brandLogoImage);
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

    private static string QuotePageContent(QuoteDto quote, string companyName, bool includeCompanyLogo, bool includeBrandLogo, bool includeProductImage, string? customerPhone, string? customerAddress, string? advisor)
    {
        var customerName = $"{quote.CustomerFirstName} {quote.CustomerMiddleName} {quote.CustomerLastName} {quote.CustomerSecondLastName}"
            .Replace("  ", " ")
            .Trim();
        if (string.IsNullOrWhiteSpace(customerName)) customerName = $"{quote.CustomerFirstNames} {quote.CustomerLastNames}".Trim();

        var commands = new StringBuilder();
        commands.AppendLine("1 1 1 rg 0 0 612 792 re f");
        commands.AppendLine("0.06 0.06 0.06 RG 0.8 w 40 28 532 736 re S");

        if (includeCompanyLogo)
        {
            commands.AppendLine("q 126 0 0 56 48 685 cm /Logo Do Q");
        }
        else
        {
            commands.AppendLine($"0.06 0.06 0.06 rg BT /F2 16 Tf 48 714 Td ({Escape(Shorten(companyName, 22))}) Tj ET");
        }

        CenterText(commands, 306, 728, companyName.ToUpperInvariant(), 14, true);
        CenterText(commands, 306, 711, Value(quote.SalesPointName, "Sede principal"), 10, true);
        CenterText(commands, 306, 696, Value(quote.SalesPointCommercialTerms, "Cotizacion comercial"), 9, false, 44);

        if (includeBrandLogo)
        {
            commands.AppendLine("q 96 0 0 58 466 682 cm /BrandLogo Do Q");
        }
        else
        {
            CenterText(commands, 514, 706, Value(quote.SalesPointBrand, "MARCA"), 18, true);
        }

        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 10 Tf 45 650 Td (Fecha: {Escape(Date(quote.QuoteDate))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 8 Tf 412 660 Td (Cotizacion valida hasta {Escape(Date(quote.ValidUntil))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 12 Tf 418 646 Td (#{Escape(quote.Number)}) Tj ET");

        DrawRoundedLikeBox(commands, 45, 608, 522, 34);
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 12 Tf 60 629 Td (Asesor Comercial:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 60 615 Td ({Escape(Shorten(Value(advisor, "Asesor comercial"), 34))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 10 Tf 300 629 Td (Correo:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 345 629 Td ({Escape(Shorten(Value(advisor), 31))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 10 Tf 300 615 Td (Cel:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 345 615 Td ({Escape(Value(customerPhone))}) Tj ET");

        if (includeProductImage)
        {
            commands.AppendLine("q 230 0 0 150 58 422 cm /Product Do Q");
        }
        else
        {
            commands.AppendLine("0.93 0.95 0.96 rg 58 422 230 150 re f");
            CenterText(commands, 173, 493, "Sin foto del producto", 10, false);
        }

        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 17 Tf 315 575 Td ({Escape(Shorten(quote.ProductName.ToUpperInvariant(), 24))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 12 Tf 315 558 Td ({Escape(Shorten(quote.ProductName, 34))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 13 Tf 315 528 Td (Ficha Tecnica:) Tj ET");
        var techY = 514;
        foreach (var line in TechnicalLines(quote).Take(10))
        {
            commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 6 Tf 315 {techY} Td (- {Escape(Shorten(line, 44))}) Tj ET");
            techY -= 10;
        }

        var quoteItems = quote.Items.Count > 0 ? quote.Items.OrderBy(x => x.Order).ToList() : [];
        if (quoteItems.Count > 1)
        {
            DrawComparison(commands, 126, 305, quoteItems);
        }
        else
        {
            DrawCommercialValues(commands, quote);
        }

        DrawRoundedLikeBox(commands, 45, 162, 522, 34);
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 12 Tf 52 182 Td (Cliente:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 52 169 Td ({Escape(Shorten(customerName.ToUpperInvariant(), 42))}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 10 Tf 310 182 Td (Correo:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 352 182 Td (N/A) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 10 Tf 310 169 Td (Cel:) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 352 169 Td ({Escape(Value(customerPhone))}) Tj ET");

        commands.AppendLine($"0.05 0.05 0.05 rg BT /F1 9 Tf 170 92 Td (CC: {Escape(Value(quote.IdentificationNumber))}) Tj ET");
        commands.AppendLine("0.05 0.05 0.05 RG 0.6 w 170 112 252 0 m S");
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 8 Tf 170 76 Td (AUTORIZACION DE TRATAMIENTO DE DATOS) Tj ET");
        Paragraph(commands, 170, 64, "La aceptacion de esta cotizacion autoriza el tratamiento de datos personales para gestion comercial y estudio de credito.", 72, 6, 2);

        DrawQrPlaceholder(commands, 45, 44, "SIGUENOS");
        DrawQrPlaceholder(commands, 462, 44, "ENCUESTA");
        CenterText(commands, 306, 38, "CRM / Powered by EnMarcha CRM", 8, false);
        return commands.ToString();
    }

    private static void DrawCommercialValues(StringBuilder commands, QuoteDto quote)
    {
        var inventoryItem = quote.Items.OrderBy(x => x.Order).FirstOrDefault();
        var boxX = 132;
        var boxY = 200;
        var boxW = 435;
        var boxH = 214;
        DrawRoundedLikeBox(commands, boxX, boxY, boxW, boxH);
        CenterText(commands, boxX + boxW / 2, boxY + boxH - 22, quote.ProductName.ToUpperInvariant(), 12, true, 32);
        CenterText(commands, boxX + boxW / 2, boxY + boxH - 50, Value(quote.CreditType, "CONTADO").ToUpperInvariant(), 9, true, 24);

        var labels = new[]
        {
            "Forma de pago", "Modelo", "Precio Vehiculo", "Valor Tramites", "Valor Bono",
            "Valor Dcto", "Chasis", "Motor", "", "Bodega", "Cuota Inicial",
            "Nro de Cuotas", "Valor de Cuotas", "Valor Garantia", "Valor Poliza RC"
        };
        var values = new[]
        {
            Value(quote.CreditType, "CONTADO"),
            quote.ValidUntil.Year.ToString(CultureInfo.InvariantCulture),
            Money(quote.ProductPrice),
            Money(quote.AdministrativeFees),
            "-",
            quote.PromotionDiscount > 0 ? Money(quote.PromotionDiscount) : "-",
            Value(inventoryItem?.InventoryChassisNumber),
            Value(inventoryItem?.InventoryEngineNumber),
            "",
            Value(inventoryItem?.InventoryWarehouseName),
            quote.DownPayment > 0 ? Money(quote.DownPayment) : "-",
            quote.TermMonths > 0 ? quote.TermMonths.ToString(CultureInfo.InvariantCulture) : "-",
            quote.EstimatedMonthlyPayment > 0 ? Money(quote.EstimatedMonthlyPayment) : "-",
            "-",
            "-"
        };

        var y = boxY + boxH - 70;
        for (var i = 0; i < labels.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(labels[i]))
            {
                commands.AppendLine($"0.05 0.05 0.05 rg BT /F{(labels[i] is "Precio Vehiculo" or "Cuota Inicial" or "Nro de Cuotas" or "Valor de Cuotas" ? "2" : "1")} 8 Tf 45 {y} Td ({Escape(labels[i])}) Tj ET");
                commands.AppendLine($"0.90 0.90 0.90 RG 0.25 w 45 {y - 4} 522 0 m S");
                CenterText(commands, boxX + boxW / 2, y, values[i], labels[i] == "Precio Vehiculo" ? 13 : 9, labels[i] == "Precio Vehiculo" || labels[i] == "VALOR TOTAL");
            }
            y -= 13;
        }

        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 10 Tf 45 {boxY + 9} Td (VALOR TOTAL) Tj ET");
        CenterText(commands, boxX + boxW / 2, boxY + 9, Money(quote.DiscountedProductPrice + quote.AdministrativeFees + quote.Insurance), 13, true);
    }

    private static IReadOnlyList<string> TechnicalLines(QuoteDto quote)
    {
        var raw = quote.ProductTechnicalSheet ?? string.Empty;
        var lines = raw
            .Replace("\r", "\n")
            .Split(new[] { '\n', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (lines.Count > 0) return lines;

        return
        [
            $"Producto: {quote.ProductName}",
            $"Marca: {Value(quote.SalesPointBrand, "N/A")}",
            $"Entrega: {Value(quote.SalesPointDeliveryMode, "N/A")}",
            $"Precio: {Money(quote.ProductPrice)}",
            $"Vigencia: {Date(quote.ValidUntil)}"
        ];
    }

    private static void DrawRoundedLikeBox(StringBuilder commands, int x, int y, int width, int height)
    {
        commands.AppendLine($"1 1 1 rg {x} {y} {width} {height} re f");
        commands.AppendLine($"0.05 0.05 0.05 RG 0.6 w {x} {y} {width} {height} re S");
    }

    private static void CenterText(StringBuilder commands, int centerX, int y, string text, int size, bool bold, int maxChars = 64)
    {
        var value = Shorten(text, maxChars);
        var approxWidth = value.Length * size * 0.26;
        var x = Math.Max(35, (int)Math.Round(centerX - approxWidth));
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F{(bold ? "2" : "1")} {size} Tf {x} {y} Td ({Escape(value)}) Tj ET");
    }

    private static void DrawQrPlaceholder(StringBuilder commands, int x, int y, string label)
    {
        commands.AppendLine($"0.05 0.05 0.05 rg BT /F2 7 Tf {x + 10} {y + 92} Td ({Escape(label)}) Tj ET");
        commands.AppendLine($"0.05 0.05 0.05 RG 0.5 w {x} {y} 80 80 re S");
        for (var row = 0; row < 8; row++)
        {
            for (var col = 0; col < 8; col++)
            {
                if ((row * 3 + col * 5 + label.Length) % 4 == 0)
                {
                    commands.AppendLine($"0.05 0.05 0.05 rg {x + 8 + col * 8} {y + 8 + row * 8} 6 6 re f");
                }
            }
        }
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
        var headers = new[] { "Producto", "Precio", "Inicial", "Financiado" };
        var columns = new[] { x + 24, x + 210, x + 318, x + 410 };
        for (var i = 0; i < headers.Length; i++)
        {
            commands.AppendLine($"0.09 0.11 0.15 rg BT /F2 7 Tf {columns[i]} {y + 168} Td ({headers[i]}) Tj ET");
        }

        var rowY = y + 138;
        foreach (var item in items.Take(4))
        {
            commands.AppendLine($"0.84 0.88 0.92 RG 0.4 w {x + 16} {rowY - 8} 488 24 re S");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F2 8 Tf {columns[0]} {rowY} Td ({Escape(Shorten(item.ProductName, 26))}) Tj ET");
            if (!string.IsNullOrWhiteSpace(item.InventoryChassisNumber))
            {
                commands.AppendLine($"0.36 0.42 0.48 rg BT /F1 6 Tf {columns[0]} {rowY - 9} Td ({Escape(Shorten("Chasis: " + item.InventoryChassisNumber, 34))}) Tj ET");
            }
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[1]} {rowY} Td ({Escape(Money(item.DiscountedProductPrice))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[2]} {rowY} Td ({Escape(Money(item.DownPayment))}) Tj ET");
            commands.AppendLine($"0.08 0.10 0.14 rg BT /F1 8 Tf {columns[3]} {rowY} Td ({Escape(Money(item.FinancedAmount))}) Tj ET");
            rowY -= 30;
        }

        var bestPrice = items.OrderBy(x => x.DiscountedProductPrice).First();
        var total = items.Sum(x => x.DiscountedProductPrice);
        commands.AppendLine("0.082 0.373 0.459 rg " + (x + 16) + " " + (y + 18) + " 488 34 re f");
        commands.AppendLine($"1 1 1 rg BT /F2 9 Tf {x + 30} {y + 38} Td (Total articulos: {Escape(Money(total))}) Tj ET");
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

    private static string Shorten(string value, int max) => value.Length <= max ? value : value[..Math.Max(0, max - 3)] + "...";

    private static bool IsJpeg(QuotePdfImage? image) =>
        image is not null && HasJpegSignature(image.Data);

    private static bool HasJpegSignature(byte[] data) =>
        data.Length > 4 &&
        data[0] == 0xFF &&
        data[1] == 0xD8;

    private static bool HasPngSignature(byte[] data) =>
        data.Length > 8 &&
        data[0] == 137 &&
        data[1] == 80 &&
        data[2] == 78 &&
        data[3] == 71 &&
        data[4] == 13 &&
        data[5] == 10 &&
        data[6] == 26 &&
        data[7] == 10;

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

        if (image is not null && HasPngSignature(image.Data))
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
