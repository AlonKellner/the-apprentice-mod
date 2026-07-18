# Card Redesign — completed

Next-iteration deck for The Understudy: 4 Basic + 83 non-Basic = 87 cards.

**Notation.** `-a-+b+` = base value `a`, upgraded to `b`. `+text+` = added on upgrade.
`-text-` = removed on upgrade. Plain numbers are fixed. `[gold]…[/gold]` = keyword highlight.
`# suggest:` = proposed name for a still-new card (slot left `""` for you to finalize).
`# note:` = rework/standardization flag. Cost `X` = spend-all-energy card.

Targets hit: rarity **20 Common / 37 Uncommon / 26 Rare**; type **28 Attack / 35 Skill / 20 Power**;
attack targeting **4 AoE / 1 random / rest single** (base-game ratio).

---

## Basics

- "Strike" — Attack · Basic · cost 1: Deal X damage.  ⤷ upg: +3 dmg
- "Defend" — Skill · Basic · cost 1: Gain X [gold]Block[/gold].  ⤷ upg: +block
- "Practice" — Attack · Basic · cost 0: [gold]Stable[/gold]. [gold]Tuned[/gold] 1. Deal 1 damage. Apply [gold]Tuned[/gold] to -2-+3+ attacks and skills in hand.  ⤷ note: was a Skill; now an Attack that deals 1 + self-Tunes
- "Workshop" — Skill · Basic · cost 0: [gold]Stable[/gold]. Play all [gold]Planned[/gold] cards. Apply [gold]Planned[/gold] to -2-+3+ attacks and skills in your discard pile.

## Play Planned (Performing)

- "Showtime" — Skill · Uncommon · cost 1: +[gold]Retain[/gold].+ Play all [gold]Planned[/gold] cards, at most once per turn.
- "Remix" — Skill · Uncommon · cost 0: [gold]Exhaust[/gold]. Play all [gold]Planned[/gold] cards in a random order, at most once per turn.  ⤷ upg: -[gold]Exhaust[/gold]-  ⤷ note: keep existing Remix (Exhaust removed on upgrade), not +Retain
- "Da Capo" — Skill · Rare · cost -2-+1+: Play all [gold]Planned[/gold] cards, at most once per turn. Apply [gold]Planned[/gold] to each card played this way.
- "Intermission" — Power · Uncommon · cost -2-+1+: When you end your turn without playing any cards, play all [gold]Planned[/gold] cards.  ⤷ note: rename of "Venue" (Performing theme)

## Attacks

- "" — Attack · Common · cost 1: Deal 3 damage twice. Remove [gold]Unplayable[/gold] from all attacks and skills in hand.  ⤷ upg: dmg 3→4  ⤷ # suggest: Comeback
- "Melody" — Attack · Common · cost 1: Deal 9 damage. Apply [gold]Planned[/gold] to -1-+2+ attacks and skills in your discard pile.  ⤷ note: kept existing dmg 9 (draft's 7 is a nerf)
- "Run Through" — Attack · Common · cost 2: Deal 14 damage. Apply [gold]Tuned[/gold] to 1 attack or skill in hand.  ⤷ note: use existing Run Through here; dropped the "Deal 12 + Tuned 3" version (too close to Back of my Hand)
- "" — Attack · Uncommon · cost 1: Deal 3 damage 3 times. [gold]Invert[/gold] -1-+2+.  ⤷ note: single-target now (was random) — targeting rebalance  ⤷ # suggest: Freestyle
- "" — Attack · Common · cost 1: Deal 6 damage to ALL enemies. [gold]Swap[/gold] -3-+6+.  ⤷ note: AoE (revives Stage Fright's AoE-Swap; supplies the +1 AoE)  ⤷ # suggest: Upstage
- "Crash" — Attack · Common · cost 1: Deal 7 damage. Gain 3 [gold]Vigor[/gold].  ⤷ upg: +3 dmg
- "Desperate Strike" — Attack · Common · cost 1: Deal -20-+24+ damage. Apply -2-+1+ [gold]Weak[/gold] to yourself.
- "" — Attack · Uncommon · cost 2: Deal -10-+14+ damage to ALL enemies. Apply 1 [gold]Vulnerable[/gold] to yourself and ALL enemies.  ⤷ # suggest: Meltdown
- "" — Attack · Uncommon · cost 2: Deal -9-+12+ damage. Gain -9-+12+ [gold]Block[/gold]. Lose 3 [gold]Vigor[/gold].  ⤷ # suggest: Feedback
- "Freeze Up" — Attack · Uncommon · cost 2: Deal 10 damage. Gain 8 [gold]Block[/gold]. Apply -2-+1+ [gold]Weak[/gold] to yourself.  ⤷ note: existing Freeze Up, re-costed 1→2 (house rule: damage+block hybrids cost ≥2)
- "Heart Ache" — Attack · Uncommon · cost 2: Deal 10 damage. Gain 16 [gold]Block[/gold]. Apply -2-+1+ [gold]Vulnerable[/gold] to yourself.
- "Back of my Hand" — Attack · Rare · cost 2: Deal 12 damage. Apply [gold]Tuned[/gold] to all attacks and skills in hand.
- "" — Attack · Uncommon · cost 2: Deal -6-+9+ damage to ALL enemies. You and ALL enemies lose 6 [gold]Vigor[/gold].  ⤷ # suggest: Silence
- "" — Attack · Rare · cost 2: Deal 10 damage. [gold]Invert[/gold] -1-+2+. Remove [gold]Unplayable[/gold] from all attacks and skills in hand.  ⤷ # suggest: Turn It Around
- "Breaking Voice" — Attack · Uncommon · cost 0: Deal -3-+6+ damage. Gain -3-+6+ [gold]Vigor[/gold]. Apply 1 [gold]Weak[/gold] to yourself.
- "One-up" — Attack · Rare · cost 1: [gold]Tuned[/gold] 1. Deal 3 damage 3 times. Apply [gold]Tuned[/gold] to this card.  ⤷ upg: +1 hit
- "Loosen Up" — Attack · Uncommon · cost 2: Deal 5 damage to a random enemy for each [gold]Unplayable[/gold] in hand. Remove [gold]Unplayable[/gold] from all attacks and skills in hand.
- "Showstopper" — Attack · Rare · cost 3: [gold]Tuned[/gold] 1. Deal -28-+34+ damage.
- "Motif" — Attack · Rare · cost 1: Deal -10-+15+ damage. Apply [gold]Planned[/gold] to this card.
- "Clean Slate" — Attack · Rare · cost 1: [gold]Tuned[/gold] 1. Exhaust all [gold]Unplayable[/gold] cards. Deal 4 damage for each exhausted.
- "Experience" — Attack · Rare · cost 2: [gold]Tuned[/gold] 1. Deal 1 damage. Double [gold]Tuned[/gold] on all cards. -[gold]Exhaust[/gold]-.  ⤷ note: was a cost-1 Skill; now a cost-2 Attack that also deals 1
- "Second Nature" — Attack · Rare · cost 3: Deal -24-+30+ damage. At the end of your turn, if this is [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] and play this card.
- "Pathos" — Attack · Uncommon · cost 1: Deal -4-+6+ damage to ALL enemies. Apply 1 [gold]Weak[/gold] and 1 [gold]Vulnerable[/gold] to yourself and ALL enemies.  ⤷ note: X=1 (down from 2)
- "Signature" — Attack · Uncommon · cost 1: [gold]Tuned[/gold] 1. Deal -6-+10+ damage. Gain 1 energy. Starts each combat [gold]Planned[/gold].  ⤷ note: gains energy (existing Signature draws instead)
- "Fate Knocking" — Attack · Rare · cost 2: -[gold]Stable[/gold].- [gold]Tuned[/gold] 1. Deal 1 damage 3 times. Deal damage equal to the sum of the damage this card has dealt this combat.  ⤷ note: rename class Fate → FateKnocking; cost 1→2; Stable now removed on upgrade
- "" — Attack · Common · cost 0: [gold]Tuned[/gold] 1. Deal 3 damage. Draw -1-+2+.  ⤷ # suggest: Shower Thought

## Block / defense skills

- "" — Skill · Common · cost 1: Gain -8-+11+ [gold]Block[/gold]. Remove [gold]Unplayable[/gold] from all attacks and skills in hand.  ⤷ note: Remove-Unplayable stays uncapped (whole hand) by design  ⤷ # suggest: Composure
- "Foreshadow" — Skill · Common · cost 1: Gain 8 [gold]Block[/gold]. Apply [gold]Planned[/gold] to -1-+2+ attacks and skills in your draw pile.  ⤷ note: kept block 8 (draft's 6 is below the cost-1 floor)
- "Tuning Ritual" — Skill · Common · cost 1: Gain -5-+8+ [gold]Block[/gold]. Apply [gold]Tuned[/gold] to 2 attacks and skills in hand.
- "" — Skill · Common · cost 1: Gain -8-+11+ [gold]Block[/gold]. [gold]Invert[/gold] 1.  ⤷ # suggest: Silver Lining
- "Body Double" — Skill · Uncommon · cost 1: Gain -9-+12+ [gold]Block[/gold]. [gold]Swap[/gold] 3.
- "" — Skill · Common · cost 0: Gain 7 [gold]Block[/gold]. Lose 3 [gold]Vigor[/gold].  ⤷ upg: +block  ⤷ # suggest: Muffle
- "" — Skill · Uncommon · cost 2: Gain -12-+16+ [gold]Block[/gold]. Apply 1 [gold]Weak[/gold] to yourself and ALL enemies.  ⤷ # suggest: Dead Weight
- "The Wall" — Skill · Common · cost 1: Gain -16-+20+ [gold]Block[/gold]. Apply -2-+1+ [gold]Vulnerable[/gold] to yourself.

## Planned / Tuned / Invert / Swap skills

- "Magnum Opus" — Skill · Rare · cost 2: Apply [gold]Planned[/gold] to -2-+3+ attacks and skills in your draw pile.
- "Living the Dream" — Skill · Common · cost 1: [gold]Invert[/gold] -2-+3+.  ⤷ note: rarity moved Uncommon → Common (single clean mechanic)
- "Role Reversal" — Skill · Common · cost 1: [gold]Swap[/gold] -6-+10+.  ⤷ note: the single pure-Swap card (Limelight dropped); rarity Uncommon → Common
- "Orchestration" — Skill · Common · cost 1: Draw -1-+2+ cards. Apply [gold]Planned[/gold] to -1-+2+ attacks and skills in hand.
- "" — Skill · Common · cost 1: Draw -1-+2+ cards. Apply [gold]Tuned[/gold] to -1-+2+ attacks and skills in hand.  ⤷ # suggest: Take Notes
- "Write it Down" — Skill · Common · cost 1: Apply [gold]Planned[/gold] and [gold]Tuned[/gold] to -1-+2+ attacks and skills in hand.
- "" — Skill · Uncommon · cost 1: [gold]Swap[/gold] -3-+6+ & [gold]Invert[/gold] -1-+2+ simultaneously.  ⤷ note: record both amounts first, then apply — they never clash  ⤷ # suggest: Give and Take
- "Joke" — Attack · Common · cost 0: Deal 6 damage. [gold]Invert[/gold] -1-+2+. Apply 1 [gold]Vulnerable[/gold] to yourself.  ⤷ note: restored to an Attack (its current-game identity), no longer a skill
- "Canonical" — Skill · Rare · cost 1: [gold]Tuned[/gold] 1. Apply [gold]Stable[/gold] to an attack or skill in hand.  ⤷ upg: -[gold]Exhaust[/gold]-
- "Own It" — Skill · Rare · cost X: [gold]Invert[/gold] -1-+2+. Apply each buff gained X more times.  ⤷ note: cost-X (spend-all) matches existing Own It
- "Memorize" — Skill · Uncommon · cost 3: [gold]Tuned[/gold] 1. Apply [gold]Tuned[/gold] to all attacks or skills with [gold]Unplayable[/gold].

## Vigor / energy (Sounds)

- "Sonic Boom" — Skill · Rare · cost X: Gain -4-+6+ [gold]Vigor[/gold] X times.
- "" — Skill · Uncommon · cost -1-+0+: All creatures in combat gain 6 [gold]Vigor[/gold].  ⤷ note: keep "All creatures in combat" — in multiplayer this also buffs other players, their Osty, and any allies  ⤷ # suggest: Sing Along
- "Forte" — Skill · Uncommon · cost 2: Gain 1 energy. Gain -3-+6+ [gold]Vigor[/gold].
- "" — Attack · Uncommon · cost -2-+1+: Deal 6 damage. Gain 2 energy. Apply 2 [gold]Vulnerable[/gold] to yourself.  ⤷ note: skill → attack; cost 2→1  ⤷ # suggest: Nervous Energy
- "Encore" — Skill · Rare · cost 1: Gain -3-+6+ [gold]Vigor[/gold]. Retain your [gold]Vigor[/gold] after using it this turn.

## Affliction "Un-" skills (Songs)

- "Folk Song" — Skill · Uncommon · cost 3: Gain -2-+3+ [gold]Unweak[/gold].
- "Love Song" — Skill · Uncommon · cost 3: Gain -2-+3+ [gold]Unvulnerable[/gold].
- "" — Skill · Rare · cost 4: Gain 1 [gold]Unweak[/gold], [gold]Unvulnerable[/gold], [gold]Unjaded[/gold], [gold]Unlimited[/gold], and [gold]Unshaken[/gold]. +Starts each combat [gold]Planned[/gold].+  ⤷ note: cost 4 is an intentional capstone (Planned lets you "cheat" it out)  ⤷ # suggest: Playlist
- "" — Skill · Uncommon · cost 3: Gain -2-+3+ [gold]Unshaken[/gold].  ⤷ # suggest: Sad Song
- "" — Skill · Uncommon · cost 3: Gain -2-+3+ [gold]Unjaded[/gold].  ⤷ # suggest: Pop Song
- "" — Skill · Uncommon · cost 3: Gain -2-+3+ [gold]Unlimited[/gold].  ⤷ # suggest: Old Song
- "" — Skill · Rare · cost 2: Remove [gold]Unplayable[/gold] from ALL attacks and skills in your entire deck. Apply -2-+1+ [gold]Shaken[/gold] to yourself.  ⤷ # suggest: Go for Broke
- "" — Skill · Common · cost 1: Draw -3-+4+. Apply 2 [gold]Limited[/gold] to yourself.  ⤷ # suggest: Cram
- "" — Skill · Uncommon · cost 0: Gain -2-+3+ energy. Apply 2 [gold]Jaded[/gold] to yourself.  ⤷ # suggest: Burn Out

## Powers

- "Balanced" — Power · Uncommon · cost -2-+1+: Whenever a card becomes [gold]Unplayable[/gold], remove [gold]Unplayable[/gold] from 1 +of your choice+ -random- attack or skill in hand.
- "Muse" — Power · Uncommon · cost 1: +[gold]Innate[/gold].+ At the start of your turn, apply [gold]Planned[/gold] to an attack or skill in hand.
- "Perfectionism" — Power · Uncommon · cost 1: +[gold]Innate[/gold].+ At the start of your turn, apply [gold]Tuned[/gold] to an attack or skill in hand.
- "Auto Tune" — Power · Rare · cost -2-+1+: At the start of your turn, apply [gold]Tuned[/gold] to all [gold]Tuned[/gold] cards.  ⤷ note: existing Auto Tune is cost 3 ("increase all Tuned by 1"); re-costed + reworded — check power level
- "Bright Side" — Power · Uncommon · cost 1: +[gold]Innate[/gold].+ At the start of your turn, [gold]Invert[/gold] 1.  ⤷ note: existing Bright Side triggers at end of turn; moved to start + Innate
- "" — Power · Uncommon · cost 1: +[gold]Innate[/gold].+ At the start of your turn, [gold]Swap[/gold] 2.  ⤷ # suggest: Stage Presence
- "Reverb" — Power · Uncommon · cost -2-+1+: Double all [gold]Vigor[/gold] gained.  ⤷ note: rename of "Crescendo" (Sounds theme)
- "Crying Out Loud" — Power · Uncommon · cost 1: Whenever a debuff of yours clears, gain -3-+6+ [gold]Vigor[/gold].
- "Another Brick" — Power · Uncommon · cost 1: Whenever a debuff is applied to you, gain -5-+8+ [gold]Block[/gold].
- "The First Lesson" — Power · Rare · cost 1: +[gold]Retain[/gold].+ You cannot become [gold]Weak[/gold] or [gold]Vulnerable[/gold].
- "The Second Lesson" — Power · Rare · cost 2: +[gold]Retain[/gold].+ Obey my [red][sine]Orders[/sine][/red], and be [gold]Rewarded[/gold]. Otherwise, you'll be [gold]Punished[/gold].
- "The Final Lesson" — Power · Rare · cost 3: +[gold]Retain[/gold].+ Stop losing HP. In X turns you will die.  ⤷ note: keep the shipped flavor text
- "My Own Lesson" — Power · Rare · cost 0: +[gold]Retain[/gold].+ [gold]Invertible[/gold] buffs applied to you become nothing. [gold]Invertible[/gold] debuffs applied to you become buffs.  ⤷ note: intended change — buff gains become nothing (previously became debuffs)
- "Improvise" — Skill · Uncommon · cost 2: Remove [gold]Unplayable[/gold] from all [gold]Planned[/gold] cards. Draw a card for each [gold]Unplayable[/gold] removed.  ⤷ note: type was missing → Skill; reworked (draws per-Unplayable-removed) → cost 2
- "Master Form" — Power · Rare · cost 3: +[gold]Retain[/gold].+ When attacks or skills without [gold]Replay[/gold] become [gold]Unplayable[/gold], they gain [gold]Replay[/gold] 1.
- "Held Note" — Power · Rare · cost 2: Turn-based buffs and debuffs no longer decrease by 1 each turn.  ⤷ note: intended broadening from "Invertible" to ALL turn-based (shared `PowerCmd.TickDownDuration` / `SkipNextDurationTick` gate)
- "Muscle Memory" — Power · Rare · cost -2-+1+: [gold]Tuned[/gold] cards will not become [gold]Unplayable[/gold].
- "Apathy" — Power · Uncommon · cost -2-+1+: [gold]Invertible[/gold] debuffs applied to you are reduced by X.
- "One Take" — Power · Rare · cost 3: +[gold]Innate[/gold].+ Decrease all card costs by 1. Apply [gold]Unplayable[/gold] to every played card.  ⤷ note: existing One Take is cost 2, no Innate; re-costed 2→3 + Innate on upgrade
- "" — Power · Uncommon · cost 1: The first time you have an [gold]Unplayable[/gold] card in hand each turn, draw -2-+3+ cards.  ⤷ # suggest: Resourceful

---

## Distribution analysis (vs `DESIGN.md` and current pool)

### Type (non-Basic, n = 83)

| | Attack | Skill | Power |
|---|---|---|---|
| Redesign | 28 (34%) | 35 (42%) | 20 (24%) |
| Current (85) | 31 (36%) | 35 (41%) | 19 (22%) |
| DESIGN band | 29–34% | 40–48% | 21–23% |

Attacks sit at the top of the band (matching Regent/Necrobinder) after converting Joke and Nervous
Energy from skills. Powers at 24% are one point over the ceiling (20 is the max any base character
carries) — acceptable, but don't add more powers.

### Rarity — **20 Common / 37 Uncommon / 26 Rare** (target met)

Achieved via 2 rarity moves (Living the Dream, Role Reversal → Common) + a cost drop (Composure 2→1).
Every absolute rule holds: no Common Powers; Commons are all 1–2 clean effects.

### Cost — richer than base game, **by design**

Mix ≈ 0→7, 1→36 (43%), 2→25 (30%), 3→11, 4→1, X→2 (base game ~55/19/7). This character bypasses
costs with **Planned** (free plays), so high costs are cheaper here — the skew is a deliberate
identity lever. The cost-4 capstone (Playlist) and the cost-3 Un-song family are intentional.

### Attack targeting — matches base game (~81 / 15 / 2)

4 AoE (Upstage, Meltdown, Silence, Pathos), 1 random (Loosen Up), rest single. Multi-hit: Comeback,
Let Loose, One-up, Fate Knocking.

### Setup / Payoff

Setup (Muse, Perfectionism, Foreshadow, Melody, Orchestration, Write it Down, Un-songs), Payoff
(Showtime/Remix/Da Capo/Intermission, Sonic Boom, Turn It Around, Clean Slate, Fate Knocking), and
Double-duty (Signature, Motif, One-up, Give and Take) are all represented.

---

## Naming reference

One flavor family per mechanic: Play Planned = **Performing** · Apply Planned = **Composing** ·
Apply Tuned = **Preparations** · Remove Unplayable = **Overcoming** · Vigor = **Sounds** ·
Un-affliction = **Songs** · self-Weak = **Physical** · self-Vulnerable = **Emotional** ·
Invert = **Self/Positive/Fun** · Swap = **Audience/Impression/Interaction**.

Renames: Venue → **Intermission**, Crescendo → **Reverb**, Fate → **Fate Knocking** (class rename).
Approved new names carried inline as `# suggest:` above. Still-open proposals: **Let Loose** (L12),
**Silver Lining** (L20).
