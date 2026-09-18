using System.Text;
using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static partial class SimplePdfGenerator
{
    // Layout only: amounts and financing alternatives come from the persisted quotation.
    private sealed class QuoteLayout(QuoteDto quote, string company, PdfImageData? logo,
        string? phone, string? address, string? advisor)
    {
        private const double Left = 32, Width = 531, Bottom = 52;
        private const string Ink = "0.09 0.17 0.21", Teal = "0 0.50 0.45", Pale = "0.93 0.96 0.95";
        private readonly List<StringBuilder> pages = [];
        private StringBuilder page = new();
        private double y;
        private bool Cash => quote.CreditType == "Contado";
        private sealed record Finance(string Name, decimal Initial, decimal Paid, decimal Financed,
            DateTime? Start, IReadOnlyCollection<QuoteInitialPaymentDto> Schedule, IReadOnlyCollection<QuoteFinancingOptionDto> Options);

        public List<string> Render()
        {
            NewPage();
            var customer = string.Join(" ", new[] { quote.CustomerFirstName, quote.CustomerMiddleName,
                quote.CustomerLastName, quote.CustomerSecondLastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (string.IsNullOrWhiteSpace(customer)) customer = $"{quote.CustomerFirstNames} {quote.CustomerLastNames}".Trim();
            CustomerCard(customer);
            if (!string.IsNullOrWhiteSpace(address)) ParagraphText("Dirección: " + address, 8);

            var items = quote.Items.OrderBy(x => x.Order).ToArray();
            var priceRows = items.Length > 0
                ? items.Select(x => new[] { x.ProductName, Money(x.ProductPrice), Money(x.PromotionDiscount),
                    Money(Cash ? 0 : x.Insurance + x.AdministrativeFees),
                    Money(x.DiscountedProductPrice + (Cash ? 0 : x.Insurance + x.AdministrativeFees)) }).ToList()
                : new List<string[]> { new[] { quote.ProductName, Money(quote.ProductPrice), Money(quote.PromotionDiscount),
                    Money(Cash ? 0 : quote.Insurance + quote.AdministrativeFees),
                    Money(quote.DiscountedProductPrice + (Cash ? 0 : quote.Insurance + quote.AdministrativeFees)) } };
            Table("01  Artículos cotizados", ["Artículo", "Precio", "Descuento", "Cargos", "Total contado"],
                [203, 82, 76, 76, 94], priceRows);
            if (!Cash && (quote.Insurance + quote.AdministrativeFees > 0 || items.Any(x => x.Insurance + x.AdministrativeFees > 0)))
                ParagraphText("Cargos: seguro y gastos administrativos incluidos en la cotización.", 8);
            if (quote.IsBundle)
                ParagraphText("Total de los artículos: " + Money(quote.DiscountedProductPrice + (Cash ? 0 : quote.Insurance + quote.AdministrativeFees)), 9, true);

            if (!Cash)
            {
                var groups = quote.IsBundle || items.Length <= 1
                    ? new[] { new Finance(quote.IsBundle ? "Todos los artículos" : quote.ProductName,
                        quote.DownPayment, quote.InitialPaymentPaidToday, quote.FinancedAmount, quote.CreditStartDate,
                        quote.InitialPaymentSchedule, Options(quote.FinancingOptions, quote.TermMonths, quote.EstimatedMonthlyPayment, quote.EstimatedTotalPayment)) }
                    : items.Select(x => new Finance(x.ProductName, x.DownPayment, x.InitialPaymentPaidToday, x.FinancedAmount,
                        x.CreditStartDate, x.InitialPaymentSchedule,
                        Options(x.FinancingOptions, x.TermMonths, x.EstimatedMonthlyPayment, x.EstimatedTotalPayment))).ToArray();
                var terms = groups.SelectMany(x => x.Options).Select(x => x.TermMonths).Distinct().Order().ToArray();
                var compact = terms.Length > 0 && groups.All(x => x.Paid == x.Initial && x.Schedule.Count == 0);
                if (!compact)
                    Table("02  Condiciones de pago", ["Artículo", "Cuota inicial", "Cuota extra", "Inicial total", "Financiado"],
                        [171, 90, 90, 90, 90], groups.Select(x => new[] { x.Name, Money(x.Paid),
                            Money(Math.Max(0, x.Initial - x.Paid)), Money(x.Initial), Money(x.Financed) }).ToList());
                var baseWidths = compact
                    ? new[] { quote.IsBundle ? 110d : 130d,
                        Math.Max(65, groups.Max(x => Measure(Money(x.Initial), 8)) + 14),
                        Math.Max(65, groups.Max(x => Measure(Money(x.Financed), 8)) + 14) }
                    : new[] { quote.IsBundle ? 110d : 130d };
                var termWidth = Math.Max(54, groups.SelectMany(x => x.Options)
                    .Select(x => Measure(Money(x.MonthlyPayment), 8) + 14).DefaultIfEmpty(54).Max());
                var termsPerRow = Math.Max(1, (int)((Width - baseWidths.Sum()) / termWidth));
                foreach (var chunk in terms.Chunk(termsPerRow))
                {
                    var columns = (compact ? new[] { "Artículo", "Inicial total", "Financiado" } : new[] { "Artículo" }).Concat(chunk.Select(x => $"{x} cuotas")).ToArray();
                    var widths = baseWidths.Concat(chunk.Select(_ => (Width - baseWidths.Sum()) / chunk.Length)).ToArray();
                    Table(compact ? "02  Opciones de financiación" : "Opciones de financiación", columns, widths,
                        groups.Select(x => (compact ? new[] { x.Name, Money(x.Initial), Money(x.Financed) } : new[] { x.Name }).Concat(chunk.Select(term =>
                        x.Options.FirstOrDefault(o => o.TermMonths == term) is { } option ? Money(option.MonthlyPayment) : "-" )).ToArray()).ToList());
                }
                ParagraphText("Tasa: " + Value(quote.SalesPointRateName, "Tasa general") + ". Elige un solo plazo para cada alternativa de compra.", 8);
                foreach (var group in groups.Where(x => x.Schedule.Count > 0 || x.Initial > x.Paid))
                {
                    ParagraphText("Plan de pago de cuota extra - " + group.Name, 9, true);
                    if (group.Schedule.Count > 0)
                        Table(null, ["Fecha", "Valor"], [300, 231], group.Schedule.OrderBy(x => x.DueDate)
                            .Select(x => new[] { x.DueDate.ToString("dd/MM/yyyy"), Money(x.Amount) }).ToList());
                    var remaining = Math.Max(0, group.Initial - group.Paid - group.Schedule.Sum(x => x.Amount));
                    if (remaining > 0) ParagraphText("Pendiente de programar: " + Money(remaining), 8);
                    if (group.Start.HasValue) ParagraphText("Inicio del crédito: " + group.Start.Value.ToString("dd/MM/yyyy"), 8);
                }
            }
            if (!string.IsNullOrWhiteSpace(quote.Notes)) ParagraphText("Observaciones: " + quote.Notes, 8.5);
            if (!string.IsNullOrWhiteSpace(quote.SalesPointCommercialTerms)) ParagraphText("Condiciones comerciales: " + quote.SalesPointCommercialTerms, 8.5);
            Requirements();
            for (var i = 0; i < pages.Count; i++)
            {
                page = pages[i];
                Text(Left, 29, "EnMarcha CRM", 7, false);
                Text(515, 29, $"{i + 1} / {pages.Count}", 7, false);
            }
            return pages.Select(x => x.ToString()).ToList();
        }

        private static IReadOnlyCollection<QuoteFinancingOptionDto> Options(IReadOnlyCollection<QuoteFinancingOptionDto>? options,
            int term, decimal monthly, decimal total) => options is { Count: > 0 } ? options :
            term > 0 ? [new(term, monthly, total)] : [];

        private void Requirements()
        {
            // Keep the complete requirements and same-day validity together.
            const string property = "PROPIEDAD RAÍZ: Presenta certificado de libertad y tradición, libre de patrimonio familiar y afectación familiar.";
            const string merchant = "COMERCIANTE: Presenta cámara de comercio y RUT; debe llevar registrado el negocio mínimo 2 años.";
            const string employee = "EMPLEADO: Presenta carta laboral y colillas de pago; debe ganar más de 1 salario mínimo.";
            const string one = "DE ESTAS 3 OPCIONES DE CODEUDOR SOLO NECESITA UNO.";
            const string validity = "ESTA COTIZACIÓN ESTÁ SUJETA A CAMBIOS SIN PREVIO AVISO Y ES VÁLIDA SOLO POR EL DÍA EN QUE SE COTIZA.";
            var height = Wrap(validity, Width - 20, 8.2, true).Count * 11 + 20;
            if (!Cash) height += 18 + new[] { property, merchant, employee, one }.Sum(x => Wrap(x, Width, 8.2, x == one).Count * 11 + 7);
            Ensure(height);
            if (!Cash)
            {
                ParagraphText("REQUISITOS PARA EL CRÉDITO", 10, true);
                ParagraphText(property, 8.2);
                ParagraphText(merchant, 8.2);
                ParagraphText(employee, 8.2);
                ParagraphText(one, 8.2, true);
            }
            ParagraphText(validity, 8.2, true, true);
        }

        private void NewPage()
        {
            page = new StringBuilder(); pages.Add(page);
            if (logo is not null)
            {
                var scale = Math.Min(155d / logo.Width, 58d / logo.Height);
                page.AppendLine(FormattableString.Invariant($"q {logo.Width * scale:0.###} 0 0 {logo.Height * scale:0.###} 32 751 cm /Logo Do Q"));
            }
            else
            {
                var companyLines = Wrap(Value(company), 270, 14, true);
                for (var i = 0; i < Math.Min(3, companyLines.Count); i++) Text(32, 799 - i * 17, companyLines[i], 14, true);
            }
            Text(365, 793, "COTIZACIÓN", 23, true);
            Text(350, 773, quote.Number, Fit(quote.Number, 210, 10), true);
            Text(350, 753, "Fecha: " + quote.QuoteDate.ToString("dd/MM/yyyy"), 9, false);
            page.AppendLine($"{Teal} RG 1.5 w 32 735 m 563 735 l S");
            y = 720;
        }
        private void Ensure(double height) { if (y - height < Bottom) NewPage(); }

        private void CustomerCard(string customer)
        {
            var left = new[] { Value(customer).ToUpperInvariant(), "Documento: " + Value(quote.IdentificationNumber), "Teléfono: " + Value(phone) }
                .SelectMany(x => Wrap(x, 245, 8.5)).ToList();
            var right = new[] { Value(advisor), "Sede: " + Value(quote.SalesPointName), "Modalidad: " + (Cash ? "Contado" : "Crédito") }
                .SelectMany(x => Wrap(x, 245, 8.5)).ToList();
            var height = Math.Max(left.Count, right.Count) * 12 + 31;
            // Unusually long contact data uses the paginated text renderer.
            if (height > 300)
            {
                ParagraphText("CLIENTE: " + customer, 9, true);
                ParagraphText("Documento: " + Value(quote.IdentificationNumber) + ". Teléfono: " + Value(phone), 8.5);
                ParagraphText("ASESOR: " + Value(advisor) + ". Sede: " + Value(quote.SalesPointName), 8.5);
                return;
            }
            Rect(Left, y - height, Width, height, Pale);
            Text(44, y - 16, "CLIENTE", 8, true);
            Text(310, y - 16, "ASESOR / SEDE", 8, true);
            for (var i = 0; i < left.Count; i++) Text(44, y - 32 - i * 12, left[i], 8.5, i == 0);
            for (var i = 0; i < right.Count; i++) Text(310, y - 32 - i * 12, right[i], 8.5, i == 0);
            y -= height + 14;
        }

        private void Table(string? title, string[] headers, double[] widths, List<string[]> rows, double size = 8)
        {
            const double headerHeight = 27;
            void Header()
            {
                if (title is not null) { Text(Left, y - 12, title, 11, true); y -= 25; }
                Rect(Left, y - headerHeight, Width, headerHeight, Teal);
                var x = Left;
                for (var i = 0; i < headers.Length; i++)
                {
                    var headerSize = Fit(headers[i], widths[i] - 14, 8, true);
                    var headerX = i == 0 ? x + 7 : x + (widths[i] - Measure(headers[i], headerSize, true)) / 2;
                    Text(headerX, y - 17, headers[i], headerSize, true, "1 1 1");
                    x += widths[i];
                }
                y -= headerHeight;
            }
            Ensure((title is null ? 0 : 25) + headerHeight + 40);
            Header();
            for (var r = 0; r < rows.Count; r++)
            {
                var row = rows[r];
                // Currency is never split; long names wrap and continue instead of truncating.
                var lines = row.Select((value, i) => value.StartsWith("$", StringComparison.Ordinal)
                    ? new List<string> { value } : Wrap(value, widths[i] - 14, size)).ToArray();
                var totalLines = lines.Max(x => x.Count);
                var offset = 0;
                while (offset < totalLines)
                {
                    var count = Math.Min(totalLines - offset, 35);
                    var height = Math.Max(26, count * 11 + 12);
                    if (y - height < Bottom) { NewPage(); Header(); }
                    if (r % 2 == 1) Rect(Left, y - height, Width, height, Pale);
                    var x = Left;
                    for (var i = 0; i < row.Length; i++)
                    {
                        for (var l = offset; l < Math.Min(offset + count, lines[i].Count); l++)
                        {
                            var value = lines[i][l];
                            var fontSize = Fit(value, widths[i] - 14, size);
                            var tx = i == 0 ? x + 7 : x + (widths[i] - Measure(value, fontSize)) / 2;
                            Text(tx, y - 17 - (l - offset) * 11, value, fontSize, false);
                        }
                        x += widths[i];
                    }
                    y -= height;
                    page.AppendLine(FormattableString.Invariant($"0.86 0.90 0.91 RG 0.4 w 32 {y:0.###} m 563 {y:0.###} l S"));
                    offset += count;
                }
            }
            y -= 8;
        }

        private void ParagraphText(string text, double size, bool bold = false, bool shaded = false)
        {
            var lines = Wrap(text, Width - (shaded ? 20 : 0), size, bold);
            var height = lines.Count * 11 + (shaded ? 20 : 7);
            if (height < 660) Ensure(height);
            if (shaded) Rect(Left, y - height, Width, height, Pale);
            foreach (var value in lines)
            {
                Ensure(18);
                Text(Left + (shaded ? 10 : 0), y - 11, value, size, bold);
                y -= 11;
            }
            y -= shaded ? 20 : 7;
        }
        private void Rect(double x, double bottom, double width, double height, string color) =>
            page.AppendLine(FormattableString.Invariant($"{color} rg {x:0.###} {bottom:0.###} {width:0.###} {height:0.###} re f"));

        private void Text(double x, double bottom, string value, double size, bool bold, string color = Ink)
        {
            // WinAnsi hex strings preserve Spanish accents and cannot inject PDF operators.
            var encoded = Encoding.Latin1.GetBytes(value.Replace('\u2013', '-').Replace('\u2014', '-').Replace('\u2019', '\''));
            page.AppendLine(FormattableString.Invariant($"{color} rg BT /{(bold ? "F2" : "F1")} {size:0.###} Tf {x:0.###} {bottom:0.###} Td <{Convert.ToHexString(encoded)}> Tj ET"));
        }

        // Standard Helvetica widths (ASCII 32-126), in 1/1000 em. Exact metrics keep
        // regular amounts centered under bold headings, including the last partial group.
        private static readonly int[] RegularWidths = [278,278,355,556,556,889,667,191,333,333,389,584,278,333,278,278,556,556,556,556,556,556,556,556,556,556,278,278,584,584,584,556,1015,667,667,722,722,667,611,778,722,278,500,667,556,833,722,778,667,778,722,667,611,722,667,944,667,667,611,278,278,278,469,556,333,556,556,500,556,556,278,556,556,222,222,500,222,833,556,556,556,556,333,500,278,556,500,722,500,500,500,334,260,334,584];
        private static readonly int[] BoldWidths = [278,333,474,556,556,889,722,238,333,333,389,584,278,333,278,278,556,556,556,556,556,556,556,556,556,556,333,333,584,584,584,611,975,722,722,722,722,667,611,778,722,278,556,722,611,833,722,778,667,778,722,667,611,722,667,944,667,667,611,333,278,333,584,556,333,556,611,556,611,556,333,611,611,278,278,556,278,889,611,611,611,611,389,556,333,611,556,778,556,556,500,389,280,389,584];
        private static double Measure(string text, double size, bool bold = false) => text.Normalize(NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            .Sum(c => c is >= ' ' and <= '~' ? (bold ? BoldWidths : RegularWidths)[c - 32] : c == '\u00a0' ? 278 : 556) * size / 1000d;
        private static double Fit(string text, double width, double size, bool bold = false) => Math.Min(size, size * width / Math.Max(1, Measure(text, size, bold)));
        private static List<string> Wrap(string? text, double width, double size, bool bold = false)
        {
            var result = new List<string>();
            var current = "";
            foreach (var word in (text ?? "-").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                if (Measure(current.Length == 0 ? word : current + " " + word, size, bold) > width && current.Length > 0)
                { result.Add(current); current = ""; }
                foreach (var c in (current.Length == 0 ? word : " " + word))
                {
                    if (Measure(current + c, size, bold) > width && current.Length > 0) { result.Add(current); current = ""; }
                    current += c;
                }
            }
            if (current.Length > 0) result.Add(current);
            return result.Count == 0 ? ["-"] : result;
        }
    }
}
