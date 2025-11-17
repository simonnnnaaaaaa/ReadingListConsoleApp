using System;
using System.Globalization;

namespace ReadingList.Domain
{
    public static class StringExtensions
    {
        public static string NormalizeSpaces(this string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : string.Join(" ", s.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        public static string ToTitleCaseSafe(this string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(s.ToLowerInvariant());
        }

        public static bool TryParseYesNo(this string? s, out bool value)
        {
            value = false;
            if (string.IsNullOrWhiteSpace(s)) return true;
            var v = s.Trim().ToLowerInvariant();
            if (v is "y" or "yes" or "true" or "1") { value = true; return true; }
            if (v is "n" or "no" or "false" or "0") { value = false; return true; }
            return false; 
        }

        public static bool ContainsCaseInsensitive(this string? source, string? value)
            => (source ?? string.Empty).IndexOf(value ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
