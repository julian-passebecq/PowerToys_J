using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Pages;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette;

public sealed partial class JUtilityPaletteCommandsProvider : CommandProvider
{
    private const string PromptsCommandId = "com.julian.jutilitypalette.prompts";
    private const string ProjectsCommandId = "com.julian.jutilitypalette.projects";

    private readonly LibraryStore _store = new();
    private readonly ICommandItem[] _commands;
    private readonly IFallbackCommandItem[] _fallbackCommands;
    private readonly ICommandItem _promptLibraryCommand;
    private readonly ICommandItem _projectsCommand;

    public JUtilityPaletteCommandsProvider()
    {
        Id = "com.julian.jutilitypalette";
        DisplayName = "J Utility Palette - Minimal";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        var promptLibraryPage = new PromptLibraryPage(_store)
        {
            Id = PromptsCommandId,
        };
        _promptLibraryCommand = new CommandItem(promptLibraryPage)
        {
            Title = "J Prompts",
            Subtitle = "Reusable copy/paste prompts and instructions",
        };

        var projectsPage = new ProjectClipboardPage(_store)
        {
            Id = ProjectsCommandId,
        };
        _projectsCommand = new CommandItem(projectsPage)
        {
            Title = "J Project Clipboard",
            Subtitle = "Paired repo/site links with copy-row controls",
        };

        _commands =
        [
            _projectsCommand,
            _promptLibraryCommand,
            new CommandItem(new RecentPromptsPage(_store))
            {
                Title = "J Recent Prompts",
                Subtitle = "Remember the last prompts you actually used",
            },
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
        ];
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override IFallbackCommandItem[] FallbackCommands() => _fallbackCommands;

    public override ICommandItem? GetCommandItem(string id) => id switch
    {
        PromptsCommandId => _promptLibraryCommand,
        ProjectsCommandId => _projectsCommand,
        _ => null,
    };
}
