using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class QuickLinksPage : ListPage
{
    private readonly LibraryStore _store;

    public QuickLinksPage(LibraryStore store)
    {
        _store = store;
        Title = "J Quick Links";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _store.Changed += (_, _) => RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        List<IListItem> items =
        [
            new ListItem(new EditQuickLinkPage(_store, null))
            {
                Title = "+ Add quick link",
                Subtitle = "A temporary bookmark with a category",
            },
        ];

        foreach (QuickLinkEntry link in _store.Links)
        {
            items.Add(new ListItem(new OpenUrlCommand(link.Url))
            {
                Title = link.Title,
                Subtitle = $"{link.Category} · {link.Url}",
                MoreCommands =
                [
                    new CommandContextItem(new CopyTextCommand(link.Url, "Copy URL", "URL copied")) { Title = "Copy URL" },
                    new CommandContextItem(new EditQuickLinkPage(_store, link)) { Title = "Edit" },
                    new CommandContextItem(new DeleteLinkCommand(_store, link.Id)) { Title = "Delete", IsCritical = true },
                ],
            });
        }

        return [.. items];
    }
}
