using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ordercloud_bulk_import_library
{
    public static class Extensions
    {
        public static string Slugify(this string text)
        {
            return Regex.Replace(
                RemoveDiacritics(text.ToLowerInvariant())
                    .Replace(" ", "-"),
                @"[^a-z0-9-]+", "")
                .Trim('-');
        }

        public static string RemoveDiacritics(string text)
        {
            return new string(text.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray())
                .Normalize(NormalizationForm.FormC);
        }

        public static string? TruncateLongString(this string str, int maxLength) =>
            str?[0..Math.Min(str.Length, maxLength)];

        public static decimal TryParseToDecimal(this string value)
        {
            if (decimal.TryParse(value, out decimal result))
                return result;
            throw new Exception("Invalid string for decimal conversion");
        }
    }
}
