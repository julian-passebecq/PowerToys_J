using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Pages;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using JOpenUrlCommand = JUtilityPalette.Commands.OpenUrlCommand;

namespace JUtilityPalette;

public sealed partial class JUtilityPaletteCommandsProvider : CommandProvider
{
    private const string ChatGptCommandId = "com.julian.jutilitypalette.open-chatgpt";
    private const string CodexCommandId = "com.julian.jutilitypalette.open-codex";

    private readonly LibraryStore _store = new();
    private readonly ICommandItem[] _commands;
    private readonly ICommandItem _chatGptCommand;
    private readonly ICommandItem _codexCommand;

    public JUtilityPaletteCommandsProvider()
    {
        Id = "com.julian.jutilitypalette";
        DisplayName = "J Utility Palette";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        _chatGptCommand = new CommandItem(new JOpenUrlCommand(AppLauncher.ChatGptUrl, "Open ChatGPT", ChatGptCommandId))
        {
            Title = "ChatGPT",
            Subtitle = "Open ChatGPT",
        };
        _codexCommand = new CommandItem(new JOpenUrlCommand(AppLauncher.CodexNewChatUri, "Open Codex", CodexCommandId))
        {
            Title = "Codex Desktop",
            Subtitle = "Open a new local Codex chat",
        };

        _commands =
        [
            new CommandItem(new PromptLibraryPage(_store))
            {
                Title = "J Prompts",
                Subtitle = "Reusable prompts + composable instructions",
            },
            new CommandItem(new RecentPromptsPage(_store))
            {
                Title = "J Recent Prompts",
                Subtitle = "Last 25 prompts you actually used",
            },
            new CommandItem(new QuickLinksPage(_store))
            {
                Title = "J Quick Links",
                Subtitle = "Temporary categorized links",
            },
            _chatGptCommand,
            _codexCommand,
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override ICommandItem[]? GetDockBands() => [_chatGptCommand, _codexCommand];

    public override ICommandItem? GetCommandItem(string id) => id switch
    {
        ChatGptCommandId => _chatGptCommand,
        CodexCommandId => _codexCommand,
        _ => null,
    };
}
