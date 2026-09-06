using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class SystemShortcutFallbackCommandItem : FallbackCommandItem
{
    private const string Prefix = "js";
    private readonly int _rank;

    public SystemShortcutFallbackCommandItem(int rank)
        : base("J System", $"com.julian.jutilitypalette.system-fallback.{rank}")
    {
        _rank = rank;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Hide();
    }

    public override void UpdateQuery(string query)
    {
        if (!FallbackPrefix.TryExtract(query, Prefix, out string search))
        {
            Hide();
            return;
        }

        SystemShortcutDefinition? definition = SystemShortcutCatalog.Rank(search).Skip(_rank).FirstOrDefault();
        if (definition is null)
        {
            Hide();
            return;
        }

        Command = new SystemShortcutCommand(definition);
        Title = definition.Title;
        Subtitle = $"J System · {definition.Subtitle}";
    }

    private void Hide()
    {
        Command = null;
        Title = string.Empty;
        Subtitle = string.Empty;
    }
}
