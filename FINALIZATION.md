# The Understudy — Finalization Punch List

Status assessment for shipping The Understudy as a full, base-game-equivalent character,
including multiplayer (co-op) correctness. Generated 2026-07-21 against `main` @ v1.11.42.

**Build:** green (`dotnet build TheUnderstudy.csproj` → exit 0).
**Tests:** 65 test files, suite green in prior sessions.
**Mechanics:** feature-complete and well past the design target. The gaps below are almost
entirely **art** plus a short list of **multiplayer items to verify**.

---

## 0. TL;DR — what actually blocks "equivalent to any other character"

| Area | State | Blocking? |
|------|-------|-----------|
| Card mechanics / count | ✅ 84 non-basic (20C / 37U / 27R) — at/above base-game 82 (20/36/26) | No |
| Cards / relics / potions / powers code | ✅ complete, build+tests green | No |
| Character select, portrait, icon, energy orb art | ✅ bespoke art present | No |
| **Card art (portraits + full art)** | ❌ **only 3 type-placeholders for 84+ cards** | **Yes — #1 gap** |
| **Power icons** | ⚠️ 18 of ~46 done; **28 fall back to game's `missing_power.png`** | **Yes** |
| **Relic art** | ❌ **9 relics, no art** (fall back to default relic sprite) | **Yes** |
| **Potion art** | ❌ **8 potions, no art** (fall back to missing-potion sprite) | **Yes** |
| Map/campaign marker | ⚠️ inherited Ironclad default | Minor |
| `bestiaryQuote` loc | ❌ missing (every base char has one) | Minor |
| Multiplayer | ✅ mostly solid — 4 items to verify (§4) | Verify |

---

## 1. Missing / placeholder ASSETS (the dominant gap)

Everything mechanical works; the mod largely *looks* unfinished because most art is placeholder.

### 1.1 Card art — **highest priority**
- **84+ cards** (4 basics + 20 Common + 37 Uncommon + 27 Rare) share **3 placeholder images**:
  `TheUnderstudy/images/card_portraits/placeholders/{attack,skill,power}.png`.
- `UnderstudyCard.PortraitPath` (small icon) falls back to the type placeholder; `CustomPortraitPath`
  (big/full art) falls back to the **blank** base-game portrait. So both the collector-card icon and
  the full-art hover are placeholder for every card.
- **Needed:** one portrait + one big/full-art per card, dropped at
  `images/card_portraits/<slug>.png` and `images/card_portraits/big/<slug>.png`
  (`<slug>` = lowercased class name, per `UnderstudyCard`). No code change required — the path
  convention already resolves them once files exist.

### 1.2 Power icons — 28 missing
- `images/powers/` and `images/powers/big/` have art for 18 powers; **28 powers fall back to the
  game's `missing_power.png`.** Missing (small + big each):
  `AnotherBrick, Apathy, AutoTune, Balanced, BalancedChoice, BrightSide, CenterStage, CryingOutLoud,
  DoubleTime, EnjoyTheRide, FinalLesson, HeldNote, Intermission, MasterForm, MuscleMemory, Muse,
  MyOwnLesson, OneTake, Perfectionism, Punished, Resourceful, Reverb, Rewarded, SecondLesson,
  StagePresence, Stereo, TheFirstLesson, TunedLock`.
  (`InvertTrackerPower` / `DebuffClearNotifier` are internal/hidden — likely don't need art.)
- Same convention: drop `<slug>_power.png` in `images/powers/` and `images/powers/big/`.

### 1.3 Relic art — all 9 missing
- No `images/relics/` directory exists. All 9 relics (`FalseMask, TrueMask, CueLight,
  FoldableStage, Greasepaint, Lozenge, Rosin, SafetyNet, Score`) render with the default/fallback
  relic sprite.
- **Needed:** relic sprites + wire up the icon path in the relic base (mirror the `UnderstudyPower`
  convention or set `CustomPackedIconPath`).

### 1.4 Potion art — all 8 missing
- No `images/potions/` directory. `UnderstudyPotion`'s own comment: *"art is a follow-up (falls back
  to the game's missing-potion sprite)."* Potions: `Invert, Swap, Planned, Tuned, Milkshake, Love,
  Protein, Vigor`.

### 1.5 Map / campaign marker
- `TheUnderstudy.cs` comment: *"The map marker is left as the inherited Ironclad default until
  dedicated art is made."* A shipped character has its own marker.

**Already done (good):** character-select portrait (locked + unlocked), character icon / top-panel
face, character-select background scene, and a bespoke golden energy counter. These are complete.

---

## 2. Missing DIALOGUE / LOCALIZATION

Good news: the base game's per-character loc key set is small, and The Understudy already covers
almost all of it. Confirmed against the base `Slay the Spire 2.pck` (Ironclad/Silent key dump).

Present ✅: `title, titleObject, description, pronoun* (4), goldMonologue, eventDeathPrevention,
aromaPrinciple, cardsModifierTitle, cardsModifierDescription, banter.alive.endTurnPing,
banter.dead.endTurnPing`.

Missing / to add:
- ❌ **`bestiaryQuote`** — every base character has one; The Understudy's `characters.json` does not.
- ⚠️ **`unlockText`** — present on Silent (not Ironclad), so it's tied to the unlock/progression
  flow. Add if you implement a character unlock; otherwise optional.
- ℹ️ **Banter is at parity** — the base game itself only ships `banter.{alive,dead}.endTurnPing`,
  which the mod has. No extra combat barks are expected.
- ℹ️ **English only.** The base game ships many languages; a Workshop mod is not expected to, but note
  it if you want full parity. All keys live under `TheUnderstudy/localization/eng/`.

---

## 3. MECHANICS / CONTENT completeness

Largely complete — this is the mod's strong suit.

- **Cards:** 84 non-basic (20 / 37 / 27) — meets/exceeds the base-game 20 / 36 / 26 target enforced by
  `TheUnderstudyCardPoolTests`. Type/rarity/cost balance tracked in project memory.
- **Powers:** 46 custom powers (mechanics done; only art missing — §1.2).
- **Relics:** 9 (2 starter variants + 7 pool relics). Custom relics are a bonus over the shared pool.
- **Potions:** 8 custom potions (bonus over the shared pool).
- **Enchantments:** 2 (`PrePlanned`, `PreTuned`). **Rest-site options:** 2 (`Score`, `FoldableStage`).
- **Keywords / afflictions:** Planned, Tuned, Swap, Invert, Order, Intense/Stable, Limited/Jaded and
  the full `Un*` inversion family — all implemented with tooltip-sync tests.

Nice-to-haves for full parity (not blockers):
- Character **unlock/progression** flow + `unlockText` (if you want it gated like base characters).
- A **bestiary/act-boss** hook is not a character responsibility — nothing to do here.
- Steam **achievements** (base game has per-character ones) — optional, out of BaseLib scope.

### 3.1 Epochs / the Timeline — intentionally N/A (not a gap)

**What Epochs are:** STS2's meta-progression system. In the **Timeline** screen you spend end-of-run
**score** to "reveal Epochs," which (a) progressively **unlock that character's cards & relics**, and
(b) deliver **lore/story fragments** as a gallery. Each base character has ~6–7 Epochs
(`ironclad2-7`, `defect1-7`, `necrobinder1-7`, `regent1-3`, plus a **character-unlock Epoch**), each
with `.title/.description/.unlock/.unlockInfo/.unlockText` loc. Backed by `Core.Timeline.EpochMetric`
and `Core.Saves.EpochState`.

**Why it's not on the punch list:** BaseLib exposes **no `CustomEpochModel`** — you cannot author
Timeline Epochs for a modded character. Instead BaseLib deliberately **bypasses** the epoch gating:
its `ObtainCharUnlockEpoch` / `SkipCharUnlockEpoch` / `SkipBossEpochCheck` / `SkipEliteEpochCheck`
hooks grant the character-unlock Epoch automatically and skip the per-card/relic unlock gates. Net
effect: **The Understudy's entire pool is available immediately and the character is unlocked without
a Timeline entry** — which is the correct, expected behavior for a Workshop character. The mod does
not reference Epoch/Timeline anywhere, and shouldn't need to.

**The one real consequence:** base characters get much of their **backstory told through Epoch
gallery text**; a modded character has **no Timeline presence**, so that lore-delivery channel is
closed. The Understudy must therefore carry its story (see `LORE.md`) through the channels it *does*
control: the character `description`, the **Architect ancient dialogue** (`ancients.json`), card/relic
**flavor text**, and `unlockText` if an unlock flow is ever added. This is a **narrative** gap to be
aware of, not a mechanical one.

**Other BaseLib content types available but unused** (optional bonus scope, not required for character
equivalence): `CustomEventModel`, `CustomEncounterModel`, `CustomMonsterModel`, `CustomAncientModel`,
`CustomActModel`, `CustomPetModel`, `CustomOrbModel`. Worth knowing exist; none are character
prerequisites.

---

## 4. MULTIPLAYER (co-op) — mostly solid, 4 items to verify

The mod is in good shape here. Highlights:
- ✅ **Determinism:** 11 RNG sites, **all** via `player.RunState.Rng.<Stream>` seeded streams; **zero**
  `System.Random` / `Guid.NewGuid` / `DateTime.Now` uses. Matches the RNG-determinism rule.
- ✅ **Choice sync:** player choices route through `PlayerChoiceContext` (the game's
  `PlayerChoiceSynchronizer`), so random targets/selections replicate across clients.
- ✅ **Cross-player effects use replicated commands:** `Duet` (the one `MultiplayerOnly`,
  `TargetType.AnyPlayer` card, mirroring base-game Intercept) applies through
  `SceneStealing.ExecutePlan` → `PowerCmd.Apply(ctx, …)`, i.e. synchronized commands — not direct
  model mutation. Good.
- ✅ No hardcoded "local player" / `GetPlayer(0)` assumptions found; self-targeted cards act on the
  acting player's own deck.

### Items to verify / fix

**4.1 — [Verify] `PlayAllPlannedCard._resolvedThisTurnGlobal` clear timing in co-op.**
`Cards/PlayAllPlannedCard.cs` keeps a `static HashSet` of instances resolved this turn and calls
`.Clear()` inside `AfterPlayerTurnStartLate`. In co-op, confirm this hook fires **once per round**, not
once per player's turn-start — otherwise player A's turn-start could wipe player B's once-per-turn
guard mid-round (enabling a double-resolve or a false `Invariants.Check` trip). If the hook is
per-player, key the guard by owning player instead of a single global set.

**4.2 — [Verify] Client-local static UI state vs. teammate-triggered selections.**
`PlannedSelectionState` (`_armed` / `_gridOrder`) and `SelectionIndexBadge` are single-slot statics.
They're client-local UI (one local player per client), so cross-player collision is unlikely — but
confirm a Planned selection can't be `Arm()`-ed while a *different* selection screen (e.g. a
teammate-facing flow) opens on the same client, which would mis-gate the badge. Low risk; worth one
co-op pass.

**4.3 — [Verify] `Duet` / `BestOfBoth.ResolveFor` end-to-end in live co-op.**
The code path looks correct (replicated `PowerCmd.Apply`), but cross-player debuff→buff swaps are the
single most desync-prone feature. Play-test: Duet targeting a teammate, both clients observing, with
enemies that have Artifact and with the teammate holding `Un*` pairs.

**4.4 — [Add] Multiplayer smoke coverage.**
0 of 65 test files touch multiplayer. The bare xUnit host can't spin up co-op, so this is necessarily
manual. Add a short **co-op playtest checklist** (Duet, a Planned-selection card, a random-target
attack, a counter-relic proc — each observed on both clients for identical results) and run it before
release. Consider a `Log.Info` determinism trace behind a debug flag to diff RNG advancement between
clients.

---

## 5. Suggested order of work

1. **Card art** (84×2) — the one thing that makes it read as unfinished. Biggest lift; start now.
2. **Power icons** (28×2) and **relic art** (9) and **potion art** (8) — smaller, high visual payoff.
3. **`bestiaryQuote`** loc (+ `unlockText` if adding an unlock flow) — trivial.
4. **Map marker** art — minor.
5. **Multiplayer verification pass** (§4.1–4.4) — before publishing the "supports co-op" claim.
6. Optional parity polish: character unlock/progression, achievements, extra languages.

No code refactor is required for the art: every fallback path already resolves bespoke files by
naming convention the moment they're dropped in the right folder.
