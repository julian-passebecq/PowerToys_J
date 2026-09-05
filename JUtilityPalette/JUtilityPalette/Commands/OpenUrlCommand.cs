using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class OpenUrlCommand : InvokableCommand
{
    private readonly string _url;

    public OpenUrlCommand(string url, string name = "Open")
    {
        _url = url;
        Name = name;
    }

    public override CommandResult Invoke()
    {
        Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true });
        return CommandResult.Dismiss();
    }
}
