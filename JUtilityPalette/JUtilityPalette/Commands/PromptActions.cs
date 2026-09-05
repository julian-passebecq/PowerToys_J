using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class CopyPromptCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly PromptEntry _prompt;

    public CopyPromptCommand(LibraryStore store, PromptEntry prompt)
    {
        _store = store;
        _prompt = prompt;
        Name = "Copy";
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_prompt.Body);
        _store.AddRecentPrompt(_prompt.Title, _prompt.Body, _prompt.Id);
        return CommandResult.ShowToast("Prompt copied");
    }
}

internal sealed partial class CopyPromptAndOpenCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly PromptEntry _prompt;
    private readonly string _url;

    public CopyPromptAndOpenCommand(LibraryStore store, PromptEntry prompt, string url, string name)
    {
        _store = store;
        _prompt = prompt;
        _url = url;
        Name = name;
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_prompt.Body);
        _store.AddRecentPrompt(_prompt.Title, _prompt.Body, _prompt.Id);
        return AppLauncher.TryOpen(_url)
            ? CommandResult.Dismiss()
            : CommandResult.ShowToast("Prompt copied, but the destination could not be opened");
    }
}

internal sealed partial class CopyTextAndOpenCommand : InvokableCommand
{
    private readonly string _text;
    private readonly string _url;

    public CopyTextAndOpenCommand(string text, string url, string name)
    {
        _text = text;
        _url = url;
        Name = name;
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_text);
        return AppLauncher.TryOpen(_url)
            ? CommandResult.Dismiss()
            : CommandResult.ShowToast("Copied, but the destination could not be opened");
    }
}

internal sealed partial class OpenPromptInCodexCommand : InvokableCommand
{
    private readonly LibraryStore _store;
    private readonly PromptEntry _prompt;

    public OpenPromptInCodexCommand(LibraryStore store, PromptEntry prompt)
    {
        _store = store;
        _prompt = prompt;
        Name = "Open in Codex";
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_prompt.Body);
        _store.AddRecentPrompt(_prompt.Title, _prompt.Body, _prompt.Id);
        return AppLauncher.TryOpenCodex(_prompt.Body)
            ? CommandResult.Dismiss()
            : CommandResult.ShowToast("Prompt copied, but Codex could not be opened");
    }
}

internal sealed partial class OpenTextInCodexCommand : InvokableCommand
{
    private readonly string _text;

    public OpenTextInCodexCommand(string text)
    {
        _text = text;
        Name = "Open in Codex";
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_text);
        return AppLauncher.TryOpenCodex(_text)
            ? CommandResult.Dismiss()
            : CommandResult.ShowToast("Prompt copied, but Codex could not be opened");
    }
}
