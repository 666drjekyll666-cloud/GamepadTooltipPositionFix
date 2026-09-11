# Gamepad Tooltip Position Fix

A small quality-of-life mod for **Graveyard Keeper** that moves controller-focused tooltips away from the selected item or technology and keeps long descriptions on-screen.

## What it does

- Affects gamepad/controller tooltips only.
- Affects only the Character/Inventory and Technology screens.
- Anchors the tooltip by its bottom-left edge in the lower-left part of the screen.
- Long tooltips grow upward instead of extending below the screen.
- Mouse tooltips, other menus, gameplay, and save data are unchanged.

## Requirements

- Graveyard Keeper 1.407 (tested on Steam)
- BepInEx 5.x (tested with 5.4.23.5)

## Installation

If upgrading from **Move Gamepad Tooltips**, first remove `MoveGamepadTooltips.dll` so that two assemblies with the same BepInEx plugin GUID are not installed together.

Then copy the current `GamepadTooltipPositionFix.dll` release into:

`Graveyard Keeper/BepInEx/plugins/`

## Status

The accepted gameplay baseline is legacy version **1.2.0**. Version **1.3.0** is the clean public-repository/name migration of that same behavior; the plugin GUID remains unchanged for upgrade compatibility.

The public repository intentionally starts with fresh Git history. Historical development, diagnostics, and the accepted 1.2.0 binary remain in the private legacy repository. See `docs/MIGRATION_PROVENANCE.md` for exact source provenance.

## Development

- Canonical project: `GamepadTooltipPositionFix.csproj`
- Canonical runtime source: `src/GamepadTooltipPositionFix.cs`
- Verified build/test history: `docs/TEST_BUILD_LOG.md`
- Project-specific engineering rules: `AGENTS.md`

No Graveyard Keeper game binaries or extracted game assets are stored in this public repository.
