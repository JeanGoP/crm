using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static partial class SimplePdfGenerator
{
    private sealed partial class QuoteLayout
    {
        private void RenderFeatured(string customer, QuoteItemDto[] items)
        {
            AdvisorCard();
            if (quote.IsBundle && items.Length > 1) BundleProducts(items);
            else ProductShowcase();
            if (!string.IsNullOrWhiteSpace(quote.ProductTechnicalSheet))
                ParagraphText("Ficha técnica: " + quote.ProductTechnicalSheet, 7.6);

            PaymentCard();
            if (!Cash)
            {
                var schedule = quote.InitialPaymentSchedule.OrderBy(x => x.DueDate).ToArray();
                if (schedule.Length == 1)
                {
                    var plan = "Cuota extra: " + Money(schedule[0].Amount) + " el " + schedule[0].DueDate.ToString("dd/MM/yyyy");
                    if (quote.CreditStartDate.HasValue)
                        plan += ". Inicio del crédito: " + quote.CreditStartDate.Value.ToString("dd/MM/yyyy");
                    ParagraphText(plan, 8);
                }
                else if (schedule.Length > 1)
                {
                    ParagraphText("Plan de pago de cuota extra", 8.5, true);
                    Table(null, ["Fecha", "Valor"], [300, 231], schedule
                        .Select(x => new[] { x.DueDate.ToString("dd/MM/yyyy"), Money(x.Amount) }).ToList());
                }
                var remaining = Math.Max(0, quote.DownPayment - quote.InitialPaymentPaidToday - schedule.Sum(x => x.Amount));
                if (remaining > 0) ParagraphText("Cuota extra pendiente de programar: " + Money(remaining), 8);
                if (quote.CreditStartDate.HasValue && schedule.Length != 1)
                    ParagraphText("Inicio del crédito: " + quote.CreditStartDate.Value.ToString("dd/MM/yyyy"), 8);
                ParagraphText("Tasa: " + Value(quote.SalesPointRateName, "Tasa general") + ". Elige un solo plazo.", 8);
            }

            FeaturedCustomerCard(customer);
            if (!string.IsNullOrWhiteSpace(quote.Notes)) ParagraphText("Observaciones: " + quote.Notes, 8.2);
            if (!string.IsNullOrWhiteSpace(quote.SalesPointCommercialTerms))
                ParagraphText("Condiciones comerciales: " + quote.SalesPointCommercialTerms, 8.2);
            Requirements();
        }

        private void BundleProducts(QuoteItemDto[] items)
        {
            Table("01  ARTÍCULOS COTIZADOS", ["Artículo", "Precio", "Descuento", "Cargos", "Total contado"],
                [203, 82, 76, 76, 94], items.Select(x => new[] { x.ProductName, Money(x.ProductPrice),
                    Money(x.PromotionDiscount), Money(Cash ? 0 : x.Insurance + x.AdministrativeFees),
                    Money(x.DiscountedProductPrice + (Cash ? 0 : x.Insurance + x.AdministrativeFees)) }).ToList());
            ParagraphText("Todos los artículos: " + Money(quote.DiscountedProductPrice +
                (Cash ? 0 : quote.Insurance + quote.AdministrativeFees)), 8.5, true);
        }

        private void AdvisorCard()
        {
            const double height = 43;
            Ensure(height + 14);
            Outline(Left, y - height, Width, height);
            Text(Left + 12, y - 14, "ASESOR COMERCIAL", 7.5, true);
            Text(Left + 12, y - 30, Value(advisor), Fit(Value(advisor), 250, 9), true);
            Text(Left + 286, y - 14, "SEDE", 7.5, true);
            Text(Left + 286, y - 30, Value(quote.SalesPointName), Fit(Value(quote.SalesPointName), 232, 9), false);
            y -= height + 14;
        }

        private void ProductShowcase()
        {
            Ensure(productPhoto is null ? 110 : 177);
            Text(Left, y - 11, "01  ARTÍCULO COTIZADO", 11, true);
            page.AppendLine(FormattableString.Invariant($"{Ink} RG 0.6 w {Left:0.###} {y - 19:0.###} m {Left + Width:0.###} {y - 19:0.###} l S"));
            y -= 30;
            var name = Value(quote.ProductName);
            if (productPhoto is not null)
            {
                const double imageWidth = 234, imageHeight = 123;
                Outline(Left, y - imageHeight, imageWidth, imageHeight);
                var scale = Math.Min((imageWidth - 12) / productPhoto.Width, (imageHeight - 12) / productPhoto.Height);
                var drawnWidth = productPhoto.Width * scale;
                var drawnHeight = productPhoto.Height * scale;
                var imageX = Left + (imageWidth - drawnWidth) / 2;
                var imageY = y - imageHeight + (imageHeight - drawnHeight) / 2;
                page.AppendLine(FormattableString.Invariant($"q {drawnWidth:0.###} 0 0 {drawnHeight:0.###} {imageX:0.###} {imageY:0.###} cm /ProductPhoto Do Q"));
                var x = Left + imageWidth + 16;
                var lines = Wrap(name, Width - imageWidth - 28, 13, true);
                for (var i = 0; i < Math.Min(4, lines.Count); i++) Text(x, y - 20 - i * 16, lines[i], 13, true);
                Text(x, y - 96, "Precio del artículo", 8, false);
                var amount = Money(quote.ProductPrice);
                Text(Left + Width - Measure(amount, 10, true) - 8, y - 96, amount, 10, true);
                Text(x, y - 114, Cash ? "Modalidad: Contado" : "Modalidad: Crédito", 8, true);
                y -= imageHeight + 15;
                if (lines.Count > 4) ParagraphText("Nombre completo del artículo: " + name, 8);
            }
            else
            {
                const double height = 69;
                Outline(Left, y - height, Width, height);
                var lines = Wrap(name, Width - 170, 12, true);
                for (var i = 0; i < Math.Min(3, lines.Count); i++) Text(Left + 12, y - 20 - i * 15, lines[i], 12, true);
                var amount = Money(quote.ProductPrice);
                Text(Left + Width - Measure(amount, 10, true) - 12, y - 24, amount, 10, true);
                Text(Left + Width - 118, y - 42, Cash ? "Contado" : "Crédito", 8, true);
                y -= height + 14;
                if (lines.Count > 3) ParagraphText("Nombre completo del artículo: " + name, 8);
            }
        }

        private void PaymentCard()
        {
            var charges = Cash ? 0 : quote.Insurance + quote.AdministrativeFees;
            var rows = new List<(string Label, string Value, bool Bold)>
            {
                (quote.IsBundle ? "Precio de los artículos" : "Precio del artículo", Money(quote.ProductPrice), false)
            };
            if (quote.PromotionDiscount > 0) rows.Add(("Descuento", Money(quote.PromotionDiscount), false));
            if (charges > 0) rows.Add(("Cargos", Money(charges), false));
            rows.Add(("Total contado", Money(quote.DiscountedProductPrice + charges), true));
            if (!Cash)
            {
                rows.Add(("Cuota inicial", Money(quote.InitialPaymentPaidToday), false));
                rows.Add(("Cuota extra", Money(Math.Max(0, quote.DownPayment - quote.InitialPaymentPaidToday)), false));
                rows.Add(("Inicial completa", Money(quote.DownPayment), true));
                rows.Add(("Saldo financiado", Money(quote.FinancedAmount), true));
            }

            var options = Cash ? [] : Options(quote.FinancingOptions, quote.TermMonths,
                quote.EstimatedMonthlyPayment, quote.EstimatedTotalPayment).OrderBy(x => x.TermMonths).ToArray();
            var compactTerms = options.Length is > 0 and <= 4;
            var height = 27 + rows.Count * 19 + (compactTerms ? 44 : 0) + 8;
            Ensure(height + 14);
            Rect(Left, y - 27, Width, 27, Ink);
            Text(Left + 11, y - 18, "02  CONDICIONES DE PAGO", 10.5, true, White);
            y -= 27;
            Outline(Left, y - (height - 27), Width, height - 27);
            foreach (var (label, value, bold) in rows)
            {
                Text(Left + 13, y - 13, label, 8.2, bold);
                Text(Left + Width - Measure(value, 9, bold) - 13, y - 13, value, 9, bold);
                y -= 19;
                page.AppendLine(FormattableString.Invariant($"{Ink} RG 0.3 w {Left + 12:0.###} {y:0.###} m {Left + Width - 12:0.###} {y:0.###} l S"));
            }
            if (compactTerms)
            {
                var columnWidth = (Width - 26) / options.Length;
                for (var i = 0; i < options.Length; i++)
                {
                    var center = Left + 13 + columnWidth * (i + 0.5);
                    var term = $"{options[i].TermMonths} cuotas";
                    var amount = Money(options[i].MonthlyPayment);
                    Text(center - Measure(term, 8, true) / 2, y - 16, term, 8, true);
                    Text(center - Measure(amount, 9, true) / 2, y - 32, amount, 9, true);
                }
                y -= 44;
            }
            y -= 18;
            if (options.Length > 4)
                Table("Plazos disponibles", ["Plazo", "Cuota mensual"], [210, 321],
                    options.Select(x => new[] { $"{x.TermMonths} cuotas", Money(x.MonthlyPayment) }).ToList());
        }

        private void FeaturedCustomerCard(string customer)
        {
            var nameLines = Wrap(Value(customer).ToUpperInvariant(), 247, 9, true);
            var rightLines = new[] { "Documento: " + Value(quote.IdentificationNumber), "Teléfono: " + Value(phone) }
                .SelectMany(x => Wrap(x, 244, 8)).ToList();
            var height = Math.Max(nameLines.Count, rightLines.Count) * 12 + 26;
            Ensure(height + 12);
            Outline(Left, y - height, Width, height);
            Text(Left + 12, y - 13, "CLIENTE", 7.5, true);
            for (var i = 0; i < nameLines.Count; i++) Text(Left + 12, y - 28 - i * 12, nameLines[i], 9, true);
            for (var i = 0; i < rightLines.Count; i++) Text(Left + 280, y - 21 - i * 12, rightLines[i], 8, false);
            y -= height + 12;
            if (!string.IsNullOrWhiteSpace(address)) ParagraphText("Dirección: " + address, 8);
        }
    }
}
