using System.Text.Json;
using JUtilityPalette.Models;

namespace JUtilityPalette.Data;

internal sealed class LibraryStore
{
    private const int CurrentSchemaVersion = 1;
    private const int MaxRecentPrompts = 25;
    private readonly object _gate = new();
    private readonly string _path;
    private readonly string _backupPath;
    private LibraryState _state;

    public event EventHandler? Changed;

    public LibraryStore()
    {
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JUtilityPalette");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "library.json");
        _backupPath = Path.Combine(directory, "library.backup.json");
        _state = LoadOrCreate();
    }

    public IReadOnlyList<PromptEntry> Prompts
    {
        get
        {
            lock (_gate)
            {
                return _state.Prompts
                    .OrderByDescending(x => x.IsPinned)
                    .ThenByDescending(x => x.UpdatedUtc)
                    .ToArray();
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

    public IReadOnlyList<RecentPromptEntry> RecentPrompts
    {
        get
        {
            lock (_gate)
            {
                return _state.RecentPrompts.OrderByDescending(x => x.CreatedUtc).ToArray();
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

    public void TogglePromptPinned(Guid id)
    {
        lock (_gate)
        {
            PromptEntry? prompt = _state.Prompts.FirstOrDefault(x => x.Id == id);
            if (prompt is null)
            {
                return;
            }

            prompt.IsPinned = !prompt.IsPinned;
            prompt.UpdatedUtc = DateTimeOffset.UtcNow;
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

    public void AddRecentPrompt(string title, string text, Guid sourcePromptId)
    {
        string normalized = text.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        lock (_gate)
        {
            _state.RecentPrompts.RemoveAll(x => string.Equals(x.Text, normalized, StringComparison.Ordinal));
            _state.RecentPrompts.Add(new RecentPromptEntry
            {
                SourcePromptId = sourcePromptId,
                Title = Normalize(title, "Prompt"),
                Text = normalized,
                CreatedUtc = DateTimeOffset.UtcNow,
            });

            if (_state.RecentPrompts.Count > MaxRecentPrompts)
            {
                foreach (RecentPromptEntry stale in _state.RecentPrompts
                    .OrderByDescending(x => x.CreatedUtc)
                    .Skip(MaxRecentPrompts)
                    .ToArray())
                {
                    _state.RecentPrompts.Remove(stale);
                }
            }

            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteRecentPrompt(Guid id)
    {
        lock (_gate)
        {
            _state.RecentPrompts.RemoveAll(x => x.Id == id);
            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private LibraryState LoadOrCreate()
    {
        if (TryLoad(_path, out LibraryState? loaded))
        {
            return NormalizeLoadedState(loaded!);
        }

        if (TryLoad(_backupPath, out LibraryState? backup))
        {
            LibraryState recovered = NormalizeLoadedState(backup!);
            try
            {
                File.Copy(_backupPath, _path, true);
            }
            catch
            {
            }

            return recovered;
        }

        LibraryState seeded = CreateSeedState();
        string json = JsonSerializer.Serialize(seeded, LibraryJsonContext.Default.LibraryState);
        File.WriteAllText(_path, json);
        return seeded;
    }

    private static bool TryLoad(string path, out LibraryState? state)
    {
        state = null;
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            state = JsonSerializer.Deserialize(File.ReadAllText(path), LibraryJsonContext.Default.LibraryState);
            return state is not null;
        }
        catch
        {
            return false;
        }
    }

    private static LibraryState NormalizeLoadedState(LibraryState state)
    {
        state.SchemaVersion = CurrentSchemaVersion;
        state.Prompts ??= [];
        state.Links ??= [];
        state.RecentPrompts ??= [];

        foreach (QuickLinkEntry link in state.Links)
        {
            if (string.Equals(link.Title, "Codex", StringComparison.OrdinalIgnoreCase)
                && string.Equals(link.Url, "https://chatgpt.com/codex", StringComparison.OrdinalIgnoreCase))
            {
                link.Url = "codex://threads/new";
            }
        }

        return state;
    }

    private void SaveUnsafe()
    {
        _state.SchemaVersion = CurrentSchemaVersion;
        string json = JsonSerializer.Serialize(_state, LibraryJsonContext.Default.LibraryState);
        string temp = _path + ".tmp";
        File.WriteAllText(temp, json);

        if (File.Exists(_path))
        {
            try
            {
                File.Copy(_path, _backupPath, true);
            }
            catch
            {
            }
        }

        File.Move(temp, _path, true);
    }

    private static string Normalize(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static LibraryState CreateSeedState() => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        Prompts =
        [
            new PromptEntry
            {
                Title = "Debug + improve",
                Category = "Development",
                Kind = "Prompt",
                IsPinned = true,
                Body = "Audit the current project, identify concrete bugs and weak points, test the critical paths, then implement focused improvements. Preserve working behavior and explain any remaining limitations.",
            },
            new PromptEntry
            {
                Title = "Preserve existing features",
                Category = "Development",
                Kind = "Instruction",
                IsPinned = true,
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
            new QuickLinkEntry { Title = "Codex", Category = "AI", Url = "codex://threads/new" },
            new QuickLinkEntry { Title = "GitHub", Category = "Dev", Url = "https://github.com/" },
        ],
    };
}
