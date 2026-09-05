using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Pages;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions.Toolkit;
using JCopyTextCommand = JUtilityPalette.Commands.CopyTextCommand;

namespace JUtilityPalette.Commands;

internal sealed partial class PromptFallbackCommandItem : FallbackCommandItem
{
    private const string Prefix = "j ";
    private readonly LibraryStore _store;
    private readonly int _rank;

    public PromptFallbackCommandItem(LibraryStore store, int rank)
        : base("J Prompts", $"com.julian.jutilitypalette.prompt-fallback.{rank}")
    {
        _store = store;
        _rank = rank;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Hide();
    }

    public override void UpdateQuery(string query)
    {
        string normalized = query.TrimStart();
        if (!normalized.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            Hide();
            return;
        }

        string search = normalized[Prefix.Length..].Trim();
        if (search.Length == 0)
        {
            Hide();
            return;
        }

        var match = _store.Prompts
            .Select(prompt => new { Prompt = prompt, Score = Score(prompt, search) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Prompt.IsPinned)
            .ThenBy(x => x.Prompt.Title, StringComparer.OrdinalIgnoreCase)
            .Skip(_rank)
            .FirstOrDefault();

        if (match is null)
        {
            Hide();
            return;
        }

        Apply(match.Prompt);
    }

    private void Apply(PromptEntry prompt)
    {
        bool isTemplate = prompt.Kind == "Prompt" && PromptTemplate.HasVariables(prompt.Body);
        Command = prompt.Kind == "Prompt"
            ? isTemplate
                ? new ComposePromptPage(_store, prompt)
                : new CopyPromptCommand(_store, prompt)
            : new JCopyTextCommand(prompt.Body, "Copy", "Instruction copied");

        Title = prompt.IsPinned ? $"★ {prompt.Title}" : prompt.Title;
        Subtitle = $"J · {prompt.Kind} · {prompt.Category}{(isTemplate ? " · fill template" : string.Empty)}";
    }

    private void Hide()
    {
        Title = string.Empty;
        Subtitle = string.Empty;
    }

    private static int Score(PromptEntry prompt, string query)
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

        if (prompt.Kind == "Prompt")
        {
            score += 5;
        }

        return score;
    }
}
