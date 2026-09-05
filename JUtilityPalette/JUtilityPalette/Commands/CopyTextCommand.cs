using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class CopyTextCommand : InvokableCommand
{
    private readonly string _text;
    private readonly string _toast;

    public CopyTextCommand(string text, string name = "Copy", string toast = "Copied")
    {
        _text = text;
        _toast = toast;
        Name = name;
    }

    public override CommandResult Invoke()
    {
        ClipboardText.Set(_text);
        return CommandResult.ShowToast(_toast);
    }
}
