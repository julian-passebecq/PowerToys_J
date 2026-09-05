# J Utility Palette

A deliberately small PowerToys Command Palette extension for the personal workflows that are not already solved well by PowerToys.

## What it owns

1. **J Prompts** — reusable base prompts and instruction snippets.
2. **J Recent Prompts** — a bounded history of the last 25 prompts actually used.
3. **J Quick Links** — temporary categorized links that do not deserve permanent browser bookmarks.
4. **ChatGPT / Codex launchers** — top-level commands and quick actions.
5. **Fast root recall** — type `j ` followed by prompt keywords directly on the Command Palette landing page.

Everything else stays native: Windows/PowerToys handles clipboard history, screenshots, Crop & Lock, Peek, Image Resizer, PowerRename, Text Extractor, Environment Variables, and Performance Monitor.

## Fast root recall

Press `Win+Alt+Space` and type `j ` plus a few keywords, for example:

```text
j debug
j preserve features
j current sources
```

Up to the three best matching saved prompts/instructions appear directly on the landing page. The fallback stays completely hidden for normal Command Palette searches, so it does not add noise to other workflows.

- Normal prompt: Enter copies it and records it in Recent Prompts.
- Template prompt: Enter opens Compose so variables can be filled first.
- Instruction: Enter copies the instruction.
- Ranking checks title first, then category/body; pinned entries get a small preference.

For the smallest possible launcher, enable **Open with a compact search box** in Command Palette settings. Then the common flow is simply `Win+Alt+Space` → `j debug` → Enter; the palette expands only when a nested page such as Compose needs more room.

## J Prompts

- Add a reusable **Prompt** or **Instruction / add-on**.
- Capture the current text clipboard directly as a Prompt or Instruction.
- Pin the prompts/instructions you use most; pinned entries sort first.
- Search title, type, category, and body text.
- Enter on a normal prompt copies it and records it in Recent Prompts.
- Enter on a template prompt opens Compose so unresolved placeholders are not copied by accident; `Copy raw template` remains available from its context menu.
- Compose a base prompt with selected reusable instructions plus a one-off addition.
- Optional template variables use `{{name}}` syntax. They create fill-in fields only when a base prompt actually contains variables, so normal prompts stay uncluttered. Empty fields leave the original placeholder unchanged.
- From the composer, choose **Copy**, **ChatGPT**, or **Codex**.
- ChatGPT copies the final prompt and opens ChatGPT.
- Codex uses the canonical `codex://new?prompt=...` desktop deep link so a new local Codex chat opens with the prompt prefilled when Windows protocol activation succeeds. The prompt is always copied first because Codex protocol activation has had Windows-specific regressions.

Example template:

```text
Audit {{project}} for {{focus}}. Preserve existing working features and return {{output format}}.
```

Variables are intentionally limited to the base prompt in this minimal implementation; reusable instruction blocks remain plain text.

## J Recent Prompts

- Keeps only the latest 25 distinct prompt texts.
- Reusing the same exact prompt moves it back to the top instead of creating duplicates.
- Search the history.
- Copy again, copy + open ChatGPT, open directly in Codex, or promote a useful variation into the permanent prompt library.

## J Quick Links

- Add title, category and URL.
- Search title, category, or URL.
- Enter opens the URL in the default handler.
- Copy URL, edit, and delete from the context menu.

New installs seed ChatGPT, Codex (`codex://threads/new`), and GitHub.

## Storage and recovery

All custom data remains local:

`%LOCALAPPDATA%\JUtilityPalette\library.json`

Before a successful overwrite, the previous library is copied to:

`%LOCALAPPDATA%\JUtilityPalette\library.backup.json`

If the primary JSON cannot be parsed on startup, the extension attempts to recover from that backup before falling back to seed data. There is still no database, account, sync service, or background polling.

## Dock

The extension exposes one optional **J Workflow** Dock band containing three buttons: **Prompts**, **ChatGPT**, and **Codex**. This uses Command Palette's native multi-button Dock support; no custom always-on-top window is created. The individual top-level commands keep stable IDs and can still be pinned separately if preferred.

## Build / deploy

The project follows the standalone Command Palette extension model and targets the current `Microsoft.CommandPalette.Extensions` 0.11 SDK family.

Requirements:

- Current PowerToys with Command Palette enabled
- Windows Developer Mode enabled for local extension deployment
- Visual Studio with Windows application development tooling
- .NET 10 SDK

The project includes the packaged launch profile and x64/ARM64 publish profiles from the official standalone Command Palette extension template.

For local use:

1. Open `JUtilityPalette\JUtilityPalette.sln` in Visual Studio.
2. Select `x64` and the packaged **JUtilityPalette (Package)** profile.
3. Use **Build > Deploy JUtilityPalette**. Building alone is not enough to refresh an installed extension package.
4. Open Command Palette and run **Reload Command Palette Extension** after redeploying changes.

Do not use the `JUtilityPalette (Unpackaged)` profile for Command Palette discovery; the extension relies on package registration through its app manifest.

A focused `build.ps1` and Windows GitHub Actions workflow are included so this small extension can be compile-checked without building the full PowerToys fork.

`verify-extension.ps1` adds a second guardrail before compilation. It verifies that the extension's C# `[Guid]`, COM class registration and Command Palette `CreateInstance` CLSID are identical, checks the `com.microsoft.commandpalette` registration and Commands interface, and confirms that packaged x64/ARM64 deployment profiles still exist. This catches the common "builds successfully but never appears in Command Palette" class of mistakes.

The MSIX manifest currently uses `CN=Julian Passebecq` as the development publisher. For packaging outside local development, use a signing certificate whose subject matches the manifest publisher, or replace the publisher with your Store / signing identity.

## Explicitly not in the reliable core

- CPU/GPU monitoring — Command Palette already has Performance Monitor.
- Power-plan switching — device/OEM behavior deserves a separate small command set.
- f.lux replacement — display gamma/night-light logic stays outside a prompt manager.
- News/stocks — Quick Links or dedicated widgets are a better fit.
- Automatic arbitrary file attachment to ChatGPT/Codex — avoid brittle external UI automation unless a stable supported contract exists.
