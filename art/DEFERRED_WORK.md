# Deferred work

Code changes that cannot land until the art exists, because each one would replace something that
currently works with something broken. Everything that *could* be wired non-disruptively already has
been — see [../ART.md](../ART.md), "Where art plugs into the code".

Each entry names the trigger, the exact change, and the file.

---

## 1. Delete the epoch portrait patch

**Trigger:** all 7 epoch portraits exist ([assets/WORLD.md](assets/WORLD.md)).

`TheUnderstudyCode/Patches/UnderstudyEpochPortraitPatch.cs` Harmony-patches `EpochModel.Portrait` and
`EpochModel.RealPortrait` to force every `THEUNDERSTUDY*` epoch to the character icon. Its own header
says *"TO REMOVE WHEN REAL EPOCH ART SHIPS"*.

**Change:** either delete the file (if the epochs can resolve their own art through the base game's
`res://images/timeline/epoch_portraits/` tree) or repoint `PlaceholderPath` at the mod's own portraits
and keep the patch as the delivery mechanism. Decide by checking whether a mod-local path loads through
that getter — the patch already owns it, so the second option is known to work and is the safe default.

**Why deferred:** removing the patch with no art strands all seven epochs.

## 2. Retire the shared outline stopgaps

**Trigger:** per-relic outlines (`images/relics/outline/<slug>.png`) and per-potion outlines
(`images/potions/outline/<slug>.png`) exist.

- `TheUnderstudyCode/Patches/RelicOutlineOverride.cs` — the `GenericOutline` fallback in `Add(...)` can
  drop once every reward-pool relic has its own outline. The per-relic resolution around it stays; only
  the `outlineFallback` argument goes.
- `TheUnderstudyCode/Patches/PotionOutlineOverride.cs` — the whole Harmony patch can go once every
  potion has a real outline, since `UnderstudyPotion.CustomPackedOutlinePath` then supplies one.
- `TheUnderstudy/images/generic_outline.png` can be deleted when both are gone.

**Why deferred:** these are the only outlines that currently exist; removing them un-tints the
compendium and Potion Lab shadows.

## 3. The layered energy orb

**Trigger:** the three 256×256 orb layers exist ([assets/CHARACTER_UI.md](assets/CHARACTER_UI.md)).

`TheUnderstudy.cs` currently uses BaseLib's **legacy** `CustomEnergyCounter` API — one flat 74×74 image
on layer 1, transparent filler on 2–5. The modern path is `CustomEnergyCounterPath`, pointing at a scene
that composes the layers and counter-rotates 2 and 3 (TheTailor's
`tailor_energy_counter.tscn` is the reference: a 128×128 `Control`, static Layer 1, Layers 2–3 under a
`RotationLayers` node, and a Kreon-Bold label at size 36 with a 16px outline).

**Change:** add `the_understudy_energy_counter.tscn`, override `CustomEnergyCounterPath` to resolve it
existence-checked, and remove the legacy `CustomEnergyCounter` override once it does. Keep the
`#3a2800` outline and `#f0c040` burst colours.

**Why deferred:** `CustomEnergyCounterPath` takes precedence over the legacy API, so pointing it at a
scene that does not exist replaces a working orb with a broken one. (BaseLib wraps energy-counter
construction in a try/catch that falls back to the Ironclad orb, so the failure would be silent rather
than fatal — which is worse, not better.)

## 4. Restore `config/icon`

**Trigger:** `TheUnderstudy/mod_image.png` exists ([assets/STEAM_AUDIO.md](assets/STEAM_AUDIO.md)).

`project.godot` had `config/icon="res://TheUnderstudy/mod_image.png"` pointing at a file that has never
existed; the line was removed rather than left dangling. Re-add it when the 512×512 icon ships.

## 5. Steam previews

**Trigger:** enough art exists to photograph.

Create `workshop/TheUnderstudyWIP/previews/` and drop the screenshots in — the uploader picks the
directory up automatically, keyed by filename, with a 1 MB per-file limit. No code change.

## 6. Combat VFX particles

**Trigger:** the combat rig reaches Tier 3 ([assets/RIG.md](assets/RIG.md)) **and** the two particle
textures exist ([assets/WORLD.md](assets/WORLD.md)).

**Change:** add `GPUParticles2D` nodes to the visuals scene and a Harmony postfix on
`CreatureCmd.TriggerAnim` that sets `Emitting = true` when the anim name is `"PowerUp"` — walking the
`NCreatureVisuals` tree by node name, as TheTailor's `PowerupAnimPatch` does.

**Why deferred:** there is no visuals scene of the mod's own to attach particles to until the rig lands.

## 7. Custom character audio

**Trigger:** an FMOD bank pipeline exists. **This is a project, not a task.**

`CustomAttackSfx` / `CustomCastSfx` / `CustomDeathSfx` / `CharacterSelectSfx` /
`CharacterTransitionSfx` take **FMOD event paths** (`event:/sfx/characters/ironclad/...`), not
`res://` file paths. Custom audio therefore needs an authored FMOD bank shipped with the mod, which no
tooling in this repo does and which BaseLib does not appear to abstract.

TheTailor is not a counter-example: its `.ogg` files are on `CustomMinionModel.HurtSfx`/`DeathSfx`, a
different API for a different entity type, and its character uses the inherited FMOD events.

See [assets/STEAM_AUDIO.md](assets/STEAM_AUDIO.md). Until this is solved the Understudy sounds like the
Ironclad.

---

## Not deferred — already wired

For the avoidance of doubt, these were expected to need code and do not. Each resolves the mod's file
when present and falls back to exactly what shipped before:

- Card portraits, including the T1 beta tier (`UnderstudyCard`)
- Power icons (`UnderstudyPower` — unchanged; `BalancedPower`'s misnamed override was removed)
- Relic icon, outline and big, all 10 relics (`RelicOutlineOverride`)
- Potion image and outline (`UnderstudyPotion`)
- All 3 rest-site option icons
- Character icon outline, map marker, combat visuals, card trail, rest site, merchant, character-select
  transition, and all 4 multiplayer hands (`TheUnderstudy.cs`)
