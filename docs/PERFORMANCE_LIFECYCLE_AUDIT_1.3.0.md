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

## Post-audit native-state follow-up — 2026-09-19

A later cross-project audit reopened one narrower question under the current DevRules host-native-first rule: whether the mod can configure native `BaseBubbleGUI.offset` once during tooltip show/redraw and then let vanilla own all later positioning.

This follow-up closes that question from exact 1.407 static evidence. No production change or runtime harness is required.

### Exact native position ownership

For a gamepad tooltip the relevant lifecycle is:

```text
TooltipsManager.Update
  -> Tooltip.Show(true)
  -> TooltipBubbleGUI.Show
     -> LinkColliderForGamepad
     -> WidgetsBubbleGUI.Show(force_redraw: true)
        -> Redraw
           -> UpdateSizeAndWidgetsPositions
              -> UpdateSize
              -> Reposition
              -> widget.UpdateAnchors
           -> OnContentChanged / RecalcShifts
           -> WidgetsBubbleGUI.Update
              -> linked_collider.bounds
              -> pos = top-center of collider
              -> alternative_pos = bottom-center of collider
              -> BaseBubbleGUI.UpdateBubble
                 -> world/screen conversion
                 -> choose right/up and corner
                 -> SetGUIPosToWorldPos(... current_point.shift ...)
                 -> localPosition += offset
           -> schedule late recalculation / anchor updates
WidgetsBubbleGUI.LateUpdate
  -> optional Reposition
  -> BaseBubbleGUI.LateUpdate
  -> optional OnContentChanged / RecalcShifts

Every later frame:
WidgetsBubbleGUI.Update
  -> re-read linked_collider.bounds
  -> BaseBubbleGUI.UpdateBubble again
```

Important ownership facts:

- `WidgetsBubbleGUI.Update()` re-reads the linked gamepad collider bounds every frame.
- `BaseBubbleGUI.UpdateBubble(...)` chooses the native corner from current screen-space geometry before applying `offset`.
- `SetGUIPosToWorldPos(...)` writes the native position using the current corner point/shift.
- `offset` is applied only afterward as an additive `localPosition` translation.
- `WidgetsBubbleGUI.Redraw()` performs an immediate `Update()`, but then schedules additional late anchor/shift recalculation. A normal show/redraw postfix is therefore not guaranteed to observe the final shift state for the next frame.
- In the gamepad-collider tooltip path, later ordinary frames continue to run the native position calculation even when no new tooltip is shown.

### Native `offset` hypothesis

The actual relation is conceptually:

```text
final_position(t) = native_position(t) + offset
required_offset(t) = desired_position(t) - native_position(t)
```

The accepted desired position also depends on the current tooltip size because the mod anchors the tooltip by its bottom-left edge.

`native_position(t)` is not invariant. It depends on current collider bounds, screen/camera conversion, corner selection, and the current corner shift calculated from bubble geometry. The desired center position also changes when tooltip width/height changes.

Therefore one constant offset configured at show/redraw is not a general fixed-position state. It remains correct only while all of those dynamic inputs remain unchanged. Vanilla's own code explicitly supports those inputs changing without a new show by re-reading the collider and recomputing position every frame.

There is also an ordering problem on initial/redraw layout: `Redraw()` calls `Update()`, then late lifecycle work can recalculate bubble shifts. An offset computed in a simple `Show` or `Redraw` postfix can therefore become stale before the next normal frame.

Making offset robust would require at least one of:

- a deferred post-layout hook plus invalidation for later collider/layout/screen changes;
- multiple lifecycle hooks and custom state;
- or recurring recalculation of the required offset.

All three remove the simplicity advantage of the proposed event-driven design. Recurring offset recalculation is especially pointless here: it would still need to determine the same dynamic native result every frame, while the current postfix simply writes the already-known accepted final transform after vanilla finishes its calculation.

### Seam comparison

| Seam / mechanism | Frequency | Owns final size? | Owns position? | Later vanilla overwrite risk | Extra state / breadth | Verdict |
| --- | --- | --- | --- | --- | --- | --- |
| `TooltipBubbleGUI.Show` | per new tooltip | size is built before return | no | high: normal `Update` resumes, late shift recalculation follows | would need deferred/invalidation state | reject |
| `WidgetsBubbleGUI.Show/Redraw` | show/content redraw | yes | invokes positioning, but does not own later frames | high | would need offset lifecycle state | reject |
| `UpdateSizeAndWidgetsPositions` | layout/content changes | yes | no | high | misses collider/focus movement without layout | reject |
| native `BaseBubbleGUI.offset` configured once | event-driven candidate | n/a | additive input only | high when native base changes | invalidation/recompute required | reject |
| `BaseBubbleGUI.UpdateBubble` patch | recurring | receives current native inputs | yes | low if patched after native write | broader base-class seam used by multiple bubble families | worse than current |
| `TooltipsManager.Redraw` | focus/redraw events | indirectly | no | high | broad manager seam; same-tooltip movement is not represented | reject |
| current `WidgetsBubbleGUI.Update` postfix | recurring | current widget dimensions available | vanilla position just completed | none from the ordinary gamepad-collider position path before the next frame | one accepted hook, no custom lifecycle state | **keep** |
| hybrid lifecycle cache + recurring correction | mixed | can cache some inputs | partial | controllable | adds state/invalidation for negligible saved work | reject |

### Current hook breadth and cost

The Harmony target is the `WidgetsBubbleGUI.Update()` method body, so the postfix can execute for active instances that use that inherited method, not only the exact `TooltipBubbleGUI` class. Some other bubble types derive from `WidgetsBubbleGUI`; an exact invocation count is runtime-state dependent rather than a fixed project constant.

This does not change the performance verdict:

- the first check is the cached gamepad flag;
- reflection/member discovery occurs only during initialization;
- the recurring path contains no LINQ, string work, hierarchy search, `Find`, `GetComponent`, or intentional managed allocation;
- outside Inventory/Technology it exits after bounded cached state checks;
- on the two target screens it reads current widget width/height and performs one transform assignment.

The transform assignment is intentionally repeated because vanilla itself recomputes the position every frame. Caching the previous position would retain the recurring checks/geometry reads while adding invalidation state.

A subtype-only guard could theoretically reduce the semantic surface further, but there is no accepted bug or measured cost caused by the present surface. Adding another runtime assumption solely for architectural neatness is not justified under the least-sufficient-mechanism rule.

### Resolution, UI scale, content size, and focus

Native `UpdateBubble` consults current `Screen.width` / `Screen.height`, current collider position, current corner shifts, and camera conversion. This is additional evidence against a one-time offset: resolution/aspect/layout changes can alter the required compensation.

The accepted current implementation continues to derive its center from the live widget width/height, preserving the established bottom-left anchoring when tooltip content size changes.

This follow-up does **not** claim new runtime certification for every resolution, UI scale, locale, or aspect ratio. It establishes only that replacing the accepted direct correction with a constant native offset would add lifecycle/invalidation requirements rather than remove them.

### Post-audit decision

**Keep the current implementation.**

The later audit did identify a real host-native input (`BaseBubbleGUI.offset`), but exact lifecycle inspection proves that it is an additive translation after a dynamic native position calculation, not a persistent absolute-position mode. A fully event-driven offset solution is therefore not equivalent without extra hooks/state.

Patching `BaseBubbleGUI.UpdateBubble` would move to a broader shared base-class seam, not a narrower one. A hybrid would retain recurring work and add state. The accepted `WidgetsBubbleGUI.Update()` postfix remains the smallest proven mechanism with the lowest complexity and ordering risk.

No production source, version, branch, binary, accepted ref, or artifact changes are warranted. No runtime harness or hosted CI run is warranted because the decisive property is closed by exact static lifecycle evidence.

Canonical accepted runtime identity remains:

- version: `1.3.0`;
- source: `fefe71d3492221d879efd51d3cafaabceab8c115`;
- frozen ref: `baseline/1.3.0-accepted`;
- accepted DLL SHA-256: `bc1915d92afdb2eb6d35995193414aba17a36784c7758dd200705f38ba5e8b8e`.

Reopen this architecture question only if contradictory runtime evidence appears, the host lifecycle changes, or a genuinely narrower post-layout owner is discovered that preserves dynamic collider/content/screen behavior without extra state.

