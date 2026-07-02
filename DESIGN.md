# STS2 Card Design: Deep-Dive Analysis

A reference document for The Understudy mod. Covers structural patterns, balance statistics,
design principles, and best practices derived from decompiling all five STS2 characters.
Applies to all current and future Understudy mechanics: Planned, Dreams & Ambitions, Unplayable,
Emotional Expression (debuffs), and future Music combos.

Sources: ilspycmd decompilation of `sts2.dll`, card constructor analysis (435 cards parsed),
and community analysis from [Cloudfall Studios](https://www.cloudfallstudios.com/blog/2020/11/2/game-design-tips-reverse-engineering-slay-the-spires-decisions),
[Mobalytics STS2 tier lists](https://mobalytics.gg/slay-the-spire-2/tier-lists/cards),
and [the STS2 dev philosophy interview](https://www.gamesradar.com/games/roguelike/slay-the-spire-2-devs-want-you-to-break-the-game-as-thats-part-of-the-fun-of-deckbuilders-and-if-somethings-busted-they-can-do-one-of-their-favorite-things-nerf-cards/).

---

## 1. Hard Statistics

All figures exclude Basic, Ancient, and Status cards. "n=81-82" represents a typical non-basic
character pool.

### 1.1 Card Type Distribution

| Character   | Attacks | Skills | Powers | Total |
|-------------|---------|--------|--------|-------|
| Ironclad    | 34 (42%)| 28 (34%)| 19 (23%)| 81 |
| Silent      | 24 (29%)| 40 (48%)| 18 (21%)| 82 |
| Defect      | 26 (32%)| 35 (43%)| 19 (23%)| 80 |
| Regent      | 29 (35%)| 35 (42%)| 18 (21%)| 82 |
| Necrobinder | 31 (38%)| 33 (40%)| 17 (20%)| 81 |
| **Average** | **~29-34%**| **~40-48%**| **~21-23%**| |

Skills dominate. The character with the most attacks (Ironclad) still only reaches 42%.
The floor across all characters is ~29% attack. Powers are the most constrained,
always landing in the 20-23% band — they are carefully rationed.

### 1.2 Rarity Distribution

Every character has almost exactly the same rarity breakdown:
- **Common**: 19-20 cards
- **Uncommon**: 36 cards
- **Rare**: 25-26 cards

This matches the standard deck advice (25 Common / 35 Uncommon / 20 Rare reward ratios).

### 1.3 Type Distribution by Rarity

| Rarity      | n   | Attacks       | Skills        | Powers        |
|-------------|-----|---------------|---------------|---------------|
| Common      | 99  | 54 (54%)      | 45 (45%)      | **0 (0%)**    |
| Uncommon    | 180 | 55 (30%)      | 82 (45%)      | 43 (23%)      |
| Rare        | 127 | 35 (27%)      | 44 (34%)      | 48 (37%)      |

The rarity × type relationship is the single most important structural pattern in STS2:
- Commons are entirely attacks and skills
- Powers unlock at Uncommon and peak at Rare
- As rarity increases, the share of Powers grows while Attacks shrink

### 1.4 Cost Distribution

| Cost | Common | Uncommon | Rare  | Total |
|------|--------|----------|-------|-------|
| 0    | 17     | 33       | 24    | 74    |
| 1    | 69     | 103      | 50    | 222   |
| 2    | 12     | 31       | 34    | 77    |
| 3    | 1      | 11       | 19    | 31    |
| 4+   | 0      | 2        | 0     | 2     |

Cost 1 dominates (~55% of all cards). Across all characters and rarities:
- ~70% of cards cost 0 or 1
- 3-cost cards are rare and skew heavily toward Rare rarity (61% of all 3-cost cards are Rare)
- Commons almost never cost 2+ (only 13 out of 99 do)

Cost breakdown for Powers only (since they have a distinct pattern):
- Uncommon Powers: almost all cost 1 or 2 (no 0-cost, no 3-cost)
- Rare Powers: diverse range (0-3), 0-cost Powers exist but as exceptions (1 in entire game)

### 1.5 Attack Target Distribution

Across all 144 non-basic attack cards:
- **AnyEnemy**: 117 (81%) — the default
- **AllEnemies**: 23 (15%) — 3-7 per character
- **RandomEnemy**: 4 (2%) — exactly 1 per character (Necrobinder has 0)

No attack in the game targets "Self." AoE attacks exist but are not common.
RandomEnemy attacks are the rarest pattern — treat them as spice, not a core pillar.

---

## 2. Absolute Rules (Zero Exceptions in the Source)

These are not conventions — they are universal across all five characters:

1. **Powers never appear at Common rarity.** Zero Common Power cards exist. Not one.
2. **No attack targets Self.** The `TargetType.Self` + `CardType.Attack` combination does not exist.
3. **Block is always a constant.** No card in the base game gives "X block for each Y" at Common.
   Scaling block exists only as a very specific Rare (GeneticAlgorithm permanently increases).
4. **0-cost Powers are near-impossible.** One exists (Neurosurge, Necrobinder Rare). Treat 0-cost
   Powers as almost forbidden.
5. **3-cost Commons do not exist.** One exists (Necrobinder's BorrowedTime), and it is a notable
   outlier specifically designed around paying 3 to gain 4 energy.
6. **Cards that deal damage with each play never use a block multiplier.** BodySlam (deal damage = block)
   is an Uncommon-tier concept, and it upgrades by reducing cost, not increasing the scale.

---

## 3. Card Design by Type

### 3.1 Attack Cards

Attacks form the offensive backbone. The design space is organized around five patterns,
and every character that uses attacks covers the full set (except RandomEnemy, which appears
only as one card per character):

| Pattern            | Example             | Notes |
|--------------------|---------------------|-------|
| Single-hit, 1 cost | Headbutt, PommelStrike | The baseline. 6-12 damage plus a side effect |
| Single-hit, 0 cost | Anger, BeamCell     | Usually weak in isolation, synergy-dependent |
| Single-hit, 2 cost | Bludgeon, Cinder    | High raw damage (20-28) or big side effect |
| Multi-hit, fixed N | TwinStrike (×2), GunkUp (×3), DaggerSpray (AoE ×2) | Lower damage per hit |
| AoE once           | Breakthrough, Thunderclap, DaggerSpray | Lower total damage, hits multiple |
| Random-targeting   | SwordBoomerang (×3) | 1-2 cards max per character |
| Scaling (per state)| Finisher (×attacks), Resentment (×debuffs) | Uncommon/Rare only |

**Side effects on attacks by rarity:**
- Common attacks have 0-1 side effects (usually apply 1 debuff, or draw 1 card)
- Uncommon attacks have 1-2 side effects (debuff + Vulnerable, or hit+draw)
- Rare attacks can have large or conditional effects (deal 60 to all with empty draw pile)

**Self-damage yields much higher output.** Hemokinesis: 2 HP → 15 damage for 1 cost.
BloodWall: 2 HP → 16 block for 2 cost. The premium is roughly ×2 effectiveness.

### 3.2 Skill Cards

Skills are the most varied card type. Their design space spans:

1. **Pure defense**: Gain N block. The bread-and-butter. Cost 1 typical value: 8-14 block.
   Cost 2 typical value: 15-22 block. Never "block per X" at Common.
2. **Defense + effect**: Gain N block AND (draw 1 / apply debuff / generate token).
   These have slightly lower block values to compensate.
3. **Draw/cycle**: Draw N cards. Discard N cards. Draw then discard. Cycle hand.
   Value: Draw 2 for cost 1 is the norm. Draw 3 for cost 1 is good. Draw 3 for cost 0 is Rare.
4. **Token generation**: Create special cards in hand (Shivs, Stars, Minions, Ambitions).
   This is where character identity lives most strongly.
5. **Stat manipulation**: Gain Strength/Dexterity/Focus/Stars. Apply debuffs to enemies.
   Usually via a Power, but sometimes directly in a one-shot Skill.
6. **Energy acceleration**: Gain N energy this turn. Very powerful, usually with cost
   (Bloodletting: lose 3 HP, gain 2 energy; Offering: exhaust, lose 6 HP, gain 2 energy + draw 3).
7. **Conditional effects**: "If draw pile is empty..."; "If you have any Skill in hand..."
   These appear at Uncommon and above.
8. **Card manipulation**: Look at top of deck, put cards on top, reorder hand.

**Skill cost/effect calibration:**
- A 1-cost Skill that only gains block gives 12-16 block (vs. Defend's 5 for 1 — a 2.4-3.2× bonus).
  The "premium" for a slot comes from the higher amount and any side effects.
- Combining two effects on a Skill typically reduces each by 20-30%.
- Skills with Exhaust often provide outsized immediate value (BurningPact: exhaust, pay 1, draw 3).

### 3.3 Power Cards

Powers are the long-term investment engine of a deck. They are exclusively Uncommon or Rare.

**Power archetypes:**
1. **Per-turn trigger**: "At start/end of turn, gain X." (ObsessionPower, IntrospectionPower)
2. **Per-event trigger**: "Whenever Y happens, gain X." (MasochismPower, FeelNoPain)
3. **Passive stat buff**: Apply N Strength/Dexterity/Focus permanently.
4. **Passive state modifier**: Change how cards work going forward (Corruption, SerpentForm).
5. **Scheduled effect**: Count down, then do something (Countdown, Doom powers).

**Power cost guidelines:**
- Cost 1: single mechanic, scales with stacks (e.g., "Each time you play a card, gain 1 Block")
- Cost 2: either a powerful single mechanic or two combined effects
- Cost 3: game-shaping "Form" powers (DemonForm, SerpentForm, BulletTime, WraithForm)
  — these often permanently alter how the entire combat plays out

**Power upgrade patterns:**
- Most common: increase the value (`UpgradeValueBy(1m)` or `UpgradeValueBy(2m)`)
- Alternative: add `CardKeyword.Innate` (power activates in your starting hand) — avoids cost
- The fundamental mechanic never changes on upgrade; only the numbers or timing

---

## 4. Rarity: The Complexity Ladder

Rarity is fundamentally a complexity/power gradient:

| Rarity   | Complexity | Effects | Power cards | Special keywords |
|----------|-----------|---------|-------------|-----------------|
| Common   | 1-2 effects, no conditions, no Powers | Direct: damage, block, draw | None | Rare |
| Uncommon | 2-3 effects, character mechanics emerge, Powers appear | Start combining effects | 23% of pool | More common (Exhaust, Retain) |
| Rare     | Complex conditions, transformative effects, "Form" powers | Can be multi-step or conditional | 37% of pool | Common (Exhaust often) |

**Common design philosophy**: Readable in 1 second. No decision-making required during card play
(beyond choosing a target). A Common card's value should be obvious.

**Uncommon design philosophy**: Requires some thought about timing. Benefits from synergy.
Introduces the character's unique mechanics.

**Rare design philosophy**: May require significant setup to use effectively. Can win a run alone
if properly supported. "Signature" card feel. Often has Exhaust or a severe cost/tradeoff.

---

## 5. Upgrade Patterns

Every card has exactly one upgrade state. The upgrade philosophy:

1. **Increase numbers**: The simplest and most common upgrade. `DamageVar.UpgradeValueBy(4m)`.
   Often: +4 damage, +3 block, +1 card drawn, +1 power stack.
2. **Reduce cost**: `EnergyCost.UpgradeBy(-1)`. Used when the effect is strong but cost is the
   barrier. Most common on 2-cost cards (upgrade to 1) or 1-cost (upgrade to 0).
3. **Add keyword**: `AddKeyword(CardKeyword.Innate)` or `AddKeyword(CardKeyword.Retain)`.
   Elegant — changes behavior without changing numbers or description length.
4. **Multiple values increase**: Both damage AND another value go up (IronWave, FlameBarrier).
5. **Upgrade quality changes**: Remove a downside. E.g., an upgraded card no longer Exhausts,
   or the upgrade condition is relaxed.

**What upgrade never does**: Change the fundamental mechanic. If a card draws cards, the upgrade
draws more cards — it doesn't suddenly also deal damage. If a card applies Weak, the upgrade
applies more Weak stacks or does so without a cost — it doesn't switch to Vulnerable.

---

## 6. Character Identity

Each character's identity lives primarily in their Skills and Powers, not their Attacks.
Attacks are broadly similar (deal damage to an enemy), but the supporting cards define feel.

### Ironclad
- **Identity**: Strength scaling, HP-as-resource, exhaust synergies.
- **Distinctive patterns**: Self-damage yields better output (Hemokinesis, Bloodletting, Offering).
  Exhaust → rewards (DarkEmbrace, FeelNoPain). Strength = primary damage multiplier.
- **Generic base**: Classic warrior archetypes (Bash debuffs, Strike scaling, Defend variants).

### Silent
- **Identity**: Discard engine, poison stacks, Shiv generation, Dexterity.
- **Distinctive patterns**: Hand manipulation (Acrobatics, Prepared). Shiv synergies (Accuracy,
  BladeDance). Poison for long-term DoT. Most Skill-heavy character (48% of cards).
- **Generic base**: Evasion and tactical positioning flavoring basic attacks.

### Defect
- **Identity**: Orb system (Lightning/Frost/Dark/Plasma), Focus amplification.
- **Distinctive patterns**: Channel orbs → Evoke them. Frost = passive block. Lightning = passive
  damage. Dark = accumulating burst. Plasma = energy. Defect is the most "puzzle" character.
- **Generic base**: Mechanical/technological flavor on attacks.

### Regent
- **Identity**: Stars resource, summons, cosmic scaling.
- **Distinctive patterns**: Many cards generate Stars or spend Stars for bonus effects. Summons
  act as independent entities. Most 0-cost cards of all characters (19 at 0-cost vs 11-16 others).
- **Generic base**: Regal/celestial flavor on attacks and blocks.

### Necrobinder
- **Identity**: Minion economy, debuff manipulation, borrowed HP, soul manipulation.
- **Distinctive patterns**: Debuff enemies, then clone/transfer those debuffs (Misery pattern).
  Summon bone/spirit minions. BorrowedTime uses the highest cost in the game (3-cost Common).
- **Generic base**: Death/decay flavor.

---

## 7. The Setup-Payoff Framework

The most durable design principle in STS2 is the **Setup / Payoff** taxonomy
(identified by [Cloudfall Studios](https://www.cloudfallstudios.com/blog/2020/11/2/game-design-tips-reverse-engineering-slay-the-spires-decisions)):

- **Setup cards**: Useful in isolation, but also create conditions that empower other cards.
  Example: Inflame (Uncommon Power) — useful as +2 Strength, but amazing when combined with Whirlwind.
- **Payoff cards**: Strong only when the setup condition is met; weak otherwise.
  Example: Finisher — 6 damage normally, but scales with attacks played this turn.
- **Double-duty cards**: Serve as both setup and payoff.
  Example: ThunderClap — deals AoE damage (payoff) AND applies Vulnerable to all enemies (setup for next turn).

Every card set should have all three types. A set that is all setup or all payoff will feel bad.

**For The Understudy specifically:**
- Planned set: Contemplate/Realize (setup the sequence) + Groove/Epiphany (payoff from position).
- Dreams & Ambitions: Dream/Ambition tokens (setup) + Sublimation/Pastiche (payoff from tokens).
- Emotional Debuffs: SelfPity/PassionateStrike (setup by gaining debuffs) + Outpouring/Breakdown/Phoenix (payoff from debuff count).

---

## 8. Cross-Character Patterns

These patterns appear in every character, confirming they are universal design rules:

### Block Values at Cost 1 (non-exhaust)
| Block gained | Typical secondary | Example |
|---|---|---|
| 5 | (none) | Defend (Basic) |
| 8-10 | Small secondary | Most Common Skills |
| 12-14 | No secondary | Impervious-tier |
| 16+ | Significant cost (exhaust, HP loss, 2-cost) | Impervious (30 block, Rare) |

The effective "floor" for a 1-cost pure-block Skill is ~8 block. Cards offering less must
compensate with strong secondary effects.

### Energy Economy
- 1 energy = 1 card play = the basic unit of action
- Gaining extra energy in combat is extremely powerful (Bloodletting, Offering, BulletTime all exhaust)
- A 0-cost card that only gains 1 energy is Strong (Tactician at 3-cost gains 1 energy — this is
  because Tactician has the `Sly` keyword: triggered on discard, not on play)

### Draw Value
- Drawing 1 card for free (on play of another effect) is a +1 action worth ~0.5-1 cost
- "Draw 2" as a primary effect costs 1 energy consistently
- "Draw 3+" as a primary effect costs 2 energy or has a significant restriction

### Character-Specific Token Mechanics
Each character has 1-3 token types that form the backbone of their synergy engine. These tokens
are created by Skills, consumed or empowered by other Skills, and synergize with Powers:
- Ironclad: Wounds (status), Shouts (none really — direct stat buffs)
- Silent: Shivs (generated attack tokens)
- Defect: Orbs (channeled by Skills, evoked for effects)
- Regent: Stars (resource spent for bonuses)
- Necrobinder: Minions/Souls (summoned entities)

The Understudy's token mechanics: Dreams → Ambitions → Potentials (consumed) and Planned sequence.
These follow the same paradigm.

---

## 9. Design Principles and Best Practices

### 9.1 Rules for Adding a New Card Set

Before adding any card, verify it satisfies these constraints:

**Structural:**
- [ ] Powers are not Common rarity
- [ ] All attacks target an enemy (not self)
- [ ] Block values are constants, not "per X" formulas
- [ ] Random-enemy attacks: at most 1-2 per set
- [ ] 0-cost Powers: avoid entirely unless it's a Rare with significant tradeoff

**Type/Rarity balance (per 20-23 card set):**
- [ ] ~35-40% attacks, ~40-45% skills, ~20-25% powers
- [ ] ~7 Common / ~11 Uncommon / ~5 Rare (for a ~23-card set)
- [ ] Commons: 0 Powers, max 1 multi-step condition
- [ ] Rares: at least 2 Powers or complex conditionals

**Cost balance:**
- [ ] Most cards (60-70%) cost 0 or 1
- [ ] At least 1 card costs 0
- [ ] At most 2-3 cards cost 3+
- [ ] 3-cost cards should be Rare or have extreme power

**Attack pattern completeness:**
- [ ] At least 1 zero-cost attack
- [ ] At least 1 multi-hit attack (fixed N hits)
- [ ] At least 2-3 AoE attacks (AllEnemies)
- [ ] Optional: 1 RandomEnemy attack (adds variety, not required)
- [ ] At least one attack with a significant conditional or scaling effect

**Upgrade completeness:**
- [ ] Every card has a clear upgrade path
- [ ] Most upgrades: increase a number, reduce cost, or add keyword
- [ ] No upgrade changes the fundamental effect
- [ ] For Exhaust cards: consider removing Exhaust on upgrade instead of increasing numbers

### 9.2 Rules for Skills Specifically

Skills are the most diverse type and the primary vehicle for character identity. Subcategories
that every character includes, and The Understudy should too:

1. **Defense anchor**: 1-cost skill that gives substantial block (~12-16). Every character has 2-3.
2. **Draw engine**: A way to draw more cards each turn. At least 1 Common, 1-2 Uncommon.
3. **Character mechanic feeder**: Skills that set up the character-specific mechanics (generate
   tokens, trigger sequences, create Planned cards, etc.)
4. **Removal/transformation**: A way to manage your deck state (exhaust, remove, scry).
5. **Energy generation**: At least 1 Rare that gives energy, likely with a cost.

### 9.3 Rules for Powers Specifically

Powers must be compelling long-term investments. They compete with an entire turn of action.
Design test: "Is playing this power on turn 1 worth doing instead of dealing damage or gaining block?"

- **Trigger frequency matters most**: A power that triggers every card play is worth less per-trigger
  than a power that triggers once per turn (because high-frequency triggers compound with a weak effect,
  while per-turn triggers must be individually strong enough to justify the investment).
- **Stack type determines design**: Counter (stacks additively) vs Single (overwrites) shape how the
  card interacts with multiple copies. Counter Powers should scale clearly — 1 stack = useful,
  2 stacks = great, 3 stacks = overkill doesn't happen often (rare picks).
- **Always ask "what deck does this anchor?"**: A good Power defines a deck archetype around itself.
  IntrospectionPower makes you want to self-debuff more. MasochismPower makes every new debuff
  a reward instead of a punishment. They create positive feedback with the set's core loop.

### 9.4 Common Card Design Checklist

For any Common card:
- [ ] One primary effect and at most one secondary effect
- [ ] No condition required to play (self-explanatory at a glance)
- [ ] Not a Power
- [ ] The effect is useful even without any other synergy in your deck
- [ ] The flavor/name matches the Understudy's emotional/musical identity

### 9.5 Rare Card Design Checklist

For any Rare card:
- [ ] More complex than an Uncommon (condition, or multiple effects, or transformative)
- [ ] Strong enough to be worth building around
- [ ] Has a significant tradeoff if powerful (Exhaust, HP cost, high energy cost)
- [ ] If it's a Power: it should anchor an entire deck strategy
- [ ] The upgrade should feel meaningfully different, not just "+5% effectiveness"

---

## 10. The Emotional Debuffs Set — Applying These Principles

The planned Emotional Debuffs set (23 cards: 7 Common / 11 Uncommon / 5 Rare) was designed
using these principles. Key compliance notes:

**What the set does correctly:**
- Powers (Introspection, Masochism) are Uncommon/Rare — ✓
- Block cards (SelfPity, Wallow, Catharsis) give constant amounts — ✓
- Attack pattern diversity: 0-cost single (Hemorrhage), 1-cost single (PassionateStrike, LashOut,
  Wounded), 2-cost (GutPunch, CryOut), multi-hit (CryOut ×2, Tantrum ×3), AoE (Outburst),
  RandomEnemy (Tantrum), scaling (Spite, Resentment, Breakdown) — ✓
- No self-targeting Attacks — ✓
- Cost distribution: 0-cost (Hemorrhage, Phoenix), 1-cost (majority), 2-cost (5 cards), 3-cost (0) — ✓

**Design tensions to watch:**
- Breakdown ("deal 8 damage per unique debuff to a random enemy") is a scaling attack.
  This is fine at Uncommon/Rare but would be wrong at Common.
- Spite ("deal 4 damage per total debuff stack") — the "per stack" formula means a lot of damage
  variance. This works because Attacks CAN scale; only block cannot.
- Phoenix is 0-cost Skill + Exhaust + conditional AoE. Zero-cost Skills with Exhaust exist in STS2
  (CalculatedGamble, GrandFinale at Rare). Phoenix is Rare — appropriate.
- Masochism (Power, Rare): "Whenever you gain a debuff, gain 6 block." This is a per-event Power
  at Rare, similar to FeelNoPain. The trigger is clear and frequent — this is correct.

**Open design questions resolved by the data:**
- `DexterityPower` exists: confirmed via Silent's Footwork card hover tips.
- `WeakPower`, `VulnerablePower`: confirmed via BeamCell/Thunderclap.
- `FrailPower` name: referenced in Comet (Regent, applies Weak+Vulnerable); Frail likely exists
  as `ShrinkPower` — verify at implementation time with `ilspycmd`.

---

## 11. The Understudy Card Identity

All Understudy cards should feel like they belong to a character who is:
1. Learning, practicing, and growing (Planned, D&A arc: preparation → realization)
2. Emotionally intense, self-aware, sometimes self-destructive (debuffs arc: feeling → expression → release)
3. Musical/artistic (future music arc: rhythm, harmony, resonance)

This identity means:
- **Names** should reflect internal states, artistic processes, or musical vocabulary
- **Mechanics** should feel like emotional/creative processes, not combat abstractions
- **Tradeoffs** are personal — not just "lose HP," but "become Weak" (weaker as a person)
  or "lose Strength" (emotional drain), with recovery as catharsis

When a new card set doesn't feel thematically connected to this arc, it likely doesn't belong
in The Understudy's identity — even if mechanically sound.
