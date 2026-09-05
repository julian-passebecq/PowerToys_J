using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace JUtilityPalette;

public static class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length == 0 || args[0] != "-RegisterProcessAsComServer")
        {
            return;
        }

        using ManualResetEvent disposed = new(false);
        global::Shmuelie.WinRTServer.ComServer server = new();
        JUtilityPaletteExtension extension = new(disposed);
        server.RegisterClass<JUtilityPaletteExtension, IExtension>(() => extension);
        server.Start();
        disposed.WaitOne();
        server.Stop();
        server.UnsafeDispose();
    }
}
