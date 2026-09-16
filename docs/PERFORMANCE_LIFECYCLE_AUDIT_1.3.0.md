# Performance and lifecycle audit — 1.3.0

Date: 2026-09-16
Status: **retain the accepted 1.3.0 runtime implementation**

## Scope

This audit checks whether the accepted `WidgetsBubbleGUI.Update()` postfix should be replaced with a narrower event-driven hook while preserving the exact runtime contract:

- controller/gamepad only;
- Character/Inventory and Technology only;
- bottom-left edge anchoring;
- tall tooltips grow upward and remain on-screen;
- mouse input and unrelated UI remain untouched.

No production runtime code was changed by this audit.

## Evidence baseline

- Accepted runtime source remains `fefe71d3492221d879efd51d3cafaabceab8c115` (`baseline/1.3.0-accepted`).
- Current `main` contains only documentation/release-workflow changes after that accepted runtime source; the runtime source itself is unchanged.
- Accepted 1.3.0 DLL SHA-256 remains `bc1915d92afdb2eb6d35995193414aba17a36784c7758dd200705f38ba5e8b8e`.
- Existing runtime acceptance covers both target screens and long tooltips.
- Host lifecycle conclusions below are derived from read-only inspection of the exact Graveyard Keeper 1.407 `Assembly-CSharp` used by current research evidence (Assembly-CSharp 11.0.0.0, module MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`). No decompiled game source or game assemblies are stored in this repository.

## Current hot-path cost

The 1.3.0 patch is per-frame because it is a postfix on `WidgetsBubbleGUI.Update()`, but the recurring work is deliberately small:

1. read the cached `for_gamepad` binding and return immediately for mouse/keyboard use;
2. obtain the cached GUI singleton/member bindings;
3. test whether Inventory or Technology is active and return for every unrelated screen;
4. only on a target screen, read the current tooltip widget width/height and assign one `Transform.localPosition` value.

Reflection/member discovery and expression compilation happen once during plugin initialization. The frame path performs no reflection lookup, no collection traversal, no LINQ, and introduces no intentional managed allocation. `Vector3` is a value type.

This is therefore a recurring cost, but a very small and bounded one. No evidence found in this audit justifies adding state or extra lifecycle hooks merely to remove that cost.

## Native/event-driven seams audited

### `Tooltip.Show(bool for_gamepad)` / `TooltipBubbleGUI.Show(...)`

A genuine creation/show seam exists. `Tooltip.Show` passes the data and gamepad flag to `TooltipBubbleGUI.Show`; the latter creates the tooltip bubble, links the gamepad collider, then invokes `WidgetsBubbleGUI.Show(...)`.

This is narrower in frequency than `Update`, but it is not yet proven to be an equivalent final-placement seam. Bottom-left anchoring depends on the **final rendered widget width and height**, especially for long tooltips. Existing evidence does not prove that a postfix at this creation stage always runs after every NGUI table/layout/anchor update, nor that no later host update can reposition the bubble.

### `Tooltip.SetData(...)`, `Tooltip.AddData(...)`, and related content mutation

These APIs are too early for placement. They mutate the data container, not the final rendered widget geometry. Hooking them would also require following later redraw/layout paths before width and height are authoritative.

### `TooltipsManager.Redraw()` and gamepad selection callbacks

Technology gamepad selection paths call `TooltipsManager.Redraw()`. Inventory/item gamepad selection paths also trigger tooltip redraw. This proves that screen-open events alone are insufficient: the active tooltip can change repeatedly while the same screen stays open.

`TooltipsManager.Redraw()` is a possible event-driven trigger, but it is a broad shared tooltip seam and its ordering relative to final bubble layout is not proven to be a safer placement point than `WidgetsBubbleGUI.Update()`. Moving the patch there would still require target-screen filtering and could expand the semantic blast radius.

### Host use of `WidgetsBubbleGUI.Update()` after layout work

Exact-game IL evidence shows vanilla bubble code explicitly invoking `WidgetsBubbleGUI.Update()` after content-table repositioning and `OnContentChanged`. That establishes `Update()` as part of the host's own layout/position finalization lifecycle, not merely an arbitrary frame callback chosen by the mod.

This is important for the accepted behavior: the mod's postfix runs after vanilla's own update logic and therefore reapplies the desired final position using the widget's current dimensions.

## Alternatives considered

### Reposition only when the relevant screen opens

Rejected. Tooltip selection/content changes while Inventory or Technology remains open, so one screen-open placement cannot preserve behavior.

### Reposition only when a tooltip is created

Not proven equivalent. The native show seam exists, but final widget dimensions and later layout/reposition ordering are not proven at that point. This would reopen the exact tall-tooltip/edge-anchor class of regressions already solved and runtime-accepted.

### Reposition on content/data changes

Rejected as a standalone mechanism. Data mutation precedes final rendering and sizing.

### Cache last widget/width/height and skip the transform write on unchanged frames

Technically possible, but not a useful simplification. The mod would still need the recurring target/gamepad checks and geometry reads, while adding state, invalidation rules, and more lifecycle assumptions merely to avoid a single transform assignment. Under the project's least-sufficient-mechanism rule, that is a worse reliability/complexity trade.

### Combine several event hooks

A combination such as screen-open + gamepad-selection + tooltip-show/content-change could reduce recurring calls, but it would replace one narrow, well-tested postfix with several hooks and more ordering/state assumptions. No measured or observed performance problem warrants that larger surface.

## Decision

**Keep the `WidgetsBubbleGUI.Update()` postfix for 1.3.0.**

The audit found real event-driven seams, but did **not** prove a single narrower seam that is both:

1. late enough to observe authoritative final tooltip geometry; and
2. guaranteed not to be overwritten by later vanilla positioning/layout work.

The existing patch is the narrowest currently proven-equivalent mechanism. It is cheap by static inspection, has early exits outside the exact target state, and already has runtime acceptance for the behavior that is most sensitive to timing: bottom-left anchoring of tall tooltips.

No dev branch or candidate DLL is warranted from this audit.

## Separate diagnostic-cleanliness issue

1.3.0 currently performs property-first fallback probing for `GUIElements.inventory`, `GUIElements.tech_tree`, and `WidgetsBubbleGUI.widget`. In GK 1.407 these are fields, so HarmonyX logs three startup warnings before the field fallback succeeds.

This is initialization-only and is not part of the per-frame hot path. It is therefore **not a performance reason to change the runtime architecture**. It can be cleaned up independently in a future maintenance change by binding the known fields directly, with the usual runtime test/release discipline.

## Revisit criteria

Reconsider an event-driven rewrite only if one of the following is established with exact 1.407 evidence:

- a post-layout tooltip callback whose widget dimensions are demonstrably final and whose position is not rewritten afterward;
- a measured performance problem attributable to this postfix;
- or a host lifecycle change that invalidates the current `Update()` seam.

Until then, replacing the accepted implementation would trade demonstrated reliability for unproven architectural neatness.
