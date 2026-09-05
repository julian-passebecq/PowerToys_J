# J Utility Palette

A deliberately small PowerToys Command Palette extension for the personal workflows that are not already solved well by PowerToys.

## What it owns

1. **J Prompts** — reusable base prompts and instruction snippets.
2. **J Recent Prompts** — a bounded history of the last 25 prompts actually copied/composed.
3. **J Quick Links** — temporary categorized links that do not deserve permanent browser bookmarks.
4. **ChatGPT / Codex launchers** — top-level commands that can also be pinned to the Command Palette Dock.

Everything else stays native:

- Clipboard and screenshot history: Windows `Win+V` or Command Palette Clipboard History.
- Crop & Lock, Peek, Image Resizer, PowerRename, Text Extractor, Environment Variables, and Performance Monitor: existing PowerToys modules / Command Palette entries.

This keeps the custom code small and isolated. Command Palette extensions run out-of-process, so a bug here does not require modifying the PowerToys runner or settings application.

## J Prompts

- Add a reusable **Prompt** or **Instruction / add-on**.
- Categorize entries.
- Search title, type, category, and body text.
- Enter on a saved prompt copies it and records it in Recent Prompts.
- `Compose + copy` lets you select saved instruction blocks and add a one-off note.
- `Copy + open ChatGPT` and `Copy + open Codex` are available from a prompt's context menu.
- Edit and delete from the context menu.

The composer constructs its Adaptive Card from a list of JSON elements, so it remains valid even when no instruction blocks exist.

## J Recent Prompts

- Keeps only the latest 25 distinct prompt texts.
- Reusing the same exact prompt moves it back to the top instead of creating duplicates.
- Search the history.
- Copy again, copy + open ChatGPT, copy + open Codex, or remove an individual history item.

## J Quick Links

- Add title, category and URL.
- Search title, category, or URL.
- Enter opens the URL in the default browser.
- Copy URL, edit, and delete from the context menu.

Starter links are included for ChatGPT, Codex, and GitHub.

## Dock

The extension exposes stable ChatGPT and Codex commands as Dock bands. Pin them from Command Palette if you want a permanent two-click AI switch without adding browser-specific automation.

## Storage

All custom data is local JSON:

`%LOCALAPPDATA%\JUtilityPalette\library.json`

No service, database, account, network sync, or background polling is used.

## Build / run

This project follows the standalone Command Palette extension template already present in this PowerToys fork and targets the current `Microsoft.CommandPalette.Extensions` 0.11 SDK family.

Requirements:

- Windows 10 19041+ / Windows 11
- Current PowerToys with Command Palette enabled
- Visual Studio with Windows application development tooling
- .NET 10 SDK

Open:

`JUtilityPalette\JUtilityPalette.sln`

Select `x64` and build/deploy the `JUtilityPalette` project.

The MSIX manifest currently uses `CN=Julian Passebecq` as the development publisher. For packaging outside local development, use a signing certificate whose subject matches the manifest publisher, or replace the publisher with your Store / signing identity.

## Explicitly not in the reliable core

- CPU/GPU monitoring implementation — Command Palette already has Performance Monitor.
- Power-plan switching — useful later, but device/OEM behavior deserves a separate small command set.
- f.lux replacement — keep display gamma / night-light logic outside a prompt manager.
- News / stocks — Quick Links or dedicated widgets are a better fit.
- Automatic ChatGPT/Codex file attachment — there is no stable public external deep-link contract to pre-attach an arbitrary local ZIP/DOC to a new chat, so UI-click automation is intentionally avoided.
