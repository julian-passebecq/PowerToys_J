namespace JUtilityPalette.Utilities;

internal static class FallbackPrefix
{
    public static bool TryExtract(string query, string prefixToken, out string search)
    {
        search = string.Empty;
        string prefix = prefixToken.Trim();
        if (prefix.Length == 0)
        {
            return false;
        }

        string normalized = query.TrimStart();
        if (string.Equals(normalized, prefix, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalized.Length <= prefix.Length
            || !normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || !char.IsWhiteSpace(normalized[prefix.Length]))
        {
            return false;
        }

        search = normalized[(prefix.Length + 1)..].Trim();
        return true;
    }
}
