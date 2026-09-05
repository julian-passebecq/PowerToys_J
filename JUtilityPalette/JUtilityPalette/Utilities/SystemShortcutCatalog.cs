namespace JUtilityPalette.Utilities;

internal enum SystemShortcutKind
{
    HostsFileEditor,
    EnvironmentVariables,
    TaskManager,
}

internal sealed record SystemShortcutDefinition(
    SystemShortcutKind Kind,
    string Title,
    string Subtitle,
    string Keywords);

internal static class SystemShortcutCatalog
{
    public static IReadOnlyList<SystemShortcutDefinition> All { get; } =
    [
        new(SystemShortcutKind.HostsFileEditor, "Hosts File Editor", "PowerToys · opens elevated", "host hosts dns hostname fichier hote hostsfile"),
        new(SystemShortcutKind.EnvironmentVariables, "Environment Variables", "PowerToys · profiles, user and system variables", "env environment variable variables path profile profil variable variables environnement"),
        new(SystemShortcutKind.TaskManager, "Task Manager", "Windows · processes and performance", "task manager process processes performance gestionnaire tache taches tâches processus"),
    ];

    public static IReadOnlyList<SystemShortcutDefinition> Rank(string query)
    {
        string normalized = query.Trim();
        if (normalized.Length == 0)
        {
            return All;
        }

        return All
            .Select(item => new { Item = item, Score = Score(item, normalized) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Item.Title, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Item)
            .ToArray();
    }

    private static int Score(SystemShortcutDefinition item, string query)
    {
        string[] terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int score = 0;

        foreach (string term in terms)
        {
            if (item.Title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
            else if (item.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 75;
            }
            else if (item.Keywords.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 45;
            }
            else
            {
                return 0;
            }
        }

        if (string.Equals(item.Title, query, StringComparison.OrdinalIgnoreCase))
        {
            score += 250;
        }

        return score;
    }
}
