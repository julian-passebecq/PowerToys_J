using System.Text.Json.Serialization;

namespace JUtilityPalette.Models;

internal sealed class PromptEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Kind { get; set; } = "Prompt";
    public string Body { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

internal sealed class QuickLinkEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Later";
    public string Url { get; set; } = string.Empty;
    public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

internal sealed class RecentPromptEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourcePromptId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}

internal sealed class LibraryState
{
    public int SchemaVersion { get; set; } = 1;
    public List<PromptEntry> Prompts { get; set; } = [];
    public List<QuickLinkEntry> Links { get; set; } = [];
    public List<RecentPromptEntry> RecentPrompts { get; set; } = [];
}

[JsonSerializable(typeof(LibraryState))]
[JsonSerializable(typeof(string))]
internal sealed partial class LibraryJsonContext : JsonSerializerContext
{
}
