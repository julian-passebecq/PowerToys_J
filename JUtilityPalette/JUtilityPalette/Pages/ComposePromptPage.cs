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
        Name = "Compose";
        _form = new ComposePromptForm(store, prompt);
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class ComposePromptForm : FormContent
{
    private readonly LibraryStore _store;
    private readonly PromptEntry _prompt;
    private readonly IReadOnlyList<PromptEntry> _instructions;
    private readonly IReadOnlyList<string> _variables;

    public ComposePromptForm(LibraryStore store, PromptEntry prompt)
    {
        _store = store;
        _prompt = prompt;
        _variables = PromptTemplate.GetVariables(prompt.Body);
        _instructions = store.Prompts
            .Where(x => x.Kind == "Instruction")
            .OrderByDescending(x => x.IsPinned)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.Title)
            .ToArray();

        List<string> body =
        [
            $$"""{ "type": "TextBlock", "text": {{Encode($"Base prompt: {prompt.Title}")}}, "weight": "Bolder", "wrap": true }""",
            """{ "type": "TextBlock", "text": "Fill any template variables, select reusable instructions, and add anything specific to this run.", "isSubtle": true, "wrap": true }""",
        ];

        for (int i = 0; i < _variables.Count; i++)
        {
            string variable = _variables[i];
            body.Add($$"""{ "type": "Input.Text", "id": "variable_{{i}}", "label": {{Encode(variable)}}, "placeholder": {{Encode($"Value for {{{{{variable}}}}}")}} }""");
        }

        foreach (PromptEntry instruction in _instructions)
        {
            string title = instruction.IsPinned ? $"★ {instruction.Category} · {instruction.Title}" : $"{instruction.Category} · {instruction.Title}";
            body.Add($$"""{ "type": "Input.Toggle", "id": "addon_{{instruction.Id:N}}", "title": {{Encode(title)}}, "valueOn": "true", "valueOff": "false" }""");
        }

        body.Add("""{ "type": "Input.Text", "id": "extra", "label": "One-off addition", "placeholder": "Anything specific for this chat...", "isMultiline": true }""");

        TemplateJson = $$"""
{
  "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
  "type": "AdaptiveCard",
  "version": "1.5",
  "body": [
    {{string.Join(",\n    ", body)}}
  ],
  "actions": [
    { "type": "Action.Submit", "title": "Copy", "data": { "target": "copy" } },
    { "type": "Action.Submit", "title": "ChatGPT", "data": { "target": "chatgpt" } },
    { "type": "Action.Submit", "title": "Codex", "data": { "target": "codex" } }
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

        Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < _variables.Count; i++)
        {
            string value = input[$"variable_{i}"]?.ToString() ?? string.Empty;
            values[_variables[i]] = value;
        }

        string basePrompt = PromptTemplate.Fill(_prompt.Body.Trim(), values);
        List<string> blocks = [basePrompt];
        int addOnCount = 0;
        foreach (PromptEntry instruction in _instructions)
        {
            string key = $"addon_{instruction.Id:N}";
            if (string.Equals(input[key]?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
            {
                blocks.Add(instruction.Body.Trim());
                addOnCount++;
            }
        }

        string extra = input["extra"]?.ToString().Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(extra))
        {
            blocks.Add(extra);
            addOnCount++;
        }

        string finalPrompt = string.Join("\n\n", blocks.Where(x => !string.IsNullOrWhiteSpace(x)));
        ClipboardText.Set(finalPrompt);
        int filledVariableCount = values.Count(x => !string.IsNullOrWhiteSpace(x.Value));
        int variationCount = addOnCount + filledVariableCount;
        string historyTitle = variationCount == 0 ? _prompt.Title : $"{_prompt.Title} · +{variationCount}";
        _store.AddRecentPrompt(historyTitle, finalPrompt, _prompt.Id);

        string target = input["target"]?.ToString() ?? "copy";
        if (string.Equals(target, "codex", StringComparison.OrdinalIgnoreCase))
        {
            return AppLauncher.TryOpenCodex(finalPrompt)
                ? CommandResult.Dismiss()
                : CommandResult.ShowToast("Prompt copied, but Codex could not be opened");
        }

        if (string.Equals(target, "chatgpt", StringComparison.OrdinalIgnoreCase))
        {
            return AppLauncher.TryOpenChatGpt()
                ? CommandResult.Dismiss()
                : CommandResult.ShowToast("Prompt copied, but ChatGPT could not be opened");
        }

        return CommandResult.ShowToast("Composed prompt copied");
    }

    private static string Encode(string value) => JsonSerializer.Serialize(value, LibraryJsonContext.Default.String);
}
