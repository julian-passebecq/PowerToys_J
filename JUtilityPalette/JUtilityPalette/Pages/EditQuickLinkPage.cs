using System.Text.Json;
using System.Text.Json.Nodes;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class EditQuickLinkPage : ContentPage
{
    private readonly EditQuickLinkForm _form;

    public EditQuickLinkPage(LibraryStore store, QuickLinkEntry? entry)
    {
        Title = entry is null ? "Add quick link" : "Edit quick link";
        Name = entry is null ? "Add" : "Edit";
        _form = new EditQuickLinkForm(store, entry);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class EditQuickLinkForm : FormContent
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public EditQuickLinkForm(LibraryStore store, QuickLinkEntry? entry)
    {
        _store = store;
        _id = entry?.Id ?? Guid.Empty;
        string title = entry?.Title ?? string.Empty;
        string category = entry?.Category ?? string.Empty;
        string url = entry?.Url ?? string.Empty;

        TemplateJson = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
    { "type": "Input.Text", "id": "title", "label": "Title", "value": {{Encode(title)}}, "isRequired": true, "errorMessage": "Title is required" },
    { "type": "Input.Text", "id": "category", "label": "Category", "value": {{Encode(category)}}, "placeholder": "Later, AI, Dev, Research..." },
    { "type": "Input.Text", "id": "url", "label": "URL", "value": {{Encode(url)}}, "isRequired": true, "errorMessage": "URL is required", "placeholder": "https://..." }
  ],
  "actions": [
    { "type": "Action.Submit", "title": "Save" }
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

        _store.UpsertLink(
            _id,
            input["title"]?.ToString() ?? string.Empty,
            input["category"]?.ToString() ?? string.Empty,
            input["url"]?.ToString() ?? string.Empty);

        return CommandResult.GoHome();
    }

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
