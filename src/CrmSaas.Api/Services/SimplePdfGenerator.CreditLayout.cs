using System.Text;
using CrmSaas.Application.DTOs;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Api.Services;

public static partial class SimplePdfGenerator
{
    private static byte[] CreateCreditSignaturePdf(CreditApplicationDto app, string company, string? logoDataUrl)
    {
        PdfImageData? logo = null;
        if (logoDataUrl?.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) == true)
        {
            var comma = logoDataUrl.IndexOf(',');
            if (comma > 0)
            {
                try { logo = TryCreatePdfImage(new(Convert.FromBase64String(logoDataUrl[(comma + 1)..]), logoDataUrl[5..comma].Split(';')[0], "logo")); }
                catch (FormatException) { /* An invalid optional logo must not prevent printing. */ }
            }
        }
        var objects = new List<PdfObject>
        {
            new("<< /Type /Catalog /Pages 2 0 R >>"), new(string.Empty),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"),
            new("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>")
        };
        var images = new List<string>();
        AddImageObject(objects, images, "Logo", logo);
        var resources = $"<< /Font << /F1 3 0 R /F2 4 0 R >> /XObject << {string.Join(" ", images)} >> >>";
        var pages = new CreditSignatureLayout(app, company, logo).Render();
        var pageRefs = new List<string>();
        foreach (var content in pages)
        {
            var number = objects.Count + 1;
            pageRefs.Add($"{number} 0 R");
            objects.Add(new($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595.28 841.89] /Resources {resources} /Contents {number + 1} 0 R >>"));
            objects.Add(new($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>", Encoding.ASCII.GetBytes(content)));
        }
        objects[1] = new($"<< /Type /Pages /Kids [{string.Join(" ", pageRefs)}] /Count {pages.Count} >>");
        return BuildPdf(objects, $"{app.Number}-solicitud-credito.pdf");
    }

    private sealed class CreditSignatureLayout(CreditApplicationDto app, string company, PdfImageData? logo)
    {
        private const double Left = 30, Width = 535, Bottom = 807, Font = 7, Line = 8;
        private const string Teal = "0.02 0.40 0.43";
        private readonly List<string> pages = [];
        private StringBuilder page = new();
        private double y;
        private string section = "";
        private static string Blank(string? value) => value?.Trim() ?? "";
        private static string Amount(decimal? value) => value.HasValue ? Money(value.Value) : "";
        private static string Day(DateTime? value) => value?.ToString("dd/MM/yyyy") ?? "";
        private static string IdType(TipoIdentificacionColombia type) => type switch
        {
            TipoIdentificacionColombia.CedulaCiudadania => "Cédula de ciudadanía",
            TipoIdentificacionColombia.CedulaExtranjeria => "Cédula de extranjería",
            TipoIdentificacionColombia.TarjetaIdentidad => "Tarjeta de identidad",
            TipoIdentificacionColombia.PermisoProteccionTemporal => "Permiso de protección temporal",
            TipoIdentificacionColombia.Nit => "NIT",
            TipoIdentificacionColombia.Pasaporte => "Pasaporte",
            _ => ""
        };

        public List<string> Render()
        {
            var d = app.FormDetails ?? new();
            var people = (app.CoDebtors ?? []).Where(x => x.Active).ToList();
            // Support older snapshots that only contain the original single co-debtor.
            if (app.CoDebtors is null && !string.IsNullOrWhiteSpace(app.CoDebtorName))
                people.Add(new(null, app.CoDebtorName, Blank(app.CoDebtorIdentification), Blank(app.CoDebtorMobile), app.CoDebtorRelationship,
                    app.CoDebtorMonthlyIncome ?? 0, app.CoDebtorReference1Name, app.CoDebtorReference1Mobile, app.CoDebtorReference1Relationship,
                    app.CoDebtorReference2Name, app.CoDebtorReference2Mobile, app.CoDebtorReference2Relationship));
            NewPage();
            Row(("Fecha", Day(app.CreatedAt)), ("Zona", d.Zone), ("Punto de venta", d.SalesPoint));
            Row(("Asesor", d.Advisor), ("Consecutivo", app.Number));
            Section("DATOS DEL DEUDOR");
            Person(app.CustomerName, IdType(app.IdentificationType), app.IdentificationNumber, app.Mobile, app.Address, app.City, d);
            Section("ACTIVIDAD ECONÓMICA DEL DEUDOR");
            Work(d, app.Occupation, app.MonthlyIncome);
            Row(("Nacimiento", Day(app.BirthDate)), ("Referencia ubicación", d.LocationReference));
            References("REFERENCIAS PERSONALES DEL DEUDOR", app.Reference1Name, app.Reference1Mobile, app.Reference1Relationship,
                app.Reference2Name, app.Reference2Mobile, app.Reference2Relationship, d);
            for (var i = 0; i < people.Count; i++)
            {
                var p = people[i];
                var detail = p.FormDetails ?? new();
                Section($"DATOS DEL CODEUDOR {i + 1} / DEUDOR SOLIDARIO");
                Person(p.Name, detail.IdentificationType, p.Identification, p.Mobile, detail.Address, detail.City, detail);
                Work(detail, detail.Occupation, p.MonthlyIncome);
                Row(("Relación con cliente", p.Relationship), ("Referencia ubicación", detail.LocationReference));
                References($"REFERENCIAS PERSONALES DEL CODEUDOR {i + 1}", p.Reference1Name, p.Reference1Mobile, p.Reference1Relationship,
                    p.Reference2Name, p.Reference2Mobile, p.Reference2Relationship, detail);
            }
            Section("INFORMACIÓN DE LA VENTA");
            Row(("Tipo de negocio", d.BusinessType), ("Valor producto", Amount(app.MotorcycleValue)));
            Row(("Cuota inicial", Amount(app.DownPayment)), ("SOAT", Amount(d.Soat)));
            Row(("Cuota extra", Amount(d.ExtraPayment)), ("N.º pagos extra", d.ExtraPaymentCount?.ToString()), ("Matrícula", Amount(d.Registration)));
            Row(("Plazo (meses)", app.TermMonths.ToString()), ("Valor cuota", Amount(d.MonthlyPayment)), ("Total crédito", Amount(d.TotalCredit)));
            Row(("Primer vencimiento acordado", Day(app.FirstDueDate)));
            Section("PRODUCTO / CARACTERÍSTICAS DEL VEHÍCULO");
            Row(("Producto", app.ProductName));
            Row(("Tipo", d.VehicleType), ("Color", d.VehicleColor), ("Marca", d.VehicleBrand));
            Row(("Línea", d.VehicleLine), ("Variante", d.VehicleVariant), ("Cilindraje", d.VehicleEngineCc));
            Row(("Modelo", d.VehicleModel), ("Placa", d.VehiclePlate));
            Row(("Chasis", d.VehicleChassis), ("Motor", d.VehicleEngine));
            Row(("Observaciones vehículo", d.VehicleNotes));
            if (!string.IsNullOrWhiteSpace(app.Notes)) Row(("Observaciones solicitud", app.Notes));

            NewPage();
            Section("AUTORIZACIÓN DE TRATAMIENTO DE DATOS PERSONALES");
            Paragraph($"Empresa responsable: {company}", true);
            foreach (var paragraph in DataAuthorization(app, company).SkipWhile(x => x != "AUTORIZACION").Skip(1).TakeWhile(x => x.Length > 0))
                Paragraph(paragraph);
            Paragraph("La firma de cada titular corresponde a la autorización anterior. Los espacios se diligencian al momento de la firma.");
            Section("FIRMAS Y HUELLAS");
            var signers = new List<(string Role, string Name, string Id)> { ("DEUDOR / CLIENTE", app.CustomerName, app.IdentificationNumber) };
            signers.AddRange(people.Select((p, i) => ($"CODEUDOR {i + 1} / DEUDOR SOLIDARIO", p.Name, p.Identification)));
            foreach (var pair in signers.Chunk(2))
            {
                var height = Math.Max(135, pair.Max(p => 107 + 9 * (QuoteLayout.Wrap(p.Name, Width / 2 - 21, 7.2, true).Count
                    + QuoteLayout.Wrap("Documento: " + p.Id, Width / 2 - 21, 7).Count)));
                Ensure(height + 18);
                for (var i = 0; i < pair.Length; i++) Signature(Left + i * (Width / 2 + 5), Width / 2 - 5, pair[i], height);
                y += height + 18;
            }
            Ensure(50);
            Row(("Lugar de firma", ""), ("Fecha de firma", ""));
            Finish();
            return pages;
        }

        private void Person(string name, string? idType, string id, string phone, string? address, string? city, CreditFormDetailsDto d)
        {
            Row(("Nombre completo", name));
            Row(("Tipo documento", idType), ("Número documento", id));
            Row(("Tipo vivienda", d.HousingType), ("Teléfono", phone));
            Row(("Dirección", address), ("Ciudad", city));
            Row(("Estado civil", d.MaritalStatus), ("Correo", d.Email));
        }

        private void Work(CreditFormDetailsDto d, string? occupation, decimal income)
        {
            Row(("Empresa", d.Employer), ("Cargo", d.JobTitle), ("Ingreso mensual", Amount(income)));
            Row(("Ocupación", occupation), ("Teléfono laboral", d.WorkPhone));
            Row(("Dirección laboral", d.WorkAddress), ("Correo laboral", d.WorkEmail));
        }

        private void References(string title, string? n1, string? p1, string? r1, string? n2, string? p2, string? r2, CreditFormDetailsDto d)
        {
            Section(title);
            Row(("Referencia 1", n1), ("Referencia 2", n2));
            Row(("Dirección", d.Reference1Address), ("Dirección", d.Reference2Address));
            Row(("Teléfono", p1), ("Teléfono", p2));
            Row(("Relación", r1), ("Relación", r2));
        }

        private void NewPage()
        {
            if (page.Length > 0) Finish();
            page = new();
            section = "";
            Text("SOLICITUD DE CRÉDITO", Left, 29, 13, true, Teal);
            var names = QuoteLayout.Wrap(company, 380, 9, true);
            var top = 44d;
            foreach (var name in names) { Text(name, Left, top, 9, true); top += 11; }
            if (logo is not null)
            {
                var scale = Math.Min(100d / logo.Width, 32d / logo.Height);
                page.AppendLine(FormattableString.Invariant($"q {logo.Width * scale:0.###} 0 0 {logo.Height * scale:0.###} {565 - logo.Width * scale:0.###} {841.89 - 53:0.###} cm /Logo Do Q"));
            }
            y = Math.Max(60, top + 4);
        }

        private void Finish()
        {
            Text($"{app.Number}  |  Solicitud de crédito", Left, 824, 7);
            Text($"Página {pages.Count + 1}", 510, 824, 7);
            pages.Add(page.ToString());
        }

        private void Ensure(double height)
        {
            if (y + height <= Bottom) return;
            var previous = section;
            NewPage();
            if (previous.Length > 0) Section(previous + " (continuación)");
        }

        private void Section(string title)
        {
            // Reserve the heading plus at least one data row on this page.
            if (y + 37 > Bottom) NewPage();
            section = title.Replace(" (continuación)", "");
            y += 2;
            Box(Left, y, Width, 14, Teal);
            Text(title, Left + 5, y + 10, 7.8, true, "1 1 1");
            y += 14;
        }

        private void Row(params (string Label, string? Value)[] cells)
        {
            var width = Width / cells.Length;
            var labelWidth = cells.Length == 1 ? 123d : cells.Length == 2 ? 90d : 66d;
            var labels = cells.Select(c => QuoteLayout.Wrap(c.Label, labelWidth - 8, Font, true)).ToArray();
            var values = cells.Select(c => string.IsNullOrWhiteSpace(c.Value) ? new List<string> { "" } : QuoteLayout.Wrap(c.Value, width - labelWidth - 8, Font)).ToArray();
            var count = Math.Max(1, Math.Max(labels.Max(x => x.Count), values.Max(x => x.Count)));
            // Split unusually long rows across pages, retaining column alignment and every character.
            var offset = 0;
            while (offset < count)
            {
                Ensure(11);
                var take = Math.Min(count - offset, Math.Max(1, (int)((Bottom - y - 3) / Line)));
                var height = take * Line + 3;
                for (var i = 0; i < cells.Length; i++)
                {
                    var x = Left + i * width;
                    Box(x, y, labelWidth, height, "0.94 0.97 0.97");
                    Border(x, y, width, height);
                    for (var line = 0; line < take; line++)
                    {
                        var index = offset + line;
                        if (index < labels[i].Count) Text(labels[i][index], x + 4, y + 8.5 + line * Line, Font, true);
                        if (index < values[i].Count) Text(values[i][index], x + labelWidth + 4, y + 8.5 + line * Line, Font);
                    }
                }
                y += height;
                offset += take;
            }
        }

        private void Paragraph(string text, bool bold = false)
        {
            y += 9;
            foreach (var line in QuoteLayout.Wrap(text, Width - 8, 9, bold))
            {
                Ensure(13);
                Text(line, Left + 4, y + 10, 9, bold);
                y += 13;
            }
            y += 5;
        }

        private void Signature(double x, double width, (string Role, string Name, string Id) signer, double height)
        {
            Border(x, y + 8, width, height);
            Text(signer.Role, x + 8, y + 22, 8, true, Teal);
            Border(x + width - 60, y + 33, 48, 58);
            Text("Huella", x + width - 48, y + 102, 7);
            page.AppendLine(FormattableString.Invariant($"0.3 0.3 0.3 RG .5 w {x + 8:0.###} {841.89 - y - 86:0.###} m {x + width - 70:0.###} {841.89 - y - 86:0.###} l S"));
            Text("Firma", x + 8, y + 98, 7);
            var names = QuoteLayout.Wrap(signer.Name, width - 16, 7.2, true);
            for (var i = 0; i < names.Count; i++) Text(names[i], x + 8, y + 111 + i * 9, 7.2, true);
            var ids = QuoteLayout.Wrap("Documento: " + signer.Id, width - 16, 7);
            for (var i = 0; i < ids.Count; i++) Text(ids[i], x + 8, y + height - (ids.Count - 1 - i) * 9, 7);
        }

        private void Text(string value, double x, double top, double size, bool bold = false, string color = "0.08 0.13 0.15")
        {
            var bytes = Encoding.Latin1.GetBytes(value.Replace('\u2013', '-').Replace('\u2014', '-').Replace('\u2019', '\''));
            page.AppendLine(FormattableString.Invariant($"{color} rg BT /{(bold ? "F2" : "F1")} {size:0.###} Tf {x:0.###} {841.89 - top:0.###} Td <{Convert.ToHexString(bytes)}> Tj ET"));
        }
        private void Box(double x, double top, double width, double height, string color) =>
            page.AppendLine(FormattableString.Invariant($"{color} rg {x:0.###} {841.89 - top - height:0.###} {width:0.###} {height:0.###} re f"));
        private void Border(double x, double top, double width, double height) =>
            page.AppendLine(FormattableString.Invariant($"0.48 0.57 0.58 RG .35 w {x:0.###} {841.89 - top - height:0.###} {width:0.###} {height:0.###} re S"));
    }
}
