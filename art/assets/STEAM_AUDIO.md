# Steam presentation and audio

## Branding

| File | Size | Notes | Status |
|---|---|---|---|
| `workshop/TheUnderstudyWIP/image.png` | 512×512 | The Steam Workshop thumbnail. **Currently a 250×190 copy of the Skill card placeholder.** Must be under 1 MB. The single highest-visibility image in the project — it is what a browsing player judges the mod by. | placeholder |
| `TheUnderstudy/mod_image.png` | 512×512 | The mod's own icon. Referenced by `project.godot`'s `config/icon` until that stale line was removed; **re-add the line when the file exists.** | **missing** |

## Previews

`tools/mod-uploader/template/README.md` documents an optional **`previews/`** directory beside
`image.png`: extra Workshop screenshots, each under 1 MB, keyed by filename, and omitting the directory
leaves existing previews untouched. **The mod has none.** TheTailor ships nine showcase renders
(character select, gameplay, starting deck, a mechanic close-up, card art).

Suggested set, once the art above exists:

1. Character select screen
2. A combat mid-turn — the deck's identity in one frame
3. The starting deck
4. Planned / Tuned in action — the mod's signature mechanic
5. Card art spread

These come **last** in the schedule, because they photograph everything else.

## Audio

**Read this before planning any audio work.**

Character SFX are wired, but not the way TheTailor's are. `CustomCharacterModel` exposes:

| Member | Default from `PlaceholderCharacterModel` |
|---|---|
| `CustomAttackSfx` | `event:/sfx/characters/ironclad/ironclad_attack` |
| `CustomCastSfx` | `event:/sfx/characters/ironclad/ironclad_cast` |
| `CustomDeathSfx` | `event:/sfx/characters/ironclad/ironclad_die` |
| `CharacterSelectSfx` | `event:/sfx/characters/ironclad/ironclad_select` |
| `CharacterTransitionSfx` | `event:/sfx/ui/wipe_ironclad` |

These are **FMOD event paths, not file paths.** Handing them a `res://` path to an `.ogg` will not work:
custom character audio requires authoring and shipping an **FMOD bank**, which no tooling in this repo
does today and which BaseLib does not appear to abstract.

TheTailor's `.ogg` SFX are not a counter-example — they sit on `CustomMinionModel.HurtSfx` /
`DeathSfx`, a different API for a different entity type. TheTailor's own character uses the inherited
FMOD events.

**Consequence:** a music-themed character is the most natural fit imaginable for custom audio, and it is
also the most expensive item in this document. Treat it as its own project with an FMOD spike in front
of it, not as a Phase 4 nicety. Until then the Understudy sounds like the Ironclad, which nothing in the
game contradicts.
