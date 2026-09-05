using JUtilityPalette.Commands;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class SystemShortcutsPage : ListPage
{
    public SystemShortcutsPage()
    {
        Title = "J System";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        PlaceholderText = "Search hosts, environment variables, or Task Manager";
    }

    public override IListItem[] GetItems()
    {
        string query = SearchText?.Trim() ?? string.Empty;
        return SystemShortcutCatalog.Rank(query)
            .Select(definition => (IListItem)new ListItem(new SystemShortcutCommand(definition))
            {
                Title = definition.Title,
                Subtitle = definition.Subtitle,
            })
            .ToArray();
    }
}
