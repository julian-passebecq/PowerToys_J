using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JUtilityPalette.Data;
using JUtilityPalette.Models;
using JUtilityPalette.Utilities;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace JUtilityPalette.Pages;

internal sealed partial class ComposePromptPage : ContentPage
{
    private readonly ComposePromptForm _form;

    public ComposePromptPage(LibraryStore store, PromptEntry prompt)
    {
        Title = $"Compose · {prompt.Title}";
        Name = "Compose + copy";
        _form = new ComposePromptForm(store, prompt);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class ComposePromptForm : FormContent
{
    private readonly PromptEntry _prompt;
    private readonly IReadOnlyList<PromptEntry> _instructions;

    public ComposePromptForm(LibraryStore store, PromptEntry prompt)
    {
        _prompt = prompt;
        _instructions = store.Prompts.Where(x => x.Kind == "Instruction").OrderBy(x => x.Category).ThenBy(x => x.Title).ToArray();

        StringBuilder toggles = new();
        foreach (PromptEntry instruction in _instructions)
        {
            if (toggles.Length > 0)
            {
                toggles.Append(',');
            }

            toggles.Append($$"""
{ "type": "Input.Toggle", "id": "addon_{{instruction.Id:N}}", "title": {{Encode($"{instruction.Category} · {instruction.Title}")}}, "valueOn": "true", "valueOff": "false" }
""");
        }

        string separator = toggles.Length > 0 ? "," : string.Empty;
        TemplateJson = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
    { "type": "TextBlock", "text": {{Encode($"Base prompt: {prompt.Title}")}}, "weight": "Bolder", "wrap": true },
    { "type": "TextBlock", "text": "Select reusable instructions to append.", "isSubtle": true, "wrap": true }
    {{separator}}
    {{toggles}},
    { "type": "Input.Text", "id": "extra", "label": "One-off addition", "placeholder": "Anything specific for this chat...", "isMultiline": true }
  ],
  "actions": [
    { "type": "Action.Submit", "title": "Copy final prompt" }
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

        List<string> blocks = [_prompt.Body.Trim()];
        foreach (PromptEntry instruction in _instructions)
        {
            string key = $"addon_{instruction.Id:N}";
            if (string.Equals(input[key]?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
            {
                blocks.Add(instruction.Body.Trim());
            }
        }

        string extra = input["extra"]?.ToString().Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(extra))
        {
            blocks.Add(extra);
        }

        ClipboardText.Set(string.Join("\n\n", blocks.Where(x => !string.IsNullOrWhiteSpace(x))));
        return CommandResult.ShowToast("Composed prompt copied");
    }

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
