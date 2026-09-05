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
    private const string PromptsCommandId = "com.julian.jutilitypalette.prompts";
    private const string ChatGptCommandId = "com.julian.jutilitypalette.open-chatgpt";
    private const string CodexCommandId = "com.julian.jutilitypalette.open-codex";

    private readonly LibraryStore _store = new();
    private readonly ICommandItem[] _commands;
    private readonly IFallbackCommandItem[] _fallbackCommands;
    private readonly ICommandItem _promptLibraryCommand;
    private readonly ICommandItem _chatGptCommand;
    private readonly ICommandItem _codexCommand;
    private readonly ICommandItem _workflowDockBand;

    public JUtilityPaletteCommandsProvider()
    {
        Id = "com.julian.jutilitypalette";
        DisplayName = "J Utility Palette";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var promptLibraryPage = new PromptLibraryPage(_store)
        {
            Id = PromptsCommandId,
        };
        _promptLibraryCommand = new CommandItem(promptLibraryPage)
        {
            Title = "J Prompts",
            Subtitle = "Reusable prompts + composable instructions",
        };

        var chatGptLauncher = new JOpenUrlCommand(AppLauncher.ChatGptUrl, "Open ChatGPT", ChatGptCommandId);
        _chatGptCommand = new CommandItem(chatGptLauncher)
        {
            Title = "ChatGPT",
            Subtitle = "Open ChatGPT",
        };

        var codexLauncher = new JOpenUrlCommand(AppLauncher.CodexNewChatUri, "Open Codex", CodexCommandId);
        _codexCommand = new CommandItem(codexLauncher)
        {
            Title = "Codex Desktop",
            Subtitle = "Open a new local Codex chat",
        };

        _workflowDockBand = new WrappedDockItem(
            [
                new ListItem(promptLibraryPage) { Title = "Prompts" },
                new ListItem(chatGptLauncher) { Title = "ChatGPT" },
                new ListItem(codexLauncher) { Title = "Codex" },
            ],
            "com.julian.jutilitypalette.dock.workflow",
            "J Workflow");

        _commands =
        [
            _promptLibraryCommand,
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

        _fallbackCommands =
        [
            new PromptFallbackCommandItem(_store, 0),
            new PromptFallbackCommandItem(_store, 1),
            new PromptFallbackCommandItem(_store, 2),
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override IFallbackCommandItem[] FallbackCommands() => _fallbackCommands;

    public override ICommandItem[]? GetDockBands() => [_workflowDockBand];

    public override ICommandItem? GetCommandItem(string id) => id switch
    {
        PromptsCommandId => _promptLibraryCommand,
        ChatGptCommandId => _chatGptCommand,
        CodexCommandId => _codexCommand,
        _ => null,
    };
}
