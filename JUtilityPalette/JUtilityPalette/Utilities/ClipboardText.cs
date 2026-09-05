using Windows.ApplicationModel.DataTransfer;

namespace JUtilityPalette.Utilities;

internal static class ClipboardText
{
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
}
