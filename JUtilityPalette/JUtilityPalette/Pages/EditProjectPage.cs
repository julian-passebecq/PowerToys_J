using System.Text.Json;
using System.Text.Json.Nodes;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class EditProjectPage : ContentPage
{
    private readonly EditProjectForm _form;

    public EditProjectPage(LibraryStore store, ProjectLinkEntry? entry)
    {
        Title = entry is null ? "Add project row" : "Edit project row";
        Name = entry is null ? "Add" : "Edit";
        _form = new EditProjectForm(store, entry);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class EditProjectForm : FormContent
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public EditProjectForm(LibraryStore store, ProjectLinkEntry? entry)
    {
        _store = store;
        _id = entry?.Id ?? Guid.Empty;

        string name = entry?.Name ?? string.Empty;
        string category = entry?.Category ?? "Projects";
        string note = entry?.Note ?? string.Empty;
        string repoUrl = entry?.RepoUrl ?? string.Empty;
        string siteUrl = entry?.SiteUrl ?? string.Empty;
        string extraLabel = entry?.ExtraLabel ?? "Extra";
        string extraUrl = entry?.ExtraUrl ?? string.Empty;

        TemplateJson = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.6",
  "body": [
    { "type": "TextBlock", "text": "One project = one copyable line with clickable links.", "wrap": true, "isSubtle": true },
    { "type": "Input.Text", "id": "name", "label": "Project name", "value": {{Encode(name)}}, "isRequired": true, "errorMessage": "Project name is required" },
    { "type": "Input.Text", "id": "category", "label": "Compartment / category", "value": {{Encode(category)}}, "placeholder": "Fluent2 J Consumers, Portfolio, Work..." },
    { "type": "Input.Text", "id": "note", "label": "Short note", "value": {{Encode(note)}}, "isMultiline": true, "placeholder": "Optional reminder: what this project is / what to validate" },
    { "type": "Input.Text", "id": "repoUrl", "label": "GitHub / repository URL", "value": {{Encode(repoUrl)}}, "placeholder": "https://github.com/..." },
    { "type": "Input.Text", "id": "siteUrl", "label": "Deployed site URL", "value": {{Encode(siteUrl)}}, "placeholder": "https://...netlify.app/" },
    { "type": "Input.Text", "id": "extraLabel", "label": "Third link label", "value": {{Encode(extraLabel)}}, "placeholder": "Netlify, Docs, API, Issue..." },
    { "type": "Input.Text", "id": "extraUrl", "label": "Third link URL", "value": {{Encode(extraUrl)}}, "placeholder": "Optional https://..." },
    { "type": "TextBlock", "text": "Copy-row contents", "weight": "Bolder", "spacing": "Medium" },
    { "type": "Input.Toggle", "id": "copyName", "title": "Include project name", "value": {{Encode(Bool(entry?.CopyName ?? true))}}, "valueOn": "true", "valueOff": "false" },
    { "type": "Input.Toggle", "id": "copyRepo", "title": "Include repository URL", "value": {{Encode(Bool(entry?.CopyRepo ?? true))}}, "valueOn": "true", "valueOff": "false" },
    { "type": "Input.Toggle", "id": "copySite", "title": "Include deployed site URL", "value": {{Encode(Bool(entry?.CopySite ?? true))}}, "valueOn": "true", "valueOff": "false" },
    { "type": "Input.Toggle", "id": "copyExtra", "title": "Include third URL", "value": {{Encode(Bool(entry?.CopyExtra ?? false))}}, "valueOn": "true", "valueOff": "false" },
    { "type": "Input.Toggle", "id": "includeInCopyAll", "title": "Include this row in Copy all", "value": {{Encode(Bool(entry?.IncludeInCopyAll ?? true))}}, "valueOn": "true", "valueOff": "false" }
  ],
  "actions": [
    { "type": "Action.Submit", "title": "Save project row" }
  ]
}
""";
    }

    public override CommandResult SubmitForm(string payload)
    {
        JsonNode? input = JsonNode.Parse(payload);
        if (input is null)
        {
            return CommandResult.GoHome();
        }

        bool saved = _store.UpsertProject(
            _id,
            input["name"]?.ToString() ?? string.Empty,
            input["category"]?.ToString() ?? string.Empty,
            input["note"]?.ToString() ?? string.Empty,
            input["repoUrl"]?.ToString() ?? string.Empty,
            input["siteUrl"]?.ToString() ?? string.Empty,
            input["extraLabel"]?.ToString() ?? string.Empty,
            input["extraUrl"]?.ToString() ?? string.Empty,
            Toggle(input["copyName"], true),
            Toggle(input["copyRepo"], true),
            Toggle(input["copySite"], true),
            Toggle(input["copyExtra"], false),
            Toggle(input["includeInCopyAll"], true));

        return saved
            ? CommandResult.GoBack()
            : CommandResult.ShowToast("Add at least one valid http/https project link");
    }

    private static bool Toggle(JsonNode? node, bool fallback) =>
        bool.TryParse(node?.ToString(), out bool value) ? value : fallback;

    private static string Bool(bool value) => value ? "true" : "false";

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
