using System.Text.Json;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;

var tests = new (string Name, Action Body)[]
{
    ("Template variables", TestTemplateVariables),
    ("Prompt ranking", TestPromptRanking),
    ("Recent prompt cap and dedupe", TestRecentPromptCapAndDedupe),
    ("Backup recovery", TestBackupRecovery),
    ("Legacy Codex link migration", TestLegacyCodexMigration),
    ("Named event bridge", TestNamedEventBridge),
    ("System shortcut ranking", TestSystemShortcutRanking),
};

int failed = 0;
foreach ((string name, Action body) in tests)
{
    try
    {
        body();
        Console.WriteLine($"PASS  {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL  {name}: {ex.Message}");
    }
}

Console.WriteLine($"{tests.Length - failed}/{tests.Length} smoke tests passed.");
return failed == 0 ? 0 : 1;

static void TestTemplateVariables()
{
    const string template = "Audit {{project}} for {{focus}}. Repeat {{Project}}.";
    IReadOnlyList<string> variables = PromptTemplate.GetVariables(template);
    Assert(variables.SequenceEqual(["project", "focus"]), "Variables should be unique, ordered, and case-insensitive.");

    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["project"] = "PowerToys_J",
        ["focus"] = string.Empty,
    };

    string filled = PromptTemplate.Fill(template, values);
    Assert(filled == "Audit PowerToys_J for {{focus}}. Repeat PowerToys_J.", "Filled values or unresolved placeholders are wrong.");
    Assert(!PromptTemplate.HasVariables("plain prompt"), "Plain prompts must not be treated as templates.");
}

static void TestPromptRanking()
{
    PromptEntry[] prompts =
    [
        new() { Title = "Debug + improve", Kind = "Prompt", Category = "Development", Body = "Audit bugs", IsPinned = true },
        new() { Title = "Debug instruction", Kind = "Instruction", Category = "Development", Body = "Debug carefully" },
        new() { Title = "Current sources", Kind = "Prompt", Category = "Research", Body = "Verify current sources" },
    ];

    IReadOnlyList<PromptEntry> ranked = PromptMatcher.Rank(prompts, "debug");
    Assert(ranked.Count == 2, "Both prompt and instruction should match normal recall.");
    Assert(ranked[0].Title == "Debug + improve", "Pinned exact-title-leading prompt should rank first.");

    IReadOnlyList<PromptEntry> promptOnly = PromptMatcher.Rank(prompts, "debug", promptsOnly: true);
    Assert(promptOnly.Count == 1 && promptOnly[0].Kind == "Prompt", "ChatGPT/Codex recall must exclude instruction-only entries.");
}

static void TestRecentPromptCapAndDedupe()
{
    string directory = NewTempDirectory();
    try
    {
        var store = new LibraryStore(directory);
        for (int i = 0; i < 30; i++)
        {
            store.AddRecentPrompt($"Prompt {i}", $"Text {i}", Guid.Empty);
        }

        Assert(store.RecentPrompts.Count == 25, "Recent prompt history must stay capped at 25.");
        Assert(store.RecentPrompts[0].Text == "Text 29", "Newest recent prompt should be first.");
        Assert(store.RecentPrompts.All(x => x.Text != "Text 0"), "Oldest entries should be evicted.");

        store.AddRecentPrompt("Reused", "Text 20", Guid.Empty);
        Assert(store.RecentPrompts.Count == 25, "Reusing an exact prompt must not create a duplicate.");
        Assert(store.RecentPrompts[0].Title == "Reused" && store.RecentPrompts[0].Text == "Text 20", "Reused prompt should move to the top.");
    }
    finally
    {
        DeleteDirectory(directory);
    }
}

static void TestBackupRecovery()
{
    string directory = NewTempDirectory();
    try
    {
        var store = new LibraryStore(directory);
        store.UpsertPrompt(Guid.Empty, "Alpha", "Tests", "Prompt", "alpha body");
        store.UpsertPrompt(Guid.Empty, "Beta", "Tests", "Prompt", "beta body");

        File.WriteAllText(Path.Combine(directory, "library.json"), "{ broken json");
        var recovered = new LibraryStore(directory);
        Assert(recovered.Prompts.Any(x => x.Title == "Alpha"), "Backup recovery should restore the last good previous library.");
    }
    finally
    {
        DeleteDirectory(directory);
    }
}

static void TestLegacyCodexMigration()
{
    string directory = NewTempDirectory();
    try
    {
        var state = new LibraryState
        {
            Links =
            [
                new QuickLinkEntry { Title = "Codex", Category = "AI", Url = "https://chatgpt.com/codex" },
            ],
        };
        string json = JsonSerializer.Serialize(state, LibraryJsonContext.Default.LibraryState);
        File.WriteAllText(Path.Combine(directory, "library.json"), json);

        var store = new LibraryStore(directory);
        Assert(store.Links.Single().Url == "codex://threads/new", "Legacy Codex links should migrate to the desktop protocol.");
    }
    finally
    {
        DeleteDirectory(directory);
    }
}

static void TestNamedEventBridge()
{
    string eventName = $"JUtilityPalette-Test-{Guid.NewGuid():N}";
    using var handle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
    Assert(PowerToysBridge.TrySignalEvent(eventName), "Bridge should signal an existing named event.");
    Assert(handle.WaitOne(TimeSpan.FromSeconds(1)), "Named event was not observed after signaling.");
    Assert(!PowerToysBridge.TrySignalEvent(eventName + "-missing"), "Bridge should fail cleanly when an event does not exist.");
}

static void TestSystemShortcutRanking()
{
    Assert(SystemShortcutCatalog.Rank("host")[0].Kind == SystemShortcutKind.HostsFileEditor, "host should resolve to Hosts File Editor.");
    Assert(SystemShortcutCatalog.Rank("env")[0].Kind == SystemShortcutKind.EnvironmentVariables, "env should resolve to Environment Variables.");
    Assert(SystemShortcutCatalog.Rank("gestionnaire")[0].Kind == SystemShortcutKind.TaskManager, "French task-manager alias should resolve correctly.");
    Assert(SystemShortcutCatalog.Rank(string.Empty).Count == 3, "Empty J System query should expose all three shortcuts.");
}

static string NewTempDirectory()
{
    string path = Path.Combine(Path.GetTempPath(), "JUtilityPalette.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    return path;
}

static void DeleteDirectory(string path)
{
    try
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }
    catch
    {
    }
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
