using Windows.ApplicationModel.DataTransfer;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Utilities;

internal static class ClipboardText
{
    public static bool TrySet(string text)
    {
        try
        {
            Set(text);
            return true;
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"J Utility Palette clipboard write failed: {ex}");
            return false;
        }
    }

    public static void Set(string text)
    {
        Exception? error = null;
        Thread thread = new(() =>
        {
            try
            {
                DataPackage package = new();
                package.SetText(text);
                Clipboard.SetContent(package);
                Clipboard.Flush();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            throw error;
        }
    }

    public static string? Get()
    {
        Exception? error = null;
        string? text = null;
        Thread thread = new(() =>
        {
            try
            {
                Task<string?> task = GetTextAsync();
                task.ConfigureAwait(false);
                task.Wait();
                text = task.Result;
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            throw error;
        }

        return text;
    }

    private static async Task<string?> GetTextAsync()
    {
        DataPackageView content = Clipboard.GetContent();
        if (!content.Contains(StandardDataFormats.Text))
        {
            return null;
        }

        return await content.GetTextAsync();
    }
}
