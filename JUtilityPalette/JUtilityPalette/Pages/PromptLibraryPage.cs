using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class PromptLibraryPage : ListPage
{
    private const string ChatGptUrl = "https://chatgpt.com/";
    private const string CodexUrl = "https://chatgpt.com/codex";
    private readonly LibraryStore _store;

    public PromptLibraryPage(LibraryStore store)
    {
        _store = store;
        Title = "J Prompts";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        ShowDetails = true;
        PlaceholderText = "Search prompts, instructions, or categories";
        _store.Changed += (_, _) => RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        List<IListItem> items =
        [
            new ListItem(new EditPromptPage(_store, null))
            {
                Title = "+ Add prompt / instruction",
                Subtitle = "Save a reusable base prompt or add-on",
            },
        ];

        string query = SearchText?.Trim() ?? string.Empty;
        foreach (PromptEntry prompt in _store.Prompts.Where(x => Matches(x, query)))
        {
            List<IContextItem> more =
            [
                new CommandContextItem(new EditPromptPage(_store, prompt)) { Title = "Edit" },
                new CommandContextItem(new DeletePromptCommand(_store, prompt.Id)) { Title = "Delete", IsCritical = true },
            ];

            if (prompt.Kind == "Prompt")
            {
                more.Insert(0, new CommandContextItem(new ComposePromptPage(_store, prompt)) { Title = "Compose + copy" });
                more.Insert(1, new CommandContextItem(new CopyPromptAndOpenCommand(_store, prompt, ChatGptUrl, "Copy + open ChatGPT")) { Title = "Copy + open ChatGPT" });
                more.Insert(2, new CommandContextItem(new CopyPromptAndOpenCommand(_store, prompt, CodexUrl, "Copy + open Codex")) { Title = "Copy + open Codex" });
            }

            items.Add(new ListItem(prompt.Kind == "Prompt"
                ? new CopyPromptCommand(_store, prompt)
                : new CopyTextCommand(prompt.Body, "Copy", "Instruction copied"))
            {
                Title = prompt.Title,
                Subtitle = $"{prompt.Kind} · {prompt.Category}",
                MoreCommands = [.. more],
                Details = new Details
                {
                    Title = prompt.Title,
                    Body = $"```text\n{prompt.Body}\n```",
                },
            });
        }

        return [.. items];
    }

    private static bool Matches(PromptEntry entry, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Kind.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Body.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
