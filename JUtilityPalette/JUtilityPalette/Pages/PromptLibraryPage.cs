using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
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

        foreach (PromptEntry prompt in _store.Prompts)
        {
            List<IContextItem> more =
            [
                new CommandContextItem(new EditPromptPage(_store, prompt)) { Title = "Edit" },
                new CommandContextItem(new DeletePromptCommand(_store, prompt.Id)) { Title = "Delete", IsCritical = true },
            ];

            if (prompt.Kind == "Prompt")
            {
                more.Insert(0, new CommandContextItem(new ComposePromptPage(_store, prompt)) { Title = "Compose + copy" });
            }

            items.Add(new ListItem(new CopyTextCommand(prompt.Body, "Copy", prompt.Kind == "Prompt" ? "Prompt copied" : "Instruction copied"))
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
}
