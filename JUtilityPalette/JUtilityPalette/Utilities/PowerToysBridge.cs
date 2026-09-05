using System.Diagnostics;

namespace JUtilityPalette.Utilities;

internal static class PowerToysBridge
{
    internal const string HostsAdminEvent = @"Local\Hosts-ShowHostsAdminEvent-60ff44e2-efd3-43bf-928a-f4d269f98bec";
    internal const string EnvironmentVariablesAdminEvent = @"Local\PowerToysEnvironmentVariables-EnvironmentVariablesAdminEvent-8c95d2ad-047c-49a2-9e8b-b4656326cfb2";

    public static bool TryOpenHosts() => TrySignalEvent(HostsAdminEvent);

    public static bool TryOpenEnvironmentVariables() => TrySignalEvent(EnvironmentVariablesAdminEvent);

    public static bool TryOpenTaskManager() => TryStart("taskmgr.exe");

    internal static bool TrySignalEvent(string eventName)
    {
        try
        {
            using EventWaitHandle handle = EventWaitHandle.OpenExisting(eventName);
            return handle.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    internal static bool TryStart(string fileName, string? arguments = null)
    {
        try
        {
            Process.Start(new ProcessStartInfo(fileName)
            {
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true,
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
