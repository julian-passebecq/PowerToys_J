# J Utility Palette

A deliberately small PowerToys Command Palette extension for two workflows that are not already solved well by PowerToys:

1. **Prompt Library + Composer** — save reusable base prompts and reusable instruction snippets, then compose a final prompt with selected add-ons plus a one-off note and copy it to the clipboard.
2. **Quick Links** — temporary, categorized links for things you want to revisit without polluting browser bookmarks.

Everything else stays native:

- Clipboard and screenshot history: use Windows `Win+V` or Command Palette's built-in Clipboard History.
- Crop & Lock, Peek, Image Resizer, PowerRename, Text Extractor, Environment Variables, and Performance Monitor: use the existing PowerToys modules / Command Palette entries.

This keeps the custom code small and isolated. Command Palette extensions run out-of-process, so a bug here does not require modifying the PowerToys runner or settings application.

## Current scope

### J Prompts

- Add a reusable **Prompt** or **Instruction / add-on**.
- Categorize entries.
- Enter on a prompt copies it immediately.
- `Compose + copy` lets you select any saved instructions and add a one-off note before copying the final prompt.
- Edit and delete from the context menu.
- Local JSON storage under `%LOCALAPPDATA%\JUtilityPalette\library.json`.

Starter entries demonstrate the pattern:

- `Debug + improve`
- `Preserve existing features`
- `Use current sources`

### J Quick Links

- Add title, category and URL.
- Enter opens the URL in the default browser.
- Copy URL, edit, and delete from the context menu.
- Starter links for ChatGPT, Codex, and GitHub.

## Build / run

This project follows the current standalone Command Palette extension template already present in this PowerToys fork.

Requirements:

- Windows 10 19041+ / Windows 11
- Current PowerToys with Command Palette enabled
- Visual Studio with Windows application development tooling
- .NET 10 SDK

Open:

`JUtilityPalette\JUtilityPalette.sln`

Select `x64` and build/deploy the `JUtilityPalette` project.

The MSIX manifest currently uses `CN=Julian Passebecq` as the development publisher. For packaging outside local development, use a signing certificate whose subject matches the manifest publisher, or replace the publisher with your Store / signing identity.

## Why this is separate from the PowerToys core

The root repository is a full PowerToys fork. Building a custom runner module would couple a tiny personal workflow to the entire PowerToys build, installer, settings IPC, and module lifecycle. A Command Palette extension is a much smaller failure surface and is the supported extension model.

## Explicitly not in v1

- CPU/GPU monitoring implementation — Command Palette already has Performance Monitor.
- Power-plan switching — useful later, but `powercfg`/power overlays have device-specific behavior and deserve a separate tiny command set.
- f.lux replacement — use Windows Night Light or a dedicated display utility; do not mix display gamma code into a prompt manager.
- News / stocks — browser bookmarks or dedicated widgets are better.
- Automatic ChatGPT/Codex file attachment — there is no stable public external API/deep link to pre-attach an arbitrary local ZIP/DOC to a new chat. Keep this out of the reliable core.
