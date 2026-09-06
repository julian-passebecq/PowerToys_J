using System.Diagnostics;

namespace JUtilityPalette.Utilities;

internal static class AppLauncher
{
    public const string ChatGptUrl = "https://chatgpt.com/";
    public const string CodexNewChatUri = "codex://threads/new";
    private const int MaxCodexDeepLinkPromptChars = 6000;

    public static bool TryOpenChatGpt() => TryOpen(ChatGptUrl);

    public static bool TryOpenCodex(string? prompt = null) => TryOpen(BuildCodexTarget(prompt));

    internal static string BuildCodexTarget(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt) || prompt.Length > MaxCodexDeepLinkPromptChars)
        {
            return CodexNewChatUri;
        }

        return $"{CodexNewChatUri}?prompt={Uri.EscapeDataString(prompt)}";
    }

    public static bool TryOpen(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
