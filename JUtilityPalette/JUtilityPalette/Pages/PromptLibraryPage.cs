using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class PromptLibraryPage : ListPage
{
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
                Subtitle = "Create a reusable base prompt or add-on",
            },
            new ListItem(new CaptureClipboardPromptCommand(_store, "Prompt"))
            {
                Title = "Clipboard → Prompt",
                Subtitle = "Save the current text clipboard immediately",
            },
            new ListItem(new CaptureClipboardPromptCommand(_store, "Instruction"))
            {
                Title = "Clipboard → Instruction",
                Subtitle = "Save the current text clipboard as a reusable add-on",
            },
        ];

        string query = SearchText?.Trim() ?? string.Empty;
        foreach (PromptEntry prompt in _store.Prompts.Where(x => Matches(x, query)))
        {
            List<IContextItem> more = [];

            if (prompt.Kind == "Prompt")
            {
                more.Add(new CommandContextItem(new ComposePromptPage(_store, prompt)) { Title = "Compose" });
                more.Add(new CommandContextItem(new CopyPromptAndOpenCommand(_store, prompt, AppLauncher.ChatGptUrl, "Copy + open ChatGPT")) { Title = "Copy + open ChatGPT" });
                more.Add(new CommandContextItem(new OpenPromptInCodexCommand(_store, prompt)) { Title = "Open in Codex" });
            }

            more.Add(new CommandContextItem(new TogglePromptPinCommand(_store, prompt)) { Title = prompt.IsPinned ? "Unpin" : "Pin" });
            more.Add(new CommandContextItem(new EditPromptPage(_store, prompt)) { Title = "Edit" });
            more.Add(new CommandContextItem(new DeletePromptCommand(_store, prompt.Id)) { Title = "Delete", IsCritical = true });

            items.Add(new ListItem(prompt.Kind == "Prompt"
                ? new CopyPromptCommand(_store, prompt)
                : new CopyTextCommand(prompt.Body, "Copy", "Instruction copied"))
            {
                Title = prompt.IsPinned ? $"★ {prompt.Title}" : prompt.Title,
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
