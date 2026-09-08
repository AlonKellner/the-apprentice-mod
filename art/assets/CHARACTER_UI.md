# Character UI and character sprites

Everything that shows the character outside a card frame. Wired in
[`TheUnderstudyCode/Character/TheUnderstudy.cs`](../../TheUnderstudyCode/Character/TheUnderstudy.cs).

**The important context:** `PlaceholderCharacterModel` fills every unset slot with an `"ironclad"`-keyed
base-game path. The Understudy currently fights, rests, shops, walks the map and plays multiplayer
rock-paper-scissors **as the Ironclad**. Every slot below is already wired to prefer the mod's own file
and fall back to that — so shipping the file is the only remaining step.

Dimensions are TheTailor's, which are in turn the base game's.

## Face and map

| File | Size | Depicts | Status |
|---|---|---|---|
| `charui/character_icon_the_understudy.png` | 85×85 | Top-panel corner portrait and compendium icon. Reads at postage-stamp size — face and silhouette only. | shipped (placeholder) |
| `charui/character_icon_the_understudy_outline.png` | 85×85 | Solid silhouette of the above, tinted by the UI. | **missing** |
| `charui/char_select_the_understudy.png` | 132×195 | Character-select portrait card. **Currently 85×85 — wrong size, needs redrawing at 132×195.** Mask *up*: charming, cocky, performing. | wrong size |
| `charui/char_select_the_understudy_locked.png` | 132×195 | Locked silhouette. **Currently byte-identical to the unlocked file**, so locked and unlocked are indistinguishable. | **missing** |
| `charui/map_marker_the_understudy.png` | 49×64 | The token walking the map. Tiny; silhouette is everything. Gold `#f0c040` path trails from it. | **missing** |

## Backgrounds and transitions

| File | Size | Depicts | Status |
|---|---|---|---|
| `charui/char_select_bg_the_understudy.png` | 2560×1200 | Character-select backdrop. **Currently 250×190 — a copy of the Attack card placeholder.** His space: manuscript, instruments, the unfinished masterpiece. Loaded through `scenes/char_select_bg_the_understudy.tscn` as a full-rect `TextureRect`. | wrong size |
| `charui/the_understudy_transition.png` | 2560×1200 | **A grayscale falloff mask, not a picture.** The wipe shader reads its red channel and `step()`s it against a rising threshold, so black wipes first and white wipes last. Paint the *order* the screen should dissolve in — e.g. a wave sweeping across a staff. | **missing** |
| `materials/the_understudy_transition_mat.tres` | — | `ShaderMaterial` pointing at the mask above. Copy TheTailor's shader: reads `transitionTex`'s red channel, `step()`s against a `threshold` remapped to −0.1…1.1, writes `COLOR.a`. | **missing** |

## Energy counter

The orb is currently one flat 74×74 image on layer 1 with a transparent 8×8 filler on layers 2–5.
TheTailor uses three 256×256 layers with layers 2–3 counter-rotating, composed in a scene.

| File | Size | Depicts | Status |
|---|---|---|---|
| `charui/big_energy.png` | 74×74 | The current single-layer orb; also the inline energy glyph source for card text. | shipped (placeholder) |
| `charui/text_energy.png` | 24×24 | Inline energy glyph in card descriptions. | shipped (placeholder) |
| `combatui/energy_counters/the_understudy_orb_layer_1.png` | 256×256 | Static base of the orb. | **missing** |
| `combatui/energy_counters/the_understudy_orb_layer_2.png` | 256×256 | Rotating layer. | **missing** |
| `combatui/energy_counters/the_understudy_orb_layer_3.png` | 256×256 | Counter-rotating layer. | **missing** |

Number outline is `#3a2800`, burst particles `#f0c040`. Both are set in code and should be treated as
fixed. **Adopting the layered orb needs a code change** — see [../DEFERRED_WORK.md](../DEFERRED_WORK.md).

## Multiplayer hands

Four images, `422×1200`, in `charui/`. Live surface — the mod ships five multiplayer cards, so other
players see these.

| File | Depicts | Status |
|---|---|---|
| `multiplayer_hand_the_understudy_point.png` | Pointing. The default pose. | **missing** |
| `multiplayer_hand_the_understudy_rock.png` | Rock. | **missing** |
| `multiplayer_hand_the_understudy_paper.png` | Paper. | **missing** |
| `multiplayer_hand_the_understudy_scissors.png` | Scissors. | **missing** |

An arm from the elbow, in his sleeve. The baton is a natural prop for the pointing pose.

## Character sprites

Scenes, not bare images — each is a `.tscn` under `TheUnderstudy/scenes/` wrapping the art.

| Scene | Art size | Depicts | Status |
|---|---|---|---|
| `the_understudy_rest_site.tscn` | 271×500 | Seated or standing at the campfire. Seen every rest site — the quietest, most-looked-at pose in the game. This is where the mask can be down. | **missing** |
| `the_understudy_merchant.tscn` | ~580×580 | Standing in the shop. | **missing** |
| `the_understudy_card_trail.tscn` | — | VFX scene for the trail a played card leaves. Gold `#f0c040`; staff lines or ink are the obvious reads. | **missing** |
| `the_understudy_visuals.tscn` | see [RIG.md](RIG.md) | The combat body. | **missing** |
