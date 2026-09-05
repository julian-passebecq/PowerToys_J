using System.Text.Json;
using JUtilityPalette.Models;

namespace JUtilityPalette.Data;

internal sealed class LibraryStore
{
    private readonly object _gate = new();
    private readonly string _path;
    private LibraryState _state;

    public event EventHandler? Changed;

    public LibraryStore()
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JUtilityPalette");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "library.json");
        _state = LoadOrCreate();
    }

    public IReadOnlyList<PromptEntry> Prompts
    {
        get
        {
            lock (_gate)
            {
                return _state.Prompts.OrderByDescending(x => x.UpdatedUtc).ToArray();
            }
        }
    }

    public IReadOnlyList<QuickLinkEntry> Links
    {
        get
        {
            lock (_gate)
            {
                return _state.Links.OrderBy(x => x.Category).ThenBy(x => x.Title).ToArray();
            }
        }
    }

    public void UpsertPrompt(Guid id, string title, string category, string kind, string body)
    {
        lock (_gate)
        {
            PromptEntry? existing = _state.Prompts.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                _state.Prompts.Add(new PromptEntry
                {
                    Id = id == Guid.Empty ? Guid.NewGuid() : id,
                    Title = title.Trim(),
                    Category = Normalize(category, "General"),
                    Kind = string.Equals(kind, "Instruction", StringComparison.OrdinalIgnoreCase) ? "Instruction" : "Prompt",
                    Body = body.Trim(),
                    UpdatedUtc = DateTimeOffset.UtcNow,
                });
            }
            else
            {
                existing.Title = title.Trim();
                existing.Category = Normalize(category, "General");
                existing.Kind = string.Equals(kind, "Instruction", StringComparison.OrdinalIgnoreCase) ? "Instruction" : "Prompt";
                existing.Body = body.Trim();
                existing.UpdatedUtc = DateTimeOffset.UtcNow;
            }

            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void DeletePrompt(Guid id)
    {
        lock (_gate)
        {
            _state.Prompts.RemoveAll(x => x.Id == id);
            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void UpsertLink(Guid id, string title, string category, string url)
    {
        string normalizedUrl = url.Trim();
        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out _))
        {
            normalizedUrl = "https://" + normalizedUrl;
        }

        lock (_gate)
        {
            QuickLinkEntry? existing = _state.Links.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                _state.Links.Add(new QuickLinkEntry
                {
                    Id = id == Guid.Empty ? Guid.NewGuid() : id,
                    Title = title.Trim(),
                    Category = Normalize(category, "Later"),
                    Url = normalizedUrl,
                    UpdatedUtc = DateTimeOffset.UtcNow,
                });
            }
            else
            {
                existing.Title = title.Trim();
                existing.Category = Normalize(category, "Later");
                existing.Url = normalizedUrl;
                existing.UpdatedUtc = DateTimeOffset.UtcNow;
            }

            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteLink(Guid id)
    {
        lock (_gate)
        {
            _state.Links.RemoveAll(x => x.Id == id);
            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private LibraryState LoadOrCreate()
    {
        try
        {
            if (File.Exists(_path))
            {
                string json = File.ReadAllText(_path);
                LibraryState? loaded = JsonSerializer.Deserialize(json, LibraryJsonContext.Default.LibraryState);
                if (loaded is not null)
                {
                    return loaded;
                }
            }
        }
        catch
        {
        }

        LibraryState seeded = CreateSeedState();
        _state = seeded;
        SaveUnsafe();
        return seeded;
    }

    private void SaveUnsafe()
    {
        string json = JsonSerializer.Serialize(_state, LibraryJsonContext.Default.LibraryState);
        string temp = _path + ".tmp";
        File.WriteAllText(temp, json);
        File.Move(temp, _path, true);
    }

    private static string Normalize(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static LibraryState CreateSeedState() => new()
    {
        Prompts =
        [
            new PromptEntry
            {
                Title = "Debug + improve",
                Category = "Development",
                Kind = "Prompt",
                Body = "Audit the current project, identify concrete bugs and weak points, test the critical paths, then implement focused improvements. Preserve working behavior and explain any remaining limitations.",
            },
            new PromptEntry
            {
                Title = "Preserve existing features",
                Category = "Development",
                Kind = "Instruction",
                Body = "Do not remove existing working features just to simplify the implementation. Build on the current project unless a replacement is clearly safer and justified.",
            },
            new PromptEntry
            {
                Title = "Use current sources",
                Category = "Research",
                Kind = "Instruction",
                Body = "When facts may have changed, verify them against current authoritative sources before making implementation decisions.",
            },
        ],
        Links =
        [
            new QuickLinkEntry { Title = "ChatGPT", Category = "AI", Url = "https://chatgpt.com/" },
            new QuickLinkEntry { Title = "Codex", Category = "AI", Url = "https://chatgpt.com/codex" },
            new QuickLinkEntry { Title = "GitHub", Category = "Dev", Url = "https://github.com/" },
        ],
    };
}
