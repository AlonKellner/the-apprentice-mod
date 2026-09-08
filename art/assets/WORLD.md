# Epochs, event, rest-site options and VFX

## Timeline epochs — 7 portraits

The Understudy's seven-chapter Timeline story, *The Final Lesson*, reveals the Architect's tragedy: a
dreamless creature of order who set out to design his own ending, made a perfect successor, lost it to
the blight, and stole a dreaming boy to try again. Chapters and conditions are in
[../../LORE.md](../../LORE.md) section 9; the source of truth is
`TheUnderstudyCode/Timeline/UnderstudyEpochs.cs`.

**All seven currently show the character icon**, forced by
`TheUnderstudyCode/Patches/UnderstudyEpochPortraitPatch.cs`, whose own comment reads *"TO REMOVE WHEN
REAL EPOCH ART SHIPS"*. These are the only assets in the mod that narrate, and the payoff for the
Phase 0 character work.

| # | Chapter | Subject | Concept dependency |
|---|---|---|---|
| 1 | Dreamless | The Architect before any of it — order without rest. | The Architect |
| 2 | Writing an End | He resolves to author his own ending. | The Architect · the Golden Book |
| 3 | A Perfect Mirror | The first successor. Everything he wanted, and not alive. | The Mirror |
| 4 | Consumed | The blight takes the first child. | The Mirror |
| 5 | Taken | A dreaming boy is stolen to start again. | the boy |
| 6 | Nothing Like Him | The boy chooses music. The rupture. | The Architect · the boy |
| 7 | Every Story has a Lesson | The lesson lands. | The Architect · the composer |

Match the base game's own timeline portraits for size and framing — read them out of
`res://images/timeline/epoch_portraits/` rather than guessing. **Adopting these needs the patch deleted**
— see [../DEFERRED_WORK.md](../DEFERRED_WORK.md).

## The Golden Bedroom event — 1 portrait

`TheUnderstudyCode/Events/GoldenBedroom.cs`, currently pointed at the character icon and explicitly
marked `PLACEHOLDER`. The event is unlocked by Epoch 2, *Writing an End* — the room where the book he
resolves to write is found. **The Golden Book, open,** is the shot.

Sized to the base game's event portraits; the code reads `CustomInitialPortraitPath` and will throw
`AssetLoadException` if pointed at a file that does not exist, so this slot is unforgiving of typos.

## Rest-site option icons — 3

**128×128**, in `images/restsite/`. Already wired and drop-in: each option resolves its bespoke file
first and keeps its borrowed power icon until then.

| File | Option | Currently borrows | Subject |
|---|---|---|---|
| `notate.png` | Notate (Drafting Paper) | the Planned glyph | Enchants a card with Planned — writing the part before playing it. |
| `enact.png` | Enact (Foldable Stage) | the Tuned glyph | Enchants a card with Tuned — rehearsing it. |
| `study.png` | Study (Book of Endings) | the Planned glyph | Reads ahead to reveal a boss. The Golden Book, open, at a campfire. |

Without a `CustomIconPath` the game falls back to a shovel that reads as "Dig", which is why the
borrowed icons exist at all.

## VFX particles — 2

Feed `GPUParticles2D` nodes in the combat visuals scene, triggered on `TriggerAnim("PowerUp")`.

| File | Size | Subject |
|---|---|---|
| `vfx/the_understudy_glow_particle.png` | ~200×200 | Soft gold `#f0c040` mote. Generic power-up glow. |
| `vfx/the_understudy_staff_particle.png` | ~600×200 | An elongated streak — staff line, ink stroke, or a struck string. TheTailor's equivalent is a thread. |

Attack hits reuse the base game's VFX strings (`vfx/vfx_attack_slash` and friends); no custom hit VFX is
needed. **Adopting these depends on the rig** — see [../DEFERRED_WORK.md](../DEFERRED_WORK.md).
