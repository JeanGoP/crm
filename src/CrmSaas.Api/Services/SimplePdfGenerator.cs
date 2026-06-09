using System.Globalization;
using System.Text;
using CrmSaas.Domain.Common;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Api.Services;

public sealed record QuotePdfImage(byte[] Data, string ContentType, string FileName);

public static class SimplePdfGenerator
{
    private static readonly CultureInfo ColombianCulture = CultureInfo.GetCultureInfo("es-CO");

    public static byte[] Quote(QuoteDto quote, string companyName, QuotePdfImage? productImage = null)
    {
        return CreateQuotePdf(quote, companyName, productImage);
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

    private static byte[] CreateQuotePdf(QuoteDto quote, string companyName, QuotePdfImage? productImage)
    {
        var jpeg = IsJpeg(productImage) ? productImage : null;
        var imageSize = jpeg is null ? null : TryReadJpegSize(jpeg.Data);
        if (imageSize is null) jpeg = null;

        var content = QuotePageContent(quote, companyName, jpeg is not null);
        var objects = new List<PdfObject>
        {
            new("<< /Type /Catalog /Pages 2 0 R >>"),
            new(string.Empty),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>")
        };

        var imageObjectNumber = 0;
        if (jpeg is not null && imageSize is not null)
        {
            imageObjectNumber = 5;
            objects.Add(new($"<< /Type /XObject /Subtype /Image /Width {imageSize.Value.Width} /Height {imageSize.Value.Height} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {jpeg.Data.Length} >>", jpeg.Data));
        }

        var pageNumber = objects.Count + 1;
        var contentNumber = pageNumber + 1;
        var resources = imageObjectNumber > 0
            ? $"<< /Font << /F1 3 0 R /F2 4 0 R >> /XObject << /Im1 {imageObjectNumber} 0 R >> >>"
            : "<< /Font << /F1 3 0 R /F2 4 0 R >> >>";
        objects.Add(new($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources {resources} /Contents {contentNumber} 0 R >>"));
        objects.Add(new($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>", Encoding.ASCII.GetBytes(content)));
        objects[1] = new($"<< /Type /Pages /Kids [{pageNumber} 0 R] /Count 1 >>");

        return BuildPdf(objects, $"{quote.Number}.pdf");
    }

    private static string QuotePageContent(QuoteDto quote, string companyName, bool includeImage)
    {
        var customerName = $"{quote.CustomerFirstName} {quote.CustomerMiddleName} {quote.CustomerLastName} {quote.CustomerSecondLastName}"
            .Replace("  ", " ")
            .Trim();
        if (string.IsNullOrWhiteSpace(customerName)) customerName = $"{quote.CustomerFirstNames} {quote.CustomerLastNames}".Trim();

        var commands = new StringBuilder();
        commands.AppendLine("0.961 0.980 0.988 rg 0 0 612 792 re f");
        commands.AppendLine("0.082 0.373 0.459 rg 0 724 612 68 re f");
        commands.AppendLine("1 1 1 rg BT /F2 22 Tf 40 756 Td (COTIZACION COMERCIAL) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F1 10 Tf 40 738 Td ({Escape(companyName)}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F2 12 Tf 430 756 Td ({Escape(quote.Number)}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F1 9 Tf 430 738 Td (Fecha: {Escape(Date(quote.QuoteDate))}) Tj ET");

        DrawSection(commands, 34, 590, 250, 104, "CLIENTE");
        Text(commands, 50, 660, "Nombre", customerName);
        Text(commands, 50, 636, "Identificacion", $"{IdentificationType(quote.IdentificationType)} {Value(quote.IdentificationNumber)}");
        Text(commands, 50, 612, "Valida hasta", Date(quote.ValidUntil));

        DrawSection(commands, 306, 590, 272, 104, "PRODUCTO");
        Text(commands, 322, 660, "Producto", quote.ProductName);
        Text(commands, 322, 636, "Valor comercial", Money(quote.ProductPrice));
        Text(commands, 322, 612, "Formato", includeImage ? "Foto comercial incluida" : "Foto no disponible para PDF");

        if (includeImage)
        {
            commands.AppendLine("q 220 0 0 154 196 404 cm /Im1 Do Q");
        }
        else
        {
            commands.AppendLine("0.90 0.94 0.96 rg 196 404 220 154 re f");
            commands.AppendLine("0.32 0.38 0.45 rg BT /F1 11 Tf 234 478 Td (Sin foto principal JPG) Tj ET");
        }

        DrawSection(commands, 34, 248, 544, 124, "SIMULACION FINANCIERA");
        FinancialRow(commands, 52, 330, "Precio producto", Money(quote.ProductPrice), "Cuota inicial", Money(quote.DownPayment), "Plazo", $"{quote.TermMonths} meses");
        FinancialRow(commands, 52, 300, "Seguro", Money(quote.Insurance), "Gastos adm.", Money(quote.AdministrativeFees), "Tasa mensual", $"{quote.MonthlyInterestRate:N2}%");
        commands.AppendLine("0.082 0.373 0.459 rg 52 265 508 44 re f");
        commands.AppendLine($"1 1 1 rg BT /F2 13 Tf 70 292 Td (Total financiado: {Escape(Money(quote.FinancedAmount))}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F2 13 Tf 326 292 Td (Cuota aprox.: {Escape(Money(quote.EstimatedMonthlyPayment))}) Tj ET");
        commands.AppendLine($"1 1 1 rg BT /F1 9 Tf 70 274 Td (Total estimado a pagar: {Escape(Money(quote.EstimatedTotalPayment))}) Tj ET");

        DrawSection(commands, 34, 120, 544, 96, "CONDICIONES");
        var conditions = "Cotizacion informativa sujeta a disponibilidad del producto, validacion comercial, estudio de credito, aprobacion de la entidad financiadora y politicas internas vigentes.";
        Paragraph(commands, 52, 185, conditions, 93, 11);
        Paragraph(commands, 52, 150, $"Observaciones: {Value(quote.Notes)}", 93, 10);

        commands.AppendLine("0.72 0.77 0.82 RG 1 w 52 74 m 252 74 l S 342 74 m 542 74 l S");
        commands.AppendLine("0.10 0.12 0.16 rg BT /F1 9 Tf 78 58 Td (Asesor comercial) Tj ET");
        commands.AppendLine("0.10 0.12 0.16 rg BT /F1 9 Tf 420 58 Td (Cliente) Tj ET");
        commands.AppendLine("0.35 0.40 0.46 rg BT /F1 8 Tf 40 28 Td (Documento generado por CRM Comercial. Valores aproximados sujetos a aprobacion final.) Tj ET");
        return commands.ToString();
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

    private static void Paragraph(StringBuilder commands, int x, int y, string value, int maxChars, int fontSize)
    {
        var lineY = y;
        foreach (var line in WrapText(value, maxChars).Take(3))
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
