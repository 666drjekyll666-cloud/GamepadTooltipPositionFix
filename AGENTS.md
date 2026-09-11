# Gamepad Tooltip Position Fix — Project Rules

The global engineering baseline for this repository is `666drjekyll666-cloud/DevRules`. Before substantive implementation, read `ENGINEERING_RULES.md`, `CI_POLICY.md`, `GIT_WORKFLOW.md`, and `PROJECT_BOOTSTRAP.md` there. This file contains only project-specific additions and explicit exceptions.

## Project identity and scope

- Public project: **Gamepad Tooltip Position Fix**.
- Game: `Graveyard Keeper 1.407`.
- Repository: `666drjekyll666-cloud/GamepadTooltipPositionFix`.
- Canonical project: `GamepadTooltipPositionFix.csproj`.
- Canonical runtime source: `src/GamepadTooltipPositionFix.cs`.
- Stable BepInEx GUID: `nikich.gyk.movegamepadtooltips` (legacy identity retained intentionally for upgrade compatibility).
- Scope: controller/gamepad tooltip placement only in the Character/Inventory and Technology screens.

Preserve the accepted behavior unless a new change is explicitly requested:

- gamepad/controller tooltips only;
- lower-left placement anchored by the tooltip's bottom-left edge;
- long tooltips grow upward rather than off-screen;
- mouse tooltips, unrelated menus, gameplay, and save data remain unchanged.

Do not expand this small QoL mod into a generic tooltip/UI framework without explicit user approval.

## Public/research boundary

This public repository must contain only redistributable project material: our source, documentation, build definitions, and our own release binaries/assets.

Do not commit Graveyard Keeper assemblies, extracted game assets, decompiled game source, or research archives here. Reverse-engineering material that genuinely needs retention belongs in the private `666drjekyll666-cloud/GraveyardKeeperResearch` repository; durable verified facts needed by production belong in public project documentation.

## Repository and release contract

- `main` is the stable public line.
- Runtime behavior changes use a development branch and require explicit user acceptance before promotion to `main`.
- Every numbered DLL handed to the user is immutable and tied to exact source.
- `docs/TEST_BUILD_LOG.md` is the durable test-build record.
- The legacy accepted 1.2.0 artifact remains historical evidence in the private legacy repository and must not be rebuilt or relabeled.
- Public rebranding/package changes use a new version; do not present different bytes as legacy 1.2.0.
- The user prefers a ready raw, versioned DLL, not a ZIP.

## CI policy for this repository

Follow `DevRules/CI_POLICY.md`.

- Standard GitHub-hosted CI is permitted because this is a public repository, but CI still exists to prove concrete properties rather than run ceremonially.
- A meaningful source/build change on `main` may run the clean build automatically as a stable-line integrity check.
- Manual `workflow_dispatch` remains available for candidate/handoff builds.
- Documentation-only changes must not trigger the hosted build.
- Windows remains the canonical runner until a cheaper runner is explicitly proven equivalent for this project.
- A clean Release build remains required before a new DLL is handed to the user.

## Long-lived sources of truth

Use `README.md`, `CHANGELOG.md`, `docs/MIGRATION_PROVENANCE.md`, `docs/TEST_BUILD_LOG.md`, the canonical source/project files, and current public repository history. Historical pre-public evidence remains available in `666drjekyll666-cloud/Move-Gamepad-Tooltips-legacy-private`.

When chat memory conflicts with repository evidence, investigate the conflict before changing code.
