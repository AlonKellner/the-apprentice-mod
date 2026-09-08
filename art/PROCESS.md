# How the art gets made

Art is produced in **dependency order**, not asset order, and every slot moves through explicit
**quality tiers**. Those two rules exist for the same reason: 280 images drawn over a long stretch will
drift apart and leave the mod visibly half-finished unless the shared vocabulary is settled first and
partial delivery is designed for.

See [../ART.md](../ART.md) for look and feel, palette, naming and pipeline. This document is sequence.

---

## Quality tiers

Every row in [assets/](assets/) has a status, and every asset moves T0 → T1 → T2. A tier is a
*shippable state*, not a draft.

| Tier | Meaning | Engine support |
|---|---|---|
| **T0 placeholder** | What ships today: shared per-type card placeholders, borrowed power icons for rest-site options, the base game's red *missing* glyph, the Ironclad body. | the existing fallback chain |
| **T1 beta** | Correct subject, correct dimensions, unfinished rendering. Reads correctly in play; would not survive a close look. | cards have a dedicated engine slot — `card_portraits/beta/<slug>.png` |
| **T2 final** | Style-bible compliant, source file committed under `project/`. | the primary path for every slot |

**T1 is genuinely shippable, not a stopgap you apologise for.** TheTailor ships **93 of its 111** card
portraits at exactly this tier and reads as a polished mod. Treating T1 as a real destination is what
makes it possible to finish 91 cards at all — a rule that says every card must be final before any card
ships means no card ever ships.

---

## Phase 0 — Concept art and the style bible

**Blocks everything else.** A handful of characters and objects recur across cards, powers, relics,
epochs, the event, and the combat body. Settling them once is the difference between a coherent
character and 280 unrelated pictures.

### Recurring characters

#### The Architect
The base game's final boss, and the silent second presence behind most of this mod's writing. A
*dreamless creature of order* who set out to design his own restful ending ([../LORE.md](../LORE.md)
section 9). He took the boy, taught him the secret laws of order and design, and killed him for choosing
music instead. He does not see The Understudy as a person — only as a failed project.

Visually he is the mod's **order** pole: geometry, rule, architecture, control. Everything the
Understudy's music strains against.

*Recurs in:* Epochs 1, 2, 6 and 7 · the Order affliction · The First/Second/Final Lesson cards · the
unnamed *he* in most flavour text.

#### The Understudy / the boy
Two ages that must read as unmistakably the same person:

- **the boy** — the dreaming child who was stolen (Epoch 5, *Taken*); the reason any of this happened
- **the composer** — the adult who climbs the Spire to be heard by one listener

The tension in section 2 of LORE.md has to be legible in the face: charismatic genius on the surface,
approval-starved child underneath. If a viewer can see both at once, the character is right.

*Recurs in:* character select · combat · rest site · merchant · compendium · map marker · multiplayer
hands · Epochs 5 and 7 · a large share of card art.

#### The Mirror / the child
The Architect's **first, perfect successor** — Epoch 3, *A Perfect Mirror* — lost to the blight in Epoch
4, *Consumed*. The Understudy's predecessor and permanent measuring stick: the one who did not
disappoint, and who is gone.

Design brief: recognisably a sibling to the boy, and *better* in every way the Architect would have
cared about — more ordered, more finished, less alive. The name is the instruction: he should read as a
reflection of the Understudy that got something right.

*Recurs in:* Epochs 3 and 4 · thematically underwrites the whole self-comparison and inversion mechanic
family.

### Recurring objects

#### The Mask
Literal and metaphorical at once, and the character's central image.

- **Literal** — the `False Mask` → `True Mask` starter relic. The starting relic, so this is the first
  piece of relic art any player ever sees, and it needs two states that read as a transformation, not a
  swap.
- **Metaphorical** — [../LORE.md](../LORE.md) section 7: with everyone else "the mask is *up* and he
  performs, charming and cocky"; the Architect is "the one encounter where the mask comes off."

*Recurs in:* both starter relics · the Mask Drop card · the character-select and combat silhouette.

#### The Golden Book
The book of endings the Architect resolves to write — his design for his own rest.

*Recurs in:* the `Book of Endings` relic · the **Golden Bedroom** event · Epoch 2 (*Writing an End* —
"the book he resolves to write is the event where you find it") · the `Study` rest-site option ·
Fate Knocking · Every Story has a Lesson.

Needs at least a closed and an open state; the open state is doing the work in the event art.

#### The Baton / wand
His instrument of authorship — the conductor's implement, and the clearest single object for "the
master's tools of order, turned to music." The most reusable prop in the set: it can appear in a card
portrait as a hand and a baton alone, without drawing the character.

*Recurs in:* the combat rig's held prop · the Conductor card and power · Orchestration · Tuning Ritual ·
Da Capo · Forte · the PowerUp VFX.

### Phase 0 deliverables

1. **Character sheets** — turnaround and expression sheets for the Architect, the boy/composer (both
   ages), and the Mirror.
2. **Object sheets** — the Mask (False and True), the Golden Book (closed and open), the Baton, each
   with material and lighting notes.
3. **A one-page style bible** — line weight, the locked palette from [../ART.md](../ART.md), lighting
   direction, silhouette rules, and a side-by-side calibration against Megacrit's own card art. This is
   the page every later asset is checked against.

Committed under `art/concept/` as `.kra`/`.psd` **with a `.gdignore`** so concept work never ships in
the `.pck`.

---

## Phase 1 — Identity

What a player sees *before* and *around* playing, and the first real test of the style bible on live
surfaces: Steam thumbnail · `mod_image.png` · character-select icon, locked variant and background ·
map marker · compendium icon and outline.

Highest visibility per unit of work in the whole project — a browsing player forms an opinion here
before installing anything.

Specified in [assets/CHARACTER_UI.md](assets/CHARACTER_UI.md) and
[assets/STEAM_AUDIO.md](assets/STEAM_AUDIO.md).

## Phase 2 — The long tail, in impact order

1. **Power icons (42 × 2)** — kills the red *missing* glyph. The most visible in-combat defect, and 28
   of the 84 files already have something to redraw from. → [assets/POWERS.md](assets/POWERS.md)
2. **Relics (10 × 3), potions (3 × 2), rest-site options (3)** — small, bounded, high impact per asset.
   → [assets/RELICS_POTIONS.md](assets/RELICS_POTIONS.md), [assets/WORLD.md](assets/WORLD.md)
3. **Card portraits (91)** — the long tail. Ship T1 across the whole set before taking any card to T2,
   so the deck reads as finished early. → [assets/CARDS.md](assets/CARDS.md)
4. **Epochs (7) and the Golden Bedroom event (1)** — the payoff for Phase 0's character work; these are
   the only assets that *narrate*. → [assets/WORLD.md](assets/WORLD.md)

## Phase 3 — The combat character, in three tiers

The single biggest lift, and the reason [assets/RIG.md](assets/RIG.md) is one document instead of two.
Each tier is independently shippable and replaces the previous one **without touching any card, power or
relic code** — they all arrive through the same `CustomVisualPath` scene.

- **Tier 1 — static idle.** One finished illustration in the base game's `NCreatureVisuals` scene
  contract. No SubViewport, no AnimationTree, no rig. The character visibly exists in combat for a small
  fraction of the total cost. TheTailor still ships four of its own minions exactly this way.
- **Tier 2 — layered static.** The same illustration re-authored as separated layers on a full
  1000×580 canvas with baked alignment. **Identical in game to Tier 1** — the work is entirely
  preparation for the rig, so none of it is thrown away.
- **Tier 3 — full rig.** Blender 2D-cutout skeleton → `.glb` → SubViewport → five-state AnimationTree.

Stopping after Tier 1 leaves the mod in a good state. That is the point of splitting it this way.

## Phase 4 — Polish

VFX particles · rest-site and merchant sprites · the character-select transition mask · multiplayer
hands · the layered energy orb · audio (**read the constraint in
[assets/STEAM_AUDIO.md](assets/STEAM_AUDIO.md) before committing to this — it is not a matter of
dropping in an `.ogg`**).

Steam previews and showcase screenshots come **last**, because they photograph everything above.

---

## Working rules

- **Check the filename against [assets/](assets/), every time.** A wrong name fails silently — no error,
  no warning, the art simply never appears.
- **Run `scripts/import-art.sh` and commit the `.import` sidecar** with the PNG, or the asset is not
  packed and does not ship.
- **Never put a source file where Godot can see it without a `.gdignore`.**
- **`dotnet test` tracks progress.** `ArtCoverageTests.ReportCoverage` prints per-category coverage;
  `EveryShippedArtFile_BelongsToALiveContentId` fails if a rename orphans art that already exists.
- **Renames are the main hazard.** If content is renamed, rename its art in the same commit. The test
  will catch it, but only after the fact.
