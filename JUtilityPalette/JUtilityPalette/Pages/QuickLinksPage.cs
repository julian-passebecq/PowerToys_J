using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using JCopyTextCommand = JUtilityPalette.Commands.CopyTextCommand;
using JOpenUrlCommand = JUtilityPalette.Commands.OpenUrlCommand;

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
        PlaceholderText = "Search quick links or categories";
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

        string query = SearchText?.Trim() ?? string.Empty;
        foreach (QuickLinkEntry link in _store.Links.Where(x => Matches(x, query)))
        {
            items.Add(new ListItem(new JOpenUrlCommand(link.Url))
            {
                Title = link.Title,
                Subtitle = $"{link.Category} · {link.Url}",
                MoreCommands =
                [
                    new CommandContextItem(new JCopyTextCommand(link.Url, "Copy URL", "URL copied")) { Title = "Copy URL" },
                    new CommandContextItem(new EditQuickLinkPage(_store, link)) { Title = "Edit" },
                    new CommandContextItem(new DeleteLinkCommand(_store, link.Id)) { Title = "Delete", IsCritical = true },
                ],
            });
        }

        return [.. items];
    }

    private static bool Matches(QuickLinkEntry entry, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return entry.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.Url.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
