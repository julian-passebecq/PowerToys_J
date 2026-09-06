# J Utility Palette

A deliberately small PowerToys Command Palette extension for the personal workflows that are not already solved well by PowerToys.

## What it owns

1. **J Prompts** — reusable base prompts and instruction snippets.
2. **J Recent Prompts** — a bounded history of the last 25 prompts actually used.
3. **J Quick Links** — temporary categorized links that do not deserve permanent browser bookmarks.
4. **J System** — three thin shortcuts to Hosts File Editor, Environment Variables, and Task Manager.
5. **ChatGPT / Codex launchers** — top-level commands and quick actions.
6. **Fast root recall** — retrieve and route prompts or system tools directly from the Command Palette landing page.

Everything else stays native: Windows/PowerToys still owns clipboard history, screenshots, Crop & Lock, Peek, Image Resizer, PowerRename, Text Extractor, Hosts editing, Environment Variables, Task Manager, and Performance Monitor. J Utility only routes to them where useful.

## Fast root recall

Press `Win+Alt+Space` and use one of four tiny prefixes:

```text
j                       # show top pinned/recent prompts + instructions
j debug                 # find + copy
jg                      # show top pinned/recent prompts for ChatGPT
jg debug                # find + copy + open ChatGPT
jc                      # show top pinned/recent prompts for Codex
jc debug                # find + open Codex with prompt prefilled
js                      # show the three system shortcuts
js env                  # open a tiny system shortcut
```

A prefix works both by itself and with a search. This avoids depending on whether Command Palette preserves a trailing space in the query. Prefix matching requires a token boundary, so an ordinary query such as `javascript` does not activate `j`.

You can use multiple keywords, for example `j preserve features` or `jc current sources`. Up to the three best matching entries appear directly on the landing page. These fallback results stay completely hidden for ordinary Command Palette searches, so they do not add noise to files, apps, settings, calculator, or other extensions.

- `j` searches Prompts and Instructions. Prompts copy; Instructions copy.
- `jg` and `jc` search full Prompts only, because reusable Instructions are normally add-ons rather than standalone requests.
- With no search text, prompt prefixes return pinned entries first, then the most recently updated entries.
- Template prompt: all three prompt prefixes open Compose first so unresolved `{{variables}}` cannot be routed accidentally.
- `js` searches only the three J System shortcuts.
- Ranking checks title first, then category/body/aliases; pinned prompt entries get a preference.

For the smallest possible launcher, enable **Open with a compact search box** in Command Palette settings. Then the common flow is `Win+Alt+Space` → `jg debug` → Enter; the palette expands only when a nested page such as Compose needs more room.

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
- Codex uses `codex://threads/new?prompt=...` for a new local thread with the prompt prefilled when Windows protocol activation succeeds. The prompt is always copied first because desktop protocol activation can still have cold-start regressions.
- Clipboard write failures are converted to a visible Command Palette error instead of allowing a command exception to escape.

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

## J System

J System deliberately does not reimplement Windows or PowerToys utilities. It exposes only three small shortcuts:

```text
js host                # PowerToys Hosts File Editor
js env                 # PowerToys Environment Variables
js task                # Windows Task Manager
js gestionnaire        # same Task Manager shortcut, French alias
```

- **Hosts File Editor** signals the same elevated activation event used by the PowerToys runner. The PowerToys Hosts utility must be enabled.
- **Environment Variables** does the same for PowerToys Environment Variables, preserving its profiles and User/System-variable behavior. The utility must be enabled.
- **Task Manager** launches Windows `taskmgr.exe`. `Ctrl+Shift+Esc` remains the native fastest shortcut when you only need Task Manager.

The PowerToys event names are checked against this fork's `src/common/interop/shared_constants.h` in CI so a future PowerToys rebase cannot silently break these bridges.

## Storage and recovery

All custom data remains local:

`%LOCALAPPDATA%\JUtilityPalette\library.json`

Before a successful overwrite, the previous library is copied to:

`%LOCALAPPDATA%\JUtilityPalette\library.backup.json`

If the primary JSON cannot be parsed on startup, the extension attempts to recover from that backup before falling back to seed data. There is still no database, account, sync service, or background polling.

## Dock status

J Utility does **not currently expose a Dock band**. The keyboard-first top-level and fallback commands remain the reliable core.

As of September 2026, PowerToys issue [#50367](https://github.com/microsoft/PowerToys/issues/50367) documents an open Command Palette bug where Dock buttons from out-of-process COM extensions can become stale after the host releases an idle extension. J Utility uses that same extension architecture, so the previous experimental Dock was removed rather than keeping a known flaky path. It can be reconsidered after the upstream lifecycle issue is fixed.

## Build / deploy

The project follows the standalone Command Palette extension model and targets `Microsoft.CommandPalette.Extensions` **0.12.260812002**.

Requirements:

- Current PowerToys with Command Palette enabled
- Windows Developer Mode enabled for local extension deployment
- Visual Studio with Windows application development tooling
- .NET 10 SDK

The project includes the packaged launch profile and x64/ARM64 publish profiles from the official standalone Command Palette extension template. `JUtilityPalette.csproj` binds `PublishProfile` to `win-$(Platform).pubxml` so Visual Studio selects the correct profile for the active architecture.

For local use:

1. Open `JUtilityPalette\JUtilityPalette.sln` in Visual Studio.
2. Select `x64` and the packaged **JUtilityPalette (Package)** profile.
3. Use **Build > Deploy JUtilityPalette**. Building alone is not enough to refresh an installed extension package.
4. Open Command Palette and run **Reload Command Palette Extension** after redeploying changes.

Do not use the `JUtilityPalette (Unpackaged)` profile for Command Palette discovery; the extension relies on package registration through its app manifest.

## Automated checks

The focused Windows CI intentionally does more than compile:

1. `verify-extension.ps1` checks package/COM/CmdPal registration, architecture publish profiles, and PowerToys bridge-event synchronization.
2. `build.ps1` builds the x64 Release extension against the current SDK.
3. `JUtilityPalette.Tests` runs a zero-framework smoke executable against the real production source files.

The smoke tests currently cover:

- template variable extraction/filling and unresolved-placeholder behavior
- prompt ranking and prompt-only ChatGPT/Codex filtering
- prefix-only pinned/recent prompt recall
- fallback-prefix parsing and token-boundary behavior
- Recent Prompts cap and exact-text deduplication
- JSON backup recovery
- migration of the old Codex web link to the desktop protocol
- canonical Codex new-thread deep-link generation and oversized-prompt fallback
- named-event signaling behavior
- English/French J System shortcut matching

The MSIX manifest currently uses `CN=Julian Passebecq` as the development publisher. For packaging outside local development, use a signing certificate whose subject matches the manifest publisher, or replace the publisher with your Store / signing identity.

## Explicitly not in the reliable core

- Dock band — temporarily excluded while the upstream out-of-process extension lifecycle bug is open.
- CPU/GPU monitoring — Command Palette already has Performance Monitor.
- Power-plan switching — device/OEM behavior deserves a separate small command set.
- f.lux replacement — display gamma/night-light logic stays outside a prompt manager.
- News/stocks — Quick Links or dedicated widgets are a better fit.
- Automatic arbitrary file attachment to ChatGPT/Codex — avoid brittle external UI automation unless a stable supported contract exists.
