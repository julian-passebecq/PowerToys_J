using JUtilityPalette.Data;
using JUtilityPalette.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette;

public sealed partial class JUtilityPaletteCommandsProvider : CommandProvider
{
    private readonly LibraryStore _store = new();
    private readonly ICommandItem[] _commands;

    public JUtilityPaletteCommandsProvider()
    {
        DisplayName = "J Utility Palette";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _commands =
        [
            new CommandItem(new PromptLibraryPage(_store))
            {
                Title = "J Prompts",
                Subtitle = "Reusable prompts + composable instructions",
            },
            new CommandItem(new QuickLinksPage(_store))
            {
                Title = "J Quick Links",
                Subtitle = "Temporary categorized links",
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;
}
