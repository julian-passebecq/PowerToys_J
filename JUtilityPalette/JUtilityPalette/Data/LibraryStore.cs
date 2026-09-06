using System.Text.Json;
using JUtilityPalette.Models;

namespace JUtilityPalette.Data;

internal sealed class LibraryStore
{
    private const int CurrentSchemaVersion = 2;
    private const int MaxRecentPrompts = 25;
    private readonly object _gate = new();
    private readonly string _path;
    private readonly string _backupPath;
    private LibraryState _state;

    public event EventHandler? Changed;

    public LibraryStore()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JUtilityPalette"))
    {
    }

    internal LibraryStore(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("A storage directory is required.", nameof(directory));
        }

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

    public IReadOnlyList<ProjectLinkEntry> Projects
    {
        get
        {
            lock (_gate)
            {
                return _state.Projects
                    .OrderBy(x => x.Category)
                    .ThenBy(x => x.Name)
                    .ToArray();
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

    public bool UpsertLink(Guid id, string title, string category, string url)
    {
        if (!TryNormalizeLinkUrl(url, out string normalizedUrl))
        {
            return false;
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
        return true;
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

    public bool UpsertProject(
        Guid id,
        string name,
        string category,
        string note,
        string repoUrl,
        string siteUrl,
        string extraLabel,
        string extraUrl,
        bool copyName,
        bool copyRepo,
        bool copySite,
        bool copyExtra,
        bool includeInCopyAll)
    {
        if (!TryNormalizeOptionalWebUrl(repoUrl, out string normalizedRepo)
            || !TryNormalizeOptionalWebUrl(siteUrl, out string normalizedSite)
            || !TryNormalizeOptionalWebUrl(extraUrl, out string normalizedExtra))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(normalizedRepo)
            && string.IsNullOrWhiteSpace(normalizedSite)
            && string.IsNullOrWhiteSpace(normalizedExtra))
        {
            return false;
        }

        lock (_gate)
        {
            ProjectLinkEntry? existing = _state.Projects.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                _state.Projects.Add(new ProjectLinkEntry
                {
                    Id = id == Guid.Empty ? Guid.NewGuid() : id,
                    Name = Normalize(name, "Untitled project"),
                    Category = Normalize(category, "Projects"),
                    Note = note.Trim(),
                    RepoUrl = normalizedRepo,
                    SiteUrl = normalizedSite,
                    ExtraLabel = Normalize(extraLabel, "Extra"),
                    ExtraUrl = normalizedExtra,
                    CopyName = copyName,
                    CopyRepo = copyRepo,
                    CopySite = copySite,
                    CopyExtra = copyExtra,
                    IncludeInCopyAll = includeInCopyAll,
                    UpdatedUtc = DateTimeOffset.UtcNow,
                });
            }
            else
            {
                existing.Name = Normalize(name, "Untitled project");
                existing.Category = Normalize(category, "Projects");
                existing.Note = note.Trim();
                existing.RepoUrl = normalizedRepo;
                existing.SiteUrl = normalizedSite;
                existing.ExtraLabel = Normalize(extraLabel, "Extra");
                existing.ExtraUrl = normalizedExtra;
                existing.CopyName = copyName;
                existing.CopyRepo = copyRepo;
                existing.CopySite = copySite;
                existing.CopyExtra = copyExtra;
                existing.IncludeInCopyAll = includeInCopyAll;
                existing.UpdatedUtc = DateTimeOffset.UtcNow;
            }

            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void UpdateProjectCopyFlags(Guid id, bool copyName, bool copyRepo, bool copySite, bool copyExtra, bool includeInCopyAll)
    {
        lock (_gate)
        {
            ProjectLinkEntry? project = _state.Projects.FirstOrDefault(x => x.Id == id);
            if (project is null)
            {
                return;
            }

            project.CopyName = copyName;
            project.CopyRepo = copyRepo;
            project.CopySite = copySite;
            project.CopyExtra = copyExtra;
            project.IncludeInCopyAll = includeInCopyAll;
            project.UpdatedUtc = DateTimeOffset.UtcNow;
            SaveUnsafe();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteProject(Guid id)
    {
        lock (_gate)
        {
            _state.Projects.RemoveAll(x => x.Id == id);
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

    internal static string FormatProjectLine(ProjectLinkEntry project)
    {
        List<string> parts = [];
        if (project.CopyName && !string.IsNullOrWhiteSpace(project.Name))
        {
            parts.Add(project.Name.Trim());
        }

        if (project.CopyRepo && !string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            parts.Add(project.RepoUrl);
        }

        if (project.CopySite && !string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            parts.Add(project.SiteUrl);
        }

        if (project.CopyExtra && !string.IsNullOrWhiteSpace(project.ExtraUrl))
        {
            parts.Add(project.ExtraUrl);
        }

        return string.Join(" ", parts);
    }

    internal static string FormatAllProjectLines(IEnumerable<ProjectLinkEntry> projects)
    {
        return string.Join(
            Environment.NewLine,
            projects
                .Where(x => x.IncludeInCopyAll)
                .Select(FormatProjectLine)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
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
        int incomingVersion = state.SchemaVersion;
        state.Prompts ??= [];
        state.Links ??= [];
        state.Projects ??= [];
        state.RecentPrompts ??= [];

        foreach (QuickLinkEntry link in state.Links)
        {
            if (string.Equals(link.Title, "Codex", StringComparison.OrdinalIgnoreCase)
                && string.Equals(link.Url, "https://chatgpt.com/codex", StringComparison.OrdinalIgnoreCase))
            {
                link.Url = "codex://threads/new";
            }
        }

        if (incomingVersion < 2 && state.Projects.Count == 0)
        {
            state.Projects.AddRange(CreateSeedProjects());
        }

        state.SchemaVersion = CurrentSchemaVersion;
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

    private static bool TryNormalizeLinkUrl(string url, out string normalizedUrl)
    {
        string raw = url.Trim();
        normalizedUrl = raw;
        if (raw.Length == 0)
        {
            return false;
        }

        bool hasExplicitScheme = raw.Contains("://", StringComparison.Ordinal);
        string candidate = hasExplicitScheme ? raw : "https://" + raw;
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? parsed))
        {
            return false;
        }

        if ((string.Equals(parsed.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(parsed.Host))
        {
            return false;
        }

        normalizedUrl = parsed.AbsoluteUri;
        return true;
    }

    private static bool TryNormalizeOptionalWebUrl(string url, out string normalizedUrl)
    {
        normalizedUrl = string.Empty;
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        if (!TryNormalizeLinkUrl(url, out string candidate)
            || !Uri.TryCreate(candidate, UriKind.Absolute, out Uri? parsed)
            || (!string.Equals(parsed.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        normalizedUrl = parsed.AbsoluteUri;
        return true;
    }

    private static string Normalize(string value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static List<ProjectLinkEntry> CreateSeedProjects() =>
    [
        new ProjectLinkEntry
        {
            Name = "VisualAlgo",
            Category = "Fluent2 J Consumers",
            Note = "Visual Algorithms consumer: repository + deployed validation site.",
            RepoUrl = "https://github.com/julian-passebecq/Fluent2_J_VisualAlgo",
            SiteUrl = "https://fluent2jvisualalgo.netlify.app/",
            CopyName = true,
            CopyRepo = true,
            CopySite = true,
            CopyExtra = false,
            IncludeInCopyAll = true,
        },
        new ProjectLinkEntry
        {
            Name = "CloudArchi",
            Category = "Fluent2 J Consumers",
            Note = "Cloud Architecture consumer: repository + deployed validation site.",
            RepoUrl = "https://github.com/julian-passebecq/Fluent2_J_CloudArchi",
            SiteUrl = "https://f2jcloudarchi.netlify.app/",
            CopyName = true,
            CopyRepo = true,
            CopySite = true,
            CopyExtra = false,
            IncludeInCopyAll = true,
        },
    ];

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
        Projects = CreateSeedProjects(),
    };
}
