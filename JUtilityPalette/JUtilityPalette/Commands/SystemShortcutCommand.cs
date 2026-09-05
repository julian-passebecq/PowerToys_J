using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Commands;

internal sealed partial class SystemShortcutCommand : InvokableCommand
{
    private readonly SystemShortcutDefinition _definition;

    public SystemShortcutCommand(SystemShortcutDefinition definition)
    {
        _definition = definition;
        Name = $"Open {definition.Title}";
    }

    public override CommandResult Invoke()
    {
        bool opened = _definition.Kind switch
        {
            SystemShortcutKind.HostsFileEditor => PowerToysBridge.TryOpenHosts(),
            SystemShortcutKind.EnvironmentVariables => PowerToysBridge.TryOpenEnvironmentVariables(),
            SystemShortcutKind.TaskManager => PowerToysBridge.TryOpenTaskManager(),
            _ => false,
        };

        if (opened)
        {
            return CommandResult.Dismiss();
        }

        string message = _definition.Kind switch
        {
            SystemShortcutKind.HostsFileEditor => "Hosts File Editor did not respond. Ensure the PowerToys Hosts utility is enabled.",
            SystemShortcutKind.EnvironmentVariables => "Environment Variables did not respond. Ensure the PowerToys utility is enabled.",
            _ => "Task Manager could not be opened.",
        };

        return CommandResult.ShowToast(message);
    }
}
