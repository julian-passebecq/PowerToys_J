using JUtilityPalette.Models;

namespace JUtilityPalette.Utilities;

internal static class PromptMatcher
{
    public static IReadOnlyList<PromptEntry> Rank(IEnumerable<PromptEntry> prompts, string query, bool promptsOnly = false)
    {
        string normalized = query.Trim();
        if (normalized.Length == 0)
        {
            return [];
        }

        return prompts
            .Where(prompt => !promptsOnly || string.Equals(prompt.Kind, "Prompt", StringComparison.OrdinalIgnoreCase))
            .Select(prompt => new { Prompt = prompt, Score = Score(prompt, normalized) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Prompt.IsPinned)
            .ThenBy(x => x.Prompt.Title, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Prompt)
            .ToArray();
    }

    internal static int Score(PromptEntry prompt, string query)
    {
        string[] terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0)
        {
            return 0;
        }

        int score = 0;
        foreach (string term in terms)
        {
            if (prompt.Title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
            else if (prompt.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 75;
            }
            else if (prompt.Category.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 40;
            }
            else if (prompt.Body.Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                score += 15;
            }
            else
            {
                return 0;
            }
        }

        if (string.Equals(prompt.Title, query, StringComparison.OrdinalIgnoreCase))
        {
            score += 300;
        }
        else if (prompt.Title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            score += 150;
        }

        if (prompt.IsPinned)
        {
            score += 25;
        }

        if (string.Equals(prompt.Kind, "Prompt", StringComparison.OrdinalIgnoreCase))
        {
            score += 5;
        }

        return score;
    }
}
