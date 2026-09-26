using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static partial class SimplePdfGenerator
{
    private sealed partial class CreditSignatureLayout
    {
        private void ApplianceHeader()
        {
            var d = app.FormDetails ?? new();
            var top = 28d;
            foreach (var line in QuoteLayout.Wrap(company.ToUpperInvariant(), 390, 13, true))
            {
                Text(line, Left, top, 13, true, Teal);
                top += 15;
            }
            Text("VENTA DE ELECTRODOMÉSTICOS", Left, top, 9, true);
            top += 14;
            foreach (var line in QuoteLayout.Wrap("NIT: " + Blank(d.CompanyTaxId), 390, 8))
            {
                Text(line, Left, top, 8);
                top += 12;
            }
            foreach (var line in QuoteLayout.Wrap(Blank(d.CompanyLocation), 390, 8))
            {
                if (!string.IsNullOrWhiteSpace(d.CompanyLocation)) { Text(line, Left, top, 8); top += 11; }
            }
            if (logo is not null)
            {
                var scale = Math.Min(100d / logo.Width, 42d / logo.Height);
                page.AppendLine(FormattableString.Invariant($"q {logo.Width * scale:0.###} 0 0 {logo.Height * scale:0.###} {565 - logo.Width * scale:0.###} 770 cm /Logo Do Q"));
            }
            y = top + 4;
            Section("SOLICITUD DE CRÉDITO - ELECTRODOMÉSTICOS");
        }

        private string Age(DateTime? birthDate)
        {
            // Age at the application date remains stable when an older form is reprinted.
            if (!birthDate.HasValue || birthDate.Value.Date > app.CreatedAt.Date) return "";
            var years = app.CreatedAt.Year - birthDate.Value.Year;
            if (birthDate.Value.Date.AddYears(years) > app.CreatedAt.Date) years--;
            return years.ToString();
        }

        private void ApplianceData(CreditFormDetailsDto d, List<CreditCoDebtorDto> people)
        {
            Row(("Modalidad de crédito", d.BusinessType), ("Solicitud", app.Number));
            Row(("Fecha", Day(app.CreatedAt)));
            Section("DATOS DEL DEUDOR");
            AppliancePerson(app.CustomerName, app.IdentificationNumber, app.BirthDate, app.Mobile, app.Address, app.City, app.Occupation, app.MonthlyIncome, d);
            References("REFERENCIAS FAMILIARES / PERSONALES DEL DEUDOR", app.Reference1Name, app.Reference1Mobile, app.Reference1Relationship,
                app.Reference2Name, app.Reference2Mobile, app.Reference2Relationship, d);
            for (var i = 0; i < people.Count; i++)
            {
                var p = people[i];
                var detail = p.FormDetails ?? new();
                Section($"DATOS DEL DEUDOR SOLIDARIO {i + 1}");
                Row(("Parentesco", p.Relationship));
                AppliancePerson(p.Name, p.Identification, detail.BirthDate, p.Mobile, detail.Address, detail.City, detail.Occupation, p.MonthlyIncome, detail);
                References($"REFERENCIAS FAMILIARES / PERSONALES DEL CODEUDOR {i + 1}", p.Reference1Name, p.Reference1Mobile, p.Reference1Relationship,
                    p.Reference2Name, p.Reference2Mobile, p.Reference2Relationship, detail);
            }
            Purchase(d);
        }

        private void AppliancePerson(string name, string id, DateTime? birth, string phone, string? address, string? city,
            string? occupation, decimal income, CreditFormDetailsDto d)
        {
            Row(("Apellidos y nombres", name));
            Row(("Documento", id), ("Edad", Age(birth)), ("Estado civil", d.MaritalStatus));
            Row(("Celular", phone), ("Correo electrónico", d.Email));
            Row(("Dirección", address), ("Ciudad", city));
            Row(("Empresa donde labora", d.Employer), ("Dirección laboral", d.WorkAddress));
            Row(("Ocupación", occupation), ("Salario", Amount(income)), ("Teléfono laboral", d.WorkPhone));
        }

        private void Purchase(CreditFormDetailsDto d)
        {
            Ensure(155);
            Section("DATOS DE LA COMPRA");
            var firstPage = pages.Count;
            var start = y;
            var summary = new (string Label, string Value)[]
            {
                ("C. INICIAL", Amount(app.DownPayment)),
                ("ANTICIPO", Amount(app.DownPayment)),
                ("N.º CUOTAS", app.TermMonths.ToString()),
                ("VLR. CUOTA", Amount(d.MonthlyPayment ?? context?.MonthlyPayment)),
                ("FECHA PAGO", Day(app.FirstDueDate))
            };
            var summaryY = start;
            foreach (var (label, value) in summary)
            {
                var valueLines = QuoteLayout.Wrap(value, 97, 8);
                var height = Math.Max(24, valueLines.Count * 10 + 6);
                Box(Left + 335, summaryY, 95, height, "0.94 0.97 0.97");
                Border(Left + 335, summaryY, 200, height);
                Text(label, Left + 340, summaryY + 15, 8, true);
                for (var i = 0; i < valueLines.Count; i++) Text(string.IsNullOrEmpty(value) ? "" : valueLines[i], Left + 434, summaryY + 15 + i * 10, 8);
                summaryY += height;
            }
            PurchaseHeader();
            var items = context?.Items ?? [new CreditPurchaseLine(app.ProductName, "", app.MotorcycleValue)];
            foreach (var item in items) PurchaseRow([item.Quantity.ToString(), item.Name, item.Code, Money(item.Value)]);
            for (var i = items.Count; i < 5; i++) PurchaseRow(["", "", "", ""]);
            PurchaseRow(["", "TOTAL", "", Money(items.Sum(i => i.Value))], true);
            if (pages.Count == firstPage) y = Math.Max(y, summaryY);
            y += 5;
            Row(("Nota", app.Notes));
            Row(("Soporte", d.PurchaseSupport));
        }

        private void PurchaseHeader()
        {
            Box(Left, y, 335, 20, Teal);
            var widths = new[] { 36d, 161, 54, 84 };
            var headers = new[] { "CANT.", "PRODUCTO", "CÓD.", "VALOR" };
            var x = Left;
            for (var i = 0; i < widths.Length; i++) { Text(headers[i], x + 4, y + 13, 8, true, "1 1 1"); x += widths[i]; }
            y += 20;
        }

        private void PurchaseRow(string[] values, bool bold = false)
        {
            var widths = new[] { 36d, 161, 54, 84 };
            var lines = values.Select((v, i) => string.IsNullOrEmpty(v) ? new List<string> { "" } : QuoteLayout.Wrap(v, widths[i] - 8, 8, bold)).ToArray();
            var count = lines.Max(l => l.Count);
            var offset = 0;
            while (offset < count)
            {
                if (y + 18 > Bottom) { Ensure(38); PurchaseHeader(); }
                var take = Math.Min(count - offset, Math.Max(1, (int)((Bottom - y - 8) / 10)));
                var height = take * 10 + 8;
                var x = Left;
                for (var i = 0; i < widths.Length; i++)
                {
                    Border(x, y, widths[i], height);
                    for (var l = 0; l < take; l++)
                        if (offset + l < lines[i].Count) Text(lines[i][offset + l], x + 4, y + 12 + l * 10, 8, bold);
                    x += widths[i];
                }
                y += height;
                offset += take;
            }
        }
    }
}
