using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class RecentPromptsPage : ListPage
{
    private const string ChatGptUrl = "https://chatgpt.com/";
    private const string CodexUrl = "https://chatgpt.com/codex";
    private readonly LibraryStore _store;

    public RecentPromptsPage(LibraryStore store)
    {
        _store = store;
        Title = "J Recent Prompts";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        ShowDetails = true;
        PlaceholderText = "Search recently used prompts";
        _store.Changed += (_, _) => RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        string query = SearchText?.Trim() ?? string.Empty;
        List<IListItem> items = [];

        foreach (RecentPromptEntry recent in _store.RecentPrompts.Where(x => Matches(x, query)))
        {
            items.Add(new ListItem(new CopyTextCommand(recent.Text, "Copy", "Recent prompt copied"))
            {
                Title = recent.Title,
                Subtitle = recent.CreatedUtc.LocalDateTime.ToString("g"),
                MoreCommands =
                [
                    new CommandContextItem(new CopyTextAndOpenCommand(recent.Text, ChatGptUrl, "Copy + open ChatGPT")) { Title = "Copy + open ChatGPT" },
                    new CommandContextItem(new CopyTextAndOpenCommand(recent.Text, CodexUrl, "Copy + open Codex")) { Title = "Copy + open Codex" },
                    new CommandContextItem(new DeleteRecentPromptCommand(_store, recent.Id)) { Title = "Remove from history", IsCritical = true },
                ],
                Details = new Details
                {
                    Title = recent.Title,
                    Body = $"```text\n{recent.Text}\n```",
                },
            });
        }

        return [.. items];
    }

    private static bool Matches(RecentPromptEntry entry, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Text.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
