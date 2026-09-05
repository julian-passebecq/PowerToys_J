# J Utility Palette

A deliberately small PowerToys Command Palette extension for the personal workflows that are not already solved well by PowerToys.

## What it owns

1. **J Prompts** — reusable base prompts and instruction snippets.
2. **J Recent Prompts** — a bounded history of the last 25 prompts actually used.
3. **J Quick Links** — temporary categorized links that do not deserve permanent browser bookmarks.
4. **ChatGPT / Codex launchers** — top-level commands that can also be pinned to the Command Palette Dock.

Everything else stays native: Windows/PowerToys handles clipboard history, screenshots, Crop & Lock, Peek, Image Resizer, PowerRename, Text Extractor, Environment Variables, and Performance Monitor.

## J Prompts

- Add a reusable **Prompt** or **Instruction / add-on**.
- Capture the current text clipboard directly as a Prompt or Instruction.
- Pin the prompts/instructions you use most; pinned entries sort first.
- Search title, type, category, and body text.
- Enter on a saved prompt copies it and records it in Recent Prompts.
- Compose a base prompt with selected reusable instructions plus a one-off addition.
- From the composer, choose **Copy**, **ChatGPT**, or **Codex**.
- ChatGPT copies the final prompt and opens ChatGPT.
- Codex uses the documented `codex://new?prompt=...` desktop deep link so the new local Codex chat opens with the prompt prefilled. The prompt is also copied as a fallback.

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

New installs seed ChatGPT, Codex Desktop (`codex://threads/new`), and GitHub.

## Storage and recovery

All custom data remains local:

`%LOCALAPPDATA%\JUtilityPalette\library.json`

Before a successful overwrite, the previous library is copied to:

`%LOCALAPPDATA%\JUtilityPalette\library.backup.json`

If the primary JSON cannot be parsed on startup, the extension attempts to recover from that backup before falling back to seed data. There is still no database, account, sync service, or background polling.

## Dock

The extension exposes stable ChatGPT and Codex Desktop commands as Dock bands. Command Palette Dock support is native; no custom always-on-top window is created.

## Build / run

The project follows the standalone Command Palette extension model and targets the current `Microsoft.CommandPalette.Extensions` 0.11 SDK family.

Requirements:

- Windows 10 19041+ / Windows 11
- Current PowerToys with Command Palette enabled
- Visual Studio with Windows application development tooling
- .NET 10 SDK

Open `JUtilityPalette\JUtilityPalette.sln`, select `x64`, and build/deploy the `JUtilityPalette` project. A focused `build.ps1` and Windows GitHub Actions workflow are included so this small extension can be compiled without building the full PowerToys fork.

The MSIX manifest currently uses `CN=Julian Passebecq` as the development publisher. For packaging outside local development, use a signing certificate whose subject matches the manifest publisher, or replace the publisher with your Store / signing identity.

## Explicitly not in the reliable core

- CPU/GPU monitoring — Command Palette already has Performance Monitor.
- Power-plan switching — device/OEM behavior deserves a separate small command set.
- f.lux replacement — display gamma/night-light logic stays outside a prompt manager.
- News/stocks — Quick Links or dedicated widgets are a better fit.
- Automatic arbitrary file attachment to ChatGPT/Codex — avoid brittle external UI automation unless a stable supported contract exists.
