using System.Diagnostics;
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

        try
        {
            Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
            return CommandResult.Dismiss();
        }
        catch
        {
            return CommandResult.ShowToast("Prompt copied, but the browser could not be opened");
        }
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

        try
        {
            Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
            return CommandResult.Dismiss();
        }
        catch
        {
            return CommandResult.ShowToast("Copied, but the browser could not be opened");
        }
    }
}
