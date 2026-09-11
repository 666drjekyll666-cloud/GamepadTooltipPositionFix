# Test Build Log

## 1.0.0 - Superseded legacy build

- **Goal:** Move controller-focused tooltips away from the selected UI element in the Character/Inventory and Technology screens.
- **Changed:** Added fixed-position handling for gamepad tooltips in those two screens.
- **Game test:** Core behavior worked, but long tooltips could extend below the bottom edge of the screen.
- **Result:** **superseded** by 1.1.0.

## 1.1.0 - Accepted legacy prototype

- **Goal:** Keep the successful repositioning from 1.0.0 while fixing long-tooltip clipping.
- **Changed:** Position is anchored from the tooltip's bottom-left edge using its current width and height, so taller tooltips grow upward.
- **Not intended to change:** Screen scope, controller-only behavior, mouse UI, gameplay, saves.
- **Game test:** User confirmed that the result worked as intended in both target screens, including long tooltips.
- **Accepted binary SHA-256:** `d7520247fd4fd78cdfd5e63119b05744cb6316baf2da75ebfe59d57f11187c76`
- **Result:** **accepted**.

## 1.2.0 - Accepted legacy release

- **Goal:** Replace the prototype/patched binary implementation with a clean, directly buildable canonical source project while preserving accepted 1.1.0 behavior.
- **Changed:** Clean BepInEx/Harmony source; cached runtime bindings remove the need to commit Graveyard Keeper assemblies; reproducible CI build added.
- **Not intended to change:** Visual position (`LeftEdge = -586`, `BottomEdge = -312`), bottom-left anchoring, controller-only behavior, affected screens, mouse behavior, gameplay, saves.
- **Verified in game:** Character/Inventory screen; Technology screen; long tooltip anchoring.
- **Legacy CI:** GitHub Actions run `33971304756`; artifact `Move-Gamepad-Tooltips-1.2.0` (`9971007902`).
- **Accepted DLL SHA-256:** `05e5d957c82b1a828584ffa9b93db67b7146410cbeda5a2898fa5efc7bf64869`
- **Accepted freeze:** `baseline/1.2.0-accepted` at `fbb2f7e9d44a70410c3396028409ffce77265e25`.
- **Game test:** User confirmed on 2026-09-05 that everything worked correctly.
- **Result:** **accepted**.

## 1.3.0 - Accepted public release

- **Goal:** Move the accepted mod into a clean public repository under the name **Gamepad Tooltip Position Fix**.
- **Changed:** Repository/project/assembly/DLL/plugin display name renamed; namespace/class normalized; BepInEx GUID deliberately unchanged.
- **Not intended to change:** Tooltip placement, target screens, controller-only behavior, mouse behavior, gameplay, saves, or runtime binding strategy.
- **Source basis:** accepted legacy 1.2.0 runtime behavior documented above.
- **Source commit:** `fefe71d3492221d879efd51d3cafaabceab8c115`.
- **Accepted freeze:** `baseline/1.3.0-accepted` at `fefe71d3492221d879efd51d3cafaabceab8c115`.
- **CI:** public GitHub Actions run `34609035779` succeeded on `windows-latest`; artifact `GamepadTooltipPositionFix-1.3.0` (`10267347004`).
- **Accepted DLL SHA-256:** `bc1915d92afdb2eb6d35995193414aba17a36784c7758dd200705f38ba5e8b8e`.
- **Game test:** User confirmed on 2026-09-11 that Character/Inventory, Technology, and long-tooltip behavior matched the accepted legacy behavior.
- **Result:** **accepted**.
