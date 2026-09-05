using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class CaptureClipboardPromptCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly string _kind;

    public CaptureClipboardPromptCommand(LibraryStore store, string kind)
    {
        _store = store;
        _kind = string.Equals(kind, "Instruction", StringComparison.OrdinalIgnoreCase) ? "Instruction" : "Prompt";
        Name = _kind == "Prompt" ? "Save clipboard as prompt" : "Save clipboard as instruction";
    }

    public override CommandResult Invoke()
    {
        try
        {
            string? raw = ClipboardText.Get();
            string text = raw?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return CommandResult.ShowToast("Clipboard does not contain text");
            }

            PromptEntry? existing = _store.Prompts.FirstOrDefault(x =>
                string.Equals(x.Kind, _kind, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Body.Trim(), text, StringComparison.Ordinal));
            if (existing is not null)
            {
                return CommandResult.ShowToast($"Already saved as {existing.Title}");
            }

            _store.UpsertPrompt(Guid.Empty, MakeTitle(text), "Clipboard", _kind, text);
            return CommandResult.ShowToast(_kind == "Prompt" ? "Clipboard saved as prompt" : "Clipboard saved as instruction");
        }
        catch
        {
            return CommandResult.ShowToast("Could not read the clipboard");
        }
    }

    private static string MakeTitle(string text)
    {
        string firstLine = text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Clipboard prompt";

        firstLine = firstLine.Replace('\t', ' ').Trim();
        return firstLine.Length <= 60 ? firstLine : firstLine[..57] + "...";
    }
}

internal sealed partial class SaveTextAsPromptCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly string _title;
    private readonly string _text;

    public SaveTextAsPromptCommand(LibraryStore store, string title, string text)
    {
        _store = store;
        _title = title;
        _text = text;
        Name = "Save to prompt library";
    }

    public override CommandResult Invoke()
    {
        PromptEntry? existing = _store.Prompts.FirstOrDefault(x =>
            x.Kind == "Prompt" && string.Equals(x.Body.Trim(), _text.Trim(), StringComparison.Ordinal));
        if (existing is not null)
        {
            return CommandResult.ShowToast($"Already saved as {existing.Title}");
        }

        _store.UpsertPrompt(Guid.Empty, _title, "Saved", "Prompt", _text);
        return CommandResult.ShowToast("Saved to prompt library");
    }
}
