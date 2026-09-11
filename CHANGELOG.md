# Changelog

## 1.3.0 - Public repository/name migration

- Renamed the public mod from **Move Gamepad Tooltips** to **Gamepad Tooltip Position Fix**.
- Renamed the project/assembly/DLL to `GamepadTooltipPositionFix`.
- Preserved the existing BepInEx GUID `nikich.gyk.movegamepadtooltips` for upgrade compatibility.
- Migrated the accepted runtime logic into a new public repository with fresh Git history.
- No intentional tooltip-placement, gameplay, UI-scope, or save-data behavior changes from accepted 1.2.0.

## 1.2.0 - Accepted legacy release

- Rebuilt the accepted tooltip behavior as a clean, directly buildable source project.
- Preserved lower-left bottom-edge anchoring for controller tooltips in Character/Inventory and Technology screens.
- No intentional gameplay or visual behavior changes from accepted 1.1.0.

## 1.1.0 - Accepted prototype

- Moved controller-focused tooltips in the Character/Inventory and Technology screens to the lower-left area.
- Anchored the tooltip by its bottom-left edge so variable-height descriptions grow upward and remain on-screen.
- Left mouse tooltips and other menus unchanged.
