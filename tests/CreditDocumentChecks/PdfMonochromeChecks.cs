using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

internal static class PdfMonochromeChecks
{
    public static void AssertBlackAndWhite(byte[] pdf, string document)
    {
        var content = Encoding.ASCII.GetString(pdf);
        var colors = Regex.Matches(content, @"(?<![\d.])(\d+(?:\.\d+)?) (\d+(?:\.\d+)?) (\d+(?:\.\d+)?) (?:rg|RG)\b");
        if (colors.Count == 0) throw new Exception($"{document}: no se encontraron colores de texto o relleno.");
        foreach (Match color in colors)
        {
            var channels = color.Groups.Cast<Group>().Skip(1)
                .Select(x => double.Parse(x.Value, CultureInfo.InvariantCulture)).ToArray();
            if (channels.Any(x => x is not (0 or 1)) || channels.Distinct().Count() != 1)
                throw new Exception($"{document}: color no permitido en el PDF: {color.Value}.");
        }
    }
}
