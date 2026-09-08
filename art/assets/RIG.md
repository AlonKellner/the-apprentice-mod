# The combat character

The biggest single lift in the project, split into three independently shippable tiers. All three arrive
through the **same** slot — `TheUnderstudy.CustomVisualPath` → `TheUnderstudy/scenes/the_understudy_visuals.tscn`
— so a later tier replaces an earlier one without touching any card, power or relic code.

Until any of them ships, the Understudy fights **as the Ironclad**: `PlaceholderCharacterModel` supplies
the base game's Ironclad creature-visuals scene for this slot.

Every structural requirement below is read off TheTailor's working implementation.

---

## The scene contract (all tiers)

Whatever is inside, the scene must satisfy the base game's creature contract or combat breaks:

- Root **`Node2D`** with a script subclassing **`NCreatureVisuals`**. TheTailor's subclass is *empty* —
  it exists purely so the scene types correctly.
- Unique-name markers, used by the engine to place health bars, intents and hit VFX:
  - `%Visuals` — the art container
  - `%FormVfx` — form-change VFX anchor
  - `%Bounds` — hit area (TheTailor: −121 / −278 / +121)
  - `%CenterPos` — `Marker2D`, the body's centre (TheTailor: `(0, −162)`)
  - `%IntentPos` — where the intent bubble sits (TheTailor: `(30, −302)`)

**Tier 1 needs these too.** Getting them right in the static version is exactly what makes the rig a
drop-in replacement later.

---

## Tier 1 — static idle

One finished illustration, parented under `%Visuals` as a `Sprite2D`. No SubViewport, no AnimationTree,
no rig. The character visibly exists in combat for a small fraction of the total cost, and TheTailor
still ships four of its own minions this way.

Deliverable: **one image**, roughly 1000×580 to match the eventual rig canvas, plus the scene above.

Pose brief: the composer mid-performance, mask up. Baton in hand. Grandiose posture, with something in
the face that undercuts it — see [../PROCESS.md](../PROCESS.md), Phase 0.

## Tier 2 — layered static

The same illustration re-authored as **separated layers, each exported on the full 1000×580 canvas with
alignment baked in** — i.e. every part file is the same size and already in the right place, so stacking
them reproduces the illustration exactly.

**Visually identical to Tier 1 in game.** This tier ships nothing new to the player; it exists so the
rig has its input, and it means none of Tier 1's work is thrown away.

Parts, following TheTailor's breakdown:

```
head, torso, pelvis,
upperArmL, upperArmR, forearmL, forearmR, handL, handR,
thighL, thighR, calfL, calfR, shoeL, shoeR,
shadow
```

Plus the composer-specific flourishes settled in Phase 0 — coat tails, **the baton**, and trailing
elements where TheTailor uses strings (staff lines and ink are the obvious reads here). Expect ~29 parts
in total, including alternate forearm poses for the attack swing.

## Tier 3 — full rig

```
Krita layers (Tier 2 output, 1000×580 each, alignment baked in)
  → Blender 2D-cutout rig  (.blend — 28–38 bones; unused bones scaled to Vector3(0,0,0))
  → .glb export
  → Godot: the_understudy_anim_imported.tscn → the_understudy_anim_view.tscn
  → the_understudy_visuals.tscn
```

### Godot structure

- `%Visuals` → `CharBox` → `SubViewportContainer` (`stretch = true`) → **`SubViewport`**
  - size **1000×440**
  - `own_world_3d = true`
  - `transparent_bg = true`
  - `scaling_3d_scale = 2.0`
  - `render_target_update_mode = 4` (Always)
- Inside the SubViewport: the imported `.glb` scene plus an **orthographic `Camera3D`**
  (`projection = 1`, `size ≈ 3.0`).

### AnimationTree

`AnimationTree` whose `tree_root` is an `AnimationNodeStateMachine` with **exactly five states**:

| State | Triggered by |
|---|---|
| `idle` | default; everything returns here |
| `attack` | attack cards |
| `cast` | `CreatureCmd.TriggerAnim(creature, "Cast", delay)` — skills and powers |
| `hurt` | taking damage |
| `die` | death |

- `allow_transition_to_self = true`
- Returns to `idle`: `switch_mode = 2` (AtEnd), `advance_mode = 2` (Auto), `xfade_time` 0.2–0.3
- TheTailor hand-authors 17 transitions across these five states.

Animations are driven by the base game's `CreatureCmd.TriggerAnim`. The mod already calls it — a
Harmony postfix on that method is how TheTailor fires its particle VFX on `"PowerUp"`, which is the hook
the two particles in [WORLD.md](WORLD.md) plug into.

---

## Adoption

Tier 1 is enough to unblock the slot. `TheUnderstudy.CustomVisualPath` already resolves
`the_understudy_visuals.tscn` and falls back to the Ironclad, so **no code change is needed for any
tier** — the scene appearing at that path is the entire adoption step.
