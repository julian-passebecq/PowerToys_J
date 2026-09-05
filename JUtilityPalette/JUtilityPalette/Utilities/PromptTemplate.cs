using System.Text.RegularExpressions;

namespace JUtilityPalette.Utilities;

internal static partial class PromptTemplate
{
    [GeneratedRegex(@"\{\{(?<name>[A-Za-z0-9][A-Za-z0-9_. -]{0,39})\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex VariableRegex();

    public static IReadOnlyList<string> GetVariables(string text)
    {
        List<string> variables = [];
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in VariableRegex().Matches(text ?? string.Empty))
        {
            string name = match.Groups["name"].Value.Trim();
            if (name.Length > 0 && seen.Add(name))
            {
                variables.Add(name);
            }
        }

        return variables;
    }

    public static bool HasVariables(string text) => VariableRegex().IsMatch(text ?? string.Empty);

    public static string Fill(string text, IReadOnlyDictionary<string, string> values)
    {
        return VariableRegex().Replace(text ?? string.Empty, match =>
        {
            string name = match.Groups["name"].Value.Trim();
            if (values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            return match.Value;
        });
    }
}
