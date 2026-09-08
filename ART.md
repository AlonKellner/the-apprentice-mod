# The Understudy — art

The mod is mechanically complete and visually placeholder. This is the entry point for changing that:
what the art should feel like, how it is produced, and where every remaining asset is specified.

- **[art/PROCESS.md](art/PROCESS.md)** — how the art gets made: concept art first, then in dependency
  order, with quality tiers so partial delivery never looks broken.
- **[art/assets/](art/assets/)** — the per-asset briefs. One row per file to draw.
- **[art/DEFERRED_WORK.md](art/DEFERRED_WORK.md)** — code changes waiting on art that does not exist yet.

---

## Where things stand

41 images ship today, and most are the same picture more than once. `placeholders/attack.png` is also
the character-select background. `placeholders/skill.png` is also the Steam thumbnail. The character
icon is also the character-select face, also the *locked* face, also the Golden Bedroom event portrait,
also all seven Timeline epoch portraits. The 15 power icons in `powers/big/` are byte-identical to the
60×60 ones in `powers/`, so no "big" art exists at all.

`dotnet test` prints live coverage of the id-keyed categories (`ArtCoverageTests.ReportCoverage`):

| Category | Have | Need | Spec |
|---|---|---|---|
| Card portraits | 0 | 91 | 500×380 |
| Power icons (128 + 256) | 14 + 14 | 42 + 42 | redraws — the current ones are 60×60 |
| Relic icons (icon + outline + big) | 0 | 30 | 128 / 128 / 256 |
| Potion icons (image + outline) | 0 | 6 | 256 / 256 |

Not id-keyed, so counted by hand: **13** character-UI images, **3** character sprites, **7** epoch
portraits, **3** rest-site option icons, **2** VFX particles, **~29** rig parts, **~8** Steam/branding
images. **≈280 images in total.**

The benchmark is [TheTailor](https://github.com/hex3gc/TheTailor) — 355 PNGs, 79 committed Krita
sources, and a Blender 2D-cutout combat rig. Everything specified here is calibrated against what that
mod actually ships.

---

## Look and feel

The source of truth is [LORE.md](LORE.md) sections 1–5. The short version an artist can work from:

**A charismatic, tortured composer.** Magnetic and grandiose on the surface; anguished, self-doubting
and approval-starved underneath. Grandiosity and fragility should sit in the same breath — he announces
his genius *and* betrays that he needs someone else to confirm it.

**Composer, not stage actor.** LORE.md section 5 is explicit about this and it is the single most
load-bearing constraint on the art. Reach for the vocabulary of composition — manuscript, ink, staves,
baton, tempo, symphony — over the vocabulary of performance — curtain, cue, spotlight, greasepaint.
Beethoven/Mozart/Salieri, not a leading man.

**The silent second presence.** An unnamed *he* recurs throughout the writing: the master being
addressed without being named. Where art can imply an audience of one — a figure just out of frame, a
shadow, an empty chair — it should.

### Palette

Locked in code; art must match rather than reinterpret.

| Colour | Hex | Where |
|---|---|---|
| Name | `#ffffff` | `TheUnderstudy.NameColor` |
| Gold accent | `#f0c040` | energy burst, map path, compendium tile shadow |
| Deep amber | `#3a2800` | energy-counter number outline |
| Deck-entry gold | `#c9992e` | `TheUnderstudyCardPool.DeckEntryCardColor` |

Card frames are recolored by shader (`H/S/V = 0.14 / 1.25 / 1.1` over the base game's `hsv.gdshader`),
not by bespoke frame art. Portrait art has to sit inside that warm gold cast.

### One open question, for the user rather than the artist

Three relics are named from theatre — **Greasepaint**, **Foldable Stage**, **Lampshade** — which is
exactly the vocabulary LORE.md section 5 argues against. Their art can lean theatrical to match the
name, or lean musical to match the character. This needs a ruling before those three are drawn, because
it also decides how far the "composer, not actor" rule actually reaches.

---

## The naming contract

**Filenames are derived, not chosen.** Every resolver in
[`StringExtensions.cs`](TheUnderstudyCode/Extensions/StringExtensions.cs) builds a path from the
content's own id:

> **slug** = the localization id minus the `THEUNDERSTUDY-` prefix, lowercased.
>
> `THEUNDERSTUDY-BACK_OF_MY_HAND` → `back_of_my_hand.png`

Every resolver returns `null` when the file is not packed, and every caller falls through to whatever
was showing before. That is what makes art **drop-in**: putting a correctly named file in the right
folder is the entire adoption step, and a wrong name fails silently — nothing errors, the art simply
never appears. The exact filename for every asset is listed in [art/assets/](art/assets/); take them
from there rather than deriving them by hand.

`dotnet test` guards this: `ArtCoverageTests` fails if an art file stops matching a live content id,
which is what a rename does. [renaming.md](renaming.md) shows names have churned a lot here.

---

## Pipeline

```
Krita / Photoshop  →  PNG export  →  scripts/import-art.sh  →  .pck
     (.kra/.psd)      (lossless)     (.import sidecar)         (all_resources)
```

1. **Export lossless PNG with straight alpha** at the dimensions given in the asset brief.
2. **Commit the source file** under a `project/` subfolder beside the export
   (`images/powers/project/muse.kra`), the way TheTailor does.
3. **Put a `.gdignore` in every `project/` folder.** The export preset is `export_filter="all_resources"`,
   so *any* file in a folder Godot can see ships inside the `.pck` — including 3 MB Krita documents.
   `scripts/import-art.sh` fails if it finds a source file in a folder with no `.gdignore`.
4. **Run `scripts/import-art.sh`** and commit the generated `<file>.png.import` sidecar alongside the
   PNG. Without the sidecar the asset is not packed and does not ship.
   - It must be generated by **MegaDot 4.5.1**, the same Godot build the game uses. A newer editor
     writes a pack format the game rejects at mod-load — long after the build succeeded.
   - `scripts/import-art.sh --check` reports without importing.
5. **Publish** with `scripts/publish-workshop.sh`, which now refuses to upload a `.pck` older than any
   asset in it.

Import settings are uniform and already correct on every existing asset: `compress/mode=0` (lossless),
`mipmaps/generate=false`, `fix_alpha_border=true`.

---

## Where art plugs into the code

No code change is needed to adopt art in any of these slots — that work is done.

| Slot | Resolver | Fallback while missing |
|---|---|---|
| Card portraits | `UnderstudyCard.PortraitPath` / `CustomPortraitPath` / `BetaPortraitPath` | shared per-type placeholder |
| Power icons | `UnderstudyPower.CustomPackedIconPath` / `CustomBigIconPath` | base game's red *missing* glyph |
| Relic icons | `RelicOutlineOverride.Register()` → BaseLib `RelicImageOverridePatch` | base default; generic outline for pool relics |
| Potion icons | `UnderstudyPotion.CustomPackedImagePath` / `CustomPackedOutlinePath` | base default |
| Rest-site options | each option's `CustomIconPath` | a borrowed power icon |
| Character UI, combat body, rest site, merchant, map marker, MP hands, transition | `TheUnderstudy.cs` | **BaseLib's Ironclad placeholder** |

That last row is worth stating plainly: `PlaceholderCharacterModel` fills every unset character slot
with an `"ironclad"`-keyed base-game path. The Understudy currently fights, rests, shops, and plays
rock-paper-scissors **as the Ironclad**. Nothing is broken — it is borrowed.

The slots that still need code are in [art/DEFERRED_WORK.md](art/DEFERRED_WORK.md); each one is blocked
on art existing, not on a decision.
