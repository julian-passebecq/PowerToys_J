# Variant: Full utility + Project Clipboard

This branch keeps the complete J Utility Palette surface and adds the native project-pair clipboard.

Project row behavior:
- One row groups repository, deployed site, and an optional third web link.
- Selecting the row opens the deployed site first (repository if there is no site).
- The details pane keeps each URL clickable.
- Visible details commands copy the configured row, repository, site, or third link.
- Copy all respects both row-level inclusion and per-field copy switches.
- Edit controls whether name/repo/site/extra are included in clipboard output.

Use this branch if the Project Clipboard should live alongside J Prompts, Recent Prompts, Quick Links, J System, ChatGPT, and Codex.
