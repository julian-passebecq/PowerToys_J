using System.Text.Json;
using System.Text.Json.Nodes;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class EditPromptPage : ContentPage
{
    private readonly EditPromptForm _form;

    public EditPromptPage(LibraryStore store, PromptEntry? entry)
    {
        Title = entry is null ? "Add prompt / instruction" : "Edit prompt / instruction";
        Name = entry is null ? "Add" : "Edit";
        _form = new EditPromptForm(store, entry);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class EditPromptForm : FormContent
{
    private readonly LibraryStore _store;
    private readonly Guid _id;

    public EditPromptForm(LibraryStore store, PromptEntry? entry)
    {
        _store = store;
        _id = entry?.Id ?? Guid.Empty;
        string title = entry?.Title ?? string.Empty;
        string category = entry?.Category ?? string.Empty;
        string kind = entry?.Kind ?? "Prompt";
        string body = entry?.Body ?? string.Empty;

        TemplateJson = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
    { "type": "Input.Text", "id": "title", "label": "Title", "value": {{Encode(title)}}, "isRequired": true, "errorMessage": "Title is required" },
    { "type": "Input.Text", "id": "category", "label": "Category", "value": {{Encode(category)}}, "placeholder": "Development, Research, Writing..." },
    {
      "type": "Input.ChoiceSet",
      "id": "kind",
      "label": "Type",
      "value": {{Encode(kind)}},
      "style": "compact",
      "choices": [
        { "title": "Prompt", "value": "Prompt" },
        { "title": "Instruction / add-on", "value": "Instruction" }
      ]
    },
    { "type": "Input.Text", "id": "body", "label": "Text", "value": {{Encode(body)}}, "isMultiline": true, "isRequired": true, "errorMessage": "Text is required" }
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

        _store.UpsertPrompt(
            _id,
            input["title"]?.ToString() ?? string.Empty,
            input["category"]?.ToString() ?? string.Empty,
            input["kind"]?.ToString() ?? "Prompt",
            input["body"]?.ToString() ?? string.Empty);

        return CommandResult.GoHome();
    }

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
