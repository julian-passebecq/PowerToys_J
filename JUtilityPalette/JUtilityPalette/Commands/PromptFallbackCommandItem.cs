using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Pages;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions.Toolkit;
using JCopyTextCommand = JUtilityPalette.Commands.CopyTextCommand;

namespace JUtilityPalette.Commands;

internal enum PromptFallbackAction
{
    Copy,
    ChatGpt,
    Codex,
}

internal sealed partial class PromptFallbackCommandItem : FallbackCommandItem
{
    private readonly LibraryStore _store;
    private readonly int _rank;
    private readonly string _prefix;
    private readonly PromptFallbackAction _action;

    public PromptFallbackCommandItem(LibraryStore store, int rank, string prefix, PromptFallbackAction action)
        : base(GetDisplayName(action), $"com.julian.jutilitypalette.prompt-fallback.{action.ToString().ToLowerInvariant()}.{rank}")
    {
        _store = store;
        _rank = rank;
        _prefix = prefix;
        _action = action;
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Hide();
    }

    public override void UpdateQuery(string query)
    {
        string normalized = query.TrimStart();
        if (!normalized.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
        {
            Hide();
            return;
        }

        string search = normalized[_prefix.Length..].Trim();
        bool promptsOnly = _action != PromptFallbackAction.Copy;
        IReadOnlyList<PromptEntry> matches = search.Length == 0
            ? PromptMatcher.Top(_store.Prompts, promptsOnly)
            : PromptMatcher.Rank(_store.Prompts, search, promptsOnly);

        PromptEntry? match = matches.Skip(_rank).FirstOrDefault();
        if (match is null)
        {
            Hide();
            return;
        }

        Apply(match);
    }

    private void Apply(PromptEntry prompt)
    {
        bool isPrompt = string.Equals(prompt.Kind, "Prompt", StringComparison.OrdinalIgnoreCase);
        bool isTemplate = isPrompt && PromptTemplate.HasVariables(prompt.Body);

        Command = isTemplate
            ? new ComposePromptPage(_store, prompt)
            : !isPrompt
                ? new JCopyTextCommand(prompt.Body, "Copy", "Instruction copied")
                : _action switch
                {
                    PromptFallbackAction.ChatGpt => new CopyPromptAndOpenCommand(_store, prompt, AppLauncher.ChatGptUrl, "Copy + open ChatGPT"),
                    PromptFallbackAction.Codex => new OpenPromptInCodexCommand(_store, prompt),
                    _ => new CopyPromptCommand(_store, prompt),
                };

        Title = prompt.IsPinned ? $"★ {prompt.Title}" : prompt.Title;
        Subtitle = $"{GetActionLabel(_action)} · {prompt.Kind} · {prompt.Category}{(isTemplate ? " · fill template" : string.Empty)}";
    }

    private void Hide()
    {
        Command = null;
        Title = string.Empty;
        Subtitle = string.Empty;
    }

    private static string GetDisplayName(PromptFallbackAction action) => action switch
    {
        PromptFallbackAction.ChatGpt => "J → ChatGPT",
        PromptFallbackAction.Codex => "J → Codex",
        _ => "J Prompts",
    };

    private static string GetActionLabel(PromptFallbackAction action) => action switch
    {
        PromptFallbackAction.ChatGpt => "J ChatGPT",
        PromptFallbackAction.Codex => "J Codex",
        _ => "J",
    };
}
