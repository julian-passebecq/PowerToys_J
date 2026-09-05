using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class OpenUrlCommand : InvokableCommand
{
    private readonly string _url;

    public OpenUrlCommand(string url, string name = "Open", string? id = null)
    {
        _url = url;
        Name = name;
        if (!string.IsNullOrWhiteSpace(id))
        {
            Id = id;
        }
    }

    public override CommandResult Invoke()
    {
        return AppLauncher.TryOpen(_url)
            ? CommandResult.Dismiss()
            : CommandResult.ShowToast("Could not open link");
    }
}
