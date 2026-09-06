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
    private const string DashboardCommandId = "com.julian.jutilitypalette.project-dashboard";
    private const string ManageProjectsCommandId = "com.julian.jutilitypalette.projects";
    private const string SystemCommandId = "com.julian.jutilitypalette.system";
    private const string ChatGptCommandId = "com.julian.jutilitypalette.open-chatgpt";
    private const string CodexCommandId = "com.julian.jutilitypalette.open-codex";

    private readonly LibraryStore _store = new();
    private readonly ICommandItem[] _commands;
    private readonly IFallbackCommandItem[] _fallbackCommands;
    private readonly ICommandItem _promptLibraryCommand;
    private readonly ICommandItem _dashboardCommand;
    private readonly ICommandItem _manageProjectsCommand;
    private readonly ICommandItem _systemCommand;
    private readonly ICommandItem _chatGptCommand;
    private readonly ICommandItem _codexCommand;

    public JUtilityPaletteCommandsProvider()
    {
        Id = "com.julian.jutilitypalette";
        DisplayName = "J Utility Palette - Project Dashboard";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var dashboardPage = new ProjectDashboardPage(_store) { Id = DashboardCommandId };
        _dashboardCommand = new CommandItem(dashboardPage)
        {
            Title = "J Project Dashboard",
            Subtitle = "Visible open buttons + per-row copy switches",
        };

        var manageProjectsPage = new ProjectClipboardPage(_store) { Id = ManageProjectsCommandId };
        _manageProjectsCommand = new CommandItem(manageProjectsPage)
        {
            Title = "Manage Project Rows",
            Subtitle = "Add, edit or delete project links",
        };

        var promptLibraryPage = new PromptLibraryPage(_store) { Id = PromptsCommandId };
        _promptLibraryCommand = new CommandItem(promptLibraryPage)
        {
            Title = "J Prompts",
            Subtitle = "Reusable prompts + composable instructions",
        };

        var systemShortcutsPage = new SystemShortcutsPage { Id = SystemCommandId };
        _systemCommand = new CommandItem(systemShortcutsPage)
        {
            Title = "J System",
            Subtitle = "Hosts, environment variables, and Task Manager",
        };

        var chatGptLauncher = new JOpenUrlCommand(AppLauncher.ChatGptUrl, "Open ChatGPT", ChatGptCommandId);
        _chatGptCommand = new CommandItem(chatGptLauncher) { Title = "ChatGPT", Subtitle = "Open ChatGPT" };

        var codexLauncher = new JOpenUrlCommand(AppLauncher.CodexNewChatUri, "Open Codex", CodexCommandId);
        _codexCommand = new CommandItem(codexLauncher) { Title = "Codex", Subtitle = "Open a new local Codex chat" };

        _commands =
        [
            _dashboardCommand,
            _manageProjectsCommand,
            _promptLibraryCommand,
            new CommandItem(new RecentPromptsPage(_store)) { Title = "J Recent Prompts", Subtitle = "Last 25 prompts you actually used" },
            new CommandItem(new QuickLinksPage(_store)) { Title = "J Quick Links", Subtitle = "Unpaired temporary links" },
            _systemCommand,
            _chatGptCommand,
            _codexCommand,
        ];

        _fallbackCommands =
        [
            new PromptFallbackCommandItem(_store, 0, "j", PromptFallbackAction.Copy),
            new PromptFallbackCommandItem(_store, 1, "j", PromptFallbackAction.Copy),
            new PromptFallbackCommandItem(_store, 2, "j", PromptFallbackAction.Copy),
            new PromptFallbackCommandItem(_store, 0, "jg", PromptFallbackAction.ChatGpt),
            new PromptFallbackCommandItem(_store, 1, "jg", PromptFallbackAction.ChatGpt),
            new PromptFallbackCommandItem(_store, 2, "jg", PromptFallbackAction.ChatGpt),
            new PromptFallbackCommandItem(_store, 0, "jc", PromptFallbackAction.Codex),
            new PromptFallbackCommandItem(_store, 1, "jc", PromptFallbackAction.Codex),
            new PromptFallbackCommandItem(_store, 2, "jc", PromptFallbackAction.Codex),
            new SystemShortcutFallbackCommandItem(0),
            new SystemShortcutFallbackCommandItem(1),
            new SystemShortcutFallbackCommandItem(2),
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;
    public override IFallbackCommandItem[] FallbackCommands() => _fallbackCommands;

    public override ICommandItem? GetCommandItem(string id) => id switch
    {
        PromptsCommandId => _promptLibraryCommand,
        DashboardCommandId => _dashboardCommand,
        ManageProjectsCommandId => _manageProjectsCommand,
        SystemCommandId => _systemCommand,
        ChatGptCommandId => _chatGptCommand,
        CodexCommandId => _codexCommand,
        _ => null,
    };
}
