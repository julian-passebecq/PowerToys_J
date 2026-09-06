using JUtilityPalette.Commands;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using JCopyTextCommand = JUtilityPalette.Commands.CopyTextCommand;
using JOpenUrlCommand = JUtilityPalette.Commands.OpenUrlCommand;

namespace JUtilityPalette.Pages;

internal sealed partial class ProjectClipboardPage : ListPage
{
    private readonly LibraryStore _store;

    public ProjectClipboardPage(LibraryStore store)
    {
        _store = store;
        Title = "J Project Clipboard";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        PlaceholderText = "Search project, category, repo, site, note...";
        ShowDetails = true;
        _store.Changed += (_, _) => RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        List<IListItem> items =
        [
            new ListItem(new EditProjectPage(_store, null))
            {
                Title = "+ Add project row",
                Subtitle = "Name + GitHub + site + optional third link",
            },
        ];

        string all = LibraryStore.FormatAllProjectLines(_store.Projects);
        if (!string.IsNullOrWhiteSpace(all))
        {
            items.Add(new ListItem(new JCopyTextCommand(all, "Copy all project rows", "All included project rows copied"))
            {
                Title = "Copy all included rows",
                Subtitle = "One project per line; each row respects its copy switches",
            });
        }

        string query = SearchText?.Trim() ?? string.Empty;
        foreach (ProjectLinkEntry project in _store.Projects.Where(x => Matches(x, query)))
        {
            items.Add(BuildProjectItem(project));
        }

        return [.. items];
    }

    private IListItem BuildProjectItem(ProjectLinkEntry project)
    {
        string preferredUrl = PreferredUrl(project);
        string rowText = LibraryStore.FormatProjectLine(project);
        var primary = new JOpenUrlCommand(preferredUrl, "Open preferred link");

        List<IDetailsElement> metadata = [];
        if (!string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            metadata.Add(LinkElement("GitHub", project.RepoUrl, "Open repository"));
        }

        if (!string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            metadata.Add(LinkElement("Site", project.SiteUrl, "Open deployed site"));
        }

        if (!string.IsNullOrWhiteSpace(project.ExtraUrl))
        {
            metadata.Add(LinkElement(project.ExtraLabel, project.ExtraUrl, $"Open {project.ExtraLabel}"));
        }

        List<ICommand> copyCommands = [];
        if (!string.IsNullOrWhiteSpace(rowText))
        {
            copyCommands.Add(new JCopyTextCommand(rowText, "Copy row", "Project row copied"));
        }

        if (!string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            copyCommands.Add(new JCopyTextCommand(project.RepoUrl, "Copy GitHub", "GitHub URL copied"));
        }

        if (!string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            copyCommands.Add(new JCopyTextCommand(project.SiteUrl, "Copy site", "Site URL copied"));
        }

        if (!string.IsNullOrWhiteSpace(project.ExtraUrl))
        {
            copyCommands.Add(new JCopyTextCommand(project.ExtraUrl, $"Copy {project.ExtraLabel}", $"{project.ExtraLabel} URL copied"));
        }

        metadata.Add(new DetailsElement
        {
            Key = "Copy",
            Data = new DetailsCommands
            {
                Commands = [.. copyCommands],
            },
        });

        string included = BuildIncludedSummary(project);
        return new ListItem(primary)
        {
            Title = project.Name,
            Subtitle = $"{project.Category} · copy: {included}",
            Tags = [new Tag(project.IncludeInCopyAll ? "Copy all" : "Row only")],
            Details = new Details
            {
                Title = project.Name,
                Body = string.IsNullOrWhiteSpace(project.Note)
                    ? "Click a link to open it. Use the Copy commands for clipboard actions."
                    : project.Note,
                Size = ContentSize.Medium,
                Metadata = [.. metadata],
            },
            MoreCommands =
            [
                new CommandContextItem(new JCopyTextCommand(rowText, "Copy row", "Project row copied")) { Title = "Copy row" },
                new CommandContextItem(new EditProjectPage(_store, project)) { Title = "Edit row / copy switches" },
                new CommandContextItem(new DeleteProjectCommand(_store, project.Id)) { Title = "Delete", IsCritical = true },
            ],
        };
    }

    private static DetailsElement LinkElement(string key, string url, string text) => new()
    {
        Key = key,
        Data = new DetailsLink
        {
            Text = text,
            Link = new Uri(url),
        },
    };

    private static string PreferredUrl(ProjectLinkEntry project)
    {
        if (!string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            return project.SiteUrl;
        }

        if (!string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            return project.RepoUrl;
        }

        return project.ExtraUrl;
    }

    private static string BuildIncludedSummary(ProjectLinkEntry project)
    {
        List<string> fields = [];
        if (project.CopyName)
        {
            fields.Add("name");
        }

        if (project.CopyRepo && !string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            fields.Add("repo");
        }

        if (project.CopySite && !string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            fields.Add("site");
        }

        if (project.CopyExtra && !string.IsNullOrWhiteSpace(project.ExtraUrl))
        {
            fields.Add(project.ExtraLabel);
        }

        return fields.Count == 0 ? "nothing" : string.Join(" + ", fields);
    }

    private static bool Matches(ProjectLinkEntry project, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return project.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.Note.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.RepoUrl.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.SiteUrl.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.ExtraLabel.Contains(query, StringComparison.OrdinalIgnoreCase)
            || project.ExtraUrl.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
