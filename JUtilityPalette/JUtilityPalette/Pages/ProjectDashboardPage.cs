using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class ProjectDashboardPage : ContentPage
{
    private readonly ProjectDashboardForm _form;

    public ProjectDashboardPage(LibraryStore store)
    {
        Title = "J Project Dashboard";
        Name = "Open";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        _form = new ProjectDashboardForm(store);
        store.Changed += (_, _) => _form.Refresh();
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class ProjectDashboardForm : FormContent
{
    private readonly LibraryStore _store;

    public ProjectDashboardForm(LibraryStore store)
    {
        _store = store;
        Refresh();
    }

    internal void Refresh()
    {
        TemplateJson = BuildTemplate();
    }

    public override CommandResult SubmitForm(string payload)
    {
        JsonNode? input = JsonNode.Parse(payload);
        if (input is null)
        {
            return CommandResult.KeepOpen();
        }

        string action = input["action"]?.ToString() ?? string.Empty;
        if (string.Equals(action, "copyAll", StringComparison.Ordinal))
        {
            string all = BuildAllLinesFromInputs(input);
            return ClipboardText.TrySet(all)
                ? CommandResult.ShowToast("Included project rows copied")
                : CommandResult.ShowToast("Could not copy to the clipboard");
        }

        if (!Guid.TryParse(input["projectId"]?.ToString(), out Guid id))
        {
            return CommandResult.KeepOpen();
        }

        ProjectLinkEntry? project = _store.Projects.FirstOrDefault(x => x.Id == id);
        if (project is null)
        {
            return CommandResult.ShowToast("Project row no longer exists");
        }

        ProjectLinkEntry snapshot = SnapshotFromInputs(input, project);
        PersistFlags(snapshot);

        return action switch
        {
            "copyRow" => CopyRow(snapshot),
            "openRepo" => Open(snapshot.RepoUrl),
            "openSite" => Open(snapshot.SiteUrl),
            "openExtra" => Open(snapshot.ExtraUrl),
            _ => CommandResult.KeepOpen(),
        };
    }

    private CommandResult CopyRow(ProjectLinkEntry project)
    {
        string line = LibraryStore.FormatProjectLine(project);
        if (string.IsNullOrWhiteSpace(line))
        {
            return CommandResult.ShowToast("Select at least one field to copy");
        }

        return ClipboardText.TrySet(line)
            ? CommandResult.ShowToast("Project row copied")
            : CommandResult.ShowToast("Could not copy to the clipboard");
    }

    private static CommandResult Open(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return CommandResult.ShowToast("This link is empty");
        }

        return AppLauncher.TryOpen(url)
            ? CommandResult.Hide()
            : CommandResult.ShowToast("Could not open link");
    }

    private string BuildAllLinesFromInputs(JsonNode input)
    {
        List<ProjectLinkEntry> snapshots = [];
        foreach (ProjectLinkEntry project in _store.Projects)
        {
            ProjectLinkEntry snapshot = SnapshotFromInputs(input, project);
            PersistFlags(snapshot);
            snapshots.Add(snapshot);
        }

        return LibraryStore.FormatAllProjectLines(snapshots);
    }

    private void PersistFlags(ProjectLinkEntry project)
    {
        _store.UpdateProjectCopyFlags(
            project.Id,
            project.CopyName,
            project.CopyRepo,
            project.CopySite,
            project.CopyExtra,
            project.IncludeInCopyAll);
    }

    private static ProjectLinkEntry SnapshotFromInputs(JsonNode input, ProjectLinkEntry project)
    {
        string key = project.Id.ToString("N");
        return new ProjectLinkEntry
        {
            Id = project.Id,
            Name = project.Name,
            Category = project.Category,
            Note = project.Note,
            RepoUrl = project.RepoUrl,
            SiteUrl = project.SiteUrl,
            ExtraLabel = project.ExtraLabel,
            ExtraUrl = project.ExtraUrl,
            CopyName = Toggle(input[$"name_{key}"], project.CopyName),
            CopyRepo = Toggle(input[$"repo_{key}"], project.CopyRepo),
            CopySite = Toggle(input[$"site_{key}"], project.CopySite),
            CopyExtra = Toggle(input[$"extra_{key}"], project.CopyExtra),
            IncludeInCopyAll = Toggle(input[$"all_{key}"], project.IncludeInCopyAll),
            UpdatedUtc = project.UpdatedUtc,
        };
    }

    private string BuildTemplate()
    {
        var body = new StringBuilder();
        body.AppendLine("{ \"type\": \"TextBlock\", \"text\": \"Project clipboard\", \"size\": \"Large\", \"weight\": \"Bolder\" },");
        body.AppendLine("{ \"type\": \"TextBlock\", \"text\": \"Links open. Copy is always a separate button. Toggle exactly what each copied row contains.\", \"wrap\": true, \"isSubtle\": true },");

        ProjectLinkEntry[] projects = [.. _store.Projects];
        for (int i = 0; i < projects.Length; i++)
        {
            ProjectLinkEntry project = projects[i];
            string key = project.Id.ToString("N");
            body.Append(BuildProjectContainer(project, key));
            body.AppendLine(",");
        }

        body.AppendLine("{ \"type\": \"TextBlock\", \"text\": \"Copy all uses only rows with ‘All’ enabled.\", \"wrap\": true, \"isSubtle\": true }");

        return $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.6",
  "body": [
{{body}}
  ],
  "actions": [
    { "type": "Action.Submit", "title": "Copy all included rows", "data": { "action": "copyAll" } }
  ]
}
""";
    }

    private static string BuildProjectContainer(ProjectLinkEntry project, string key)
    {
        var actions = new List<string>();
        if (!string.IsNullOrWhiteSpace(project.RepoUrl))
        {
            actions.Add(Action("GitHub", "openRepo", project.Id));
        }

        if (!string.IsNullOrWhiteSpace(project.SiteUrl))
        {
            actions.Add(Action("Site", "openSite", project.Id));
        }

        if (!string.IsNullOrWhiteSpace(project.ExtraUrl))
        {
            actions.Add(Action(project.ExtraLabel, "openExtra", project.Id));
        }

        actions.Add(Action("Copy row", "copyRow", project.Id));
        string actionJson = string.Join(",", actions);
        string note = string.IsNullOrWhiteSpace(project.Note) ? project.Category : $"{project.Category} · {project.Note}";

        return $$"""
{
  "type": "Container",
  "separator": true,
  "spacing": "Medium",
  "items": [
    { "type": "TextBlock", "text": {{Encode(project.Name)}}, "weight": "Bolder", "wrap": true },
    { "type": "TextBlock", "text": {{Encode(note)}}, "isSubtle": true, "wrap": true, "spacing": "None" },
    {
      "type": "ColumnSet",
      "columns": [
        { "type": "Column", "width": "stretch", "items": [ { "type": "Input.Toggle", "id": "name_{{key}}", "title": "Name", "value": {{Encode(Bool(project.CopyName))}}, "valueOn": "true", "valueOff": "false" } ] },
        { "type": "Column", "width": "stretch", "items": [ { "type": "Input.Toggle", "id": "repo_{{key}}", "title": "Repo", "value": {{Encode(Bool(project.CopyRepo))}}, "valueOn": "true", "valueOff": "false" } ] },
        { "type": "Column", "width": "stretch", "items": [ { "type": "Input.Toggle", "id": "site_{{key}}", "title": "Site", "value": {{Encode(Bool(project.CopySite))}}, "valueOn": "true", "valueOff": "false" } ] },
        { "type": "Column", "width": "stretch", "items": [ { "type": "Input.Toggle", "id": "extra_{{key}}", "title": {{Encode(string.IsNullOrWhiteSpace(project.ExtraUrl) ? "Extra" : project.ExtraLabel)}}, "value": {{Encode(Bool(project.CopyExtra))}}, "valueOn": "true", "valueOff": "false", "isEnabled": {{(string.IsNullOrWhiteSpace(project.ExtraUrl) ? "false" : "true")}} } ] },
        { "type": "Column", "width": "stretch", "items": [ { "type": "Input.Toggle", "id": "all_{{key}}", "title": "All", "value": {{Encode(Bool(project.IncludeInCopyAll))}}, "valueOn": "true", "valueOff": "false" } ] }
      ]
    },
    { "type": "ActionSet", "actions": [ {{actionJson}} ] }
  ]
}
""";
    }

    private static string Action(string title, string action, Guid projectId) =>
        $$"""{ "type": "Action.Submit", "title": {{Encode(title)}}, "data": { "action": {{Encode(action)}}, "projectId": {{Encode(projectId.ToString())}} } }""";

    private static bool Toggle(JsonNode? node, bool fallback) =>
        bool.TryParse(node?.ToString(), out bool value) ? value : fallback;

    private static string Bool(bool value) => value ? "true" : "false";

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
