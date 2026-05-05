using System.Text;
using CrmSaas.Application.DTOs;

namespace CrmSaas.Api.Services;

public static class SimplePdfGenerator
{
    public static byte[] Quote(QuoteDto quote, string companyName)
    {
        var lines = new[]
        {
            "COTIZACION DE MOTO",
            $"Numero: {quote.Number}",
            $"Empresa: {companyName}",
            $"Fecha: {quote.QuoteDate:yyyy-MM-dd}",
            $"Valida hasta: {quote.ValidUntil:yyyy-MM-dd}",
            "",
            "CLIENTE",
            $"Tipo identificacion: {quote.IdentificationType}",
            $"Identificacion: {quote.IdentificationNumber ?? "Pendiente"}",
            $"Nombre: {quote.CustomerFirstNames} {quote.CustomerLastNames}",
            "",
            "MOTO",
            $"Producto: {quote.ProductName}",
            $"Precio: {quote.ProductPrice:C0}",
            "",
            $"Observaciones: {quote.Notes ?? "N/A"}"
        };

        return CreatePdf(lines);
    }

    private static byte[] CreatePdf(IReadOnlyList<string> lines)
    {
        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 16 Tf");
        content.AppendLine("50 780 Td");
        foreach (var line in lines)
        {
            content.AppendLine($"({Escape(line)}) Tj");
            content.AppendLine("0 -24 Td");
            content.AppendLine("/F1 11 Tf");
        }
        content.AppendLine("ET");

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}endstream"
        };

        var pdf = new StringBuilder("%PDF-1.4\n");
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
        foreach (var offset in offsets.Skip(1))
        {
            pdf.AppendLine($"{offset:0000000000} 00000 n ");
        }

        pdf.AppendLine("trailer");
        pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        pdf.AppendLine("startxref");
        pdf.AppendLine(xrefOffset.ToString());
        pdf.AppendLine("%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string Escape(string value) => value
        .Replace("\\", "\\\\")
        .Replace("(", "\\(")
        .Replace(")", "\\)")
        .Normalize(NormalizationForm.FormD)
        .Where(c => c < 128)
        .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c), sb => sb.ToString());
}
