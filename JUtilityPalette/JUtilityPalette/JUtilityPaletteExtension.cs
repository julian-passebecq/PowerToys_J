using System.Runtime.InteropServices;
using Microsoft.CommandPalette.Extensions;

namespace JUtilityPalette;

[Guid("8A4E0B65-6F4C-4E5A-A8FD-3F88D1C8D7A9")]
public sealed partial class JUtilityPaletteExtension : IExtension, IDisposable
{
    private readonly ManualResetEvent _disposed;
    private readonly JUtilityPaletteCommandsProvider _provider = new();

    public JUtilityPaletteExtension(ManualResetEvent disposed)
    {
        _disposed = disposed;
    }

    public object? GetProvider(ProviderType providerType) => providerType switch
    {
        ProviderType.Commands => _provider,
        _ => null,
    };

    public void Dispose() => _disposed.Set();
}
