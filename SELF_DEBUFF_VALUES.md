# Self-debuff card value schema

Self-debuff cards apply an invertible debuff to **yourself** as a downside in exchange for above-rate
reward stats. Their values are assigned by a single consistent formula so that *more self-debuff → more
reward*, scaled the same way every time, with no two same-mechanic cards sharing values.

This is the reference for pricing any current or future self-debuff card. It is enforced by
`TheUnderstudy.Tests/Cards/SelfDebuffValueSchemaTests.cs` — update both together.

## The formula

A card's reward budget, in **value points (vp)**:

```
vp(card) = Base(mechanic, cost, rarity)  +  P·(stacks − 1)  +  sev(debuff)
```

- **`P = 6` vp per extra self-debuff stack** — the dominant, constant term. Going from 1→2 stacks always
  adds the same, significant amount.
- **`sev(debuff)`** — a constant per-type offset ordered by severity (a harsher self-debuff is a bigger
  downside, so it earns more reward), **1 vp per step**:

  | Vulnerable | Jaded | Shaken | Weak | Limited |
  |:---:|:---:|:---:|:---:|:---:|
  | +2 | +1 | 0 | −1 | −2 |

  (worst → best). Debuff type is only a modifier on top of stack count.

- **`Base`** — per mechanic/cost/rarity, chosen to anchor each group near its current power level (see
  the anchor cards in the table below).

### Why it covers the whole grid

Because `P` (6) is larger than the full severity spread (4, from −2 to +2), every `(stacks, debuff)` pair
maps to a **unique, totally-ordered offset**:

- 1-stack offsets: −2 … +2
- 2-stack offsets: +4 … +8
- 3-stack offsets: +10 … +14 …

So stack count always dominates (any 2-stack beats any 1-stack), debuff type breaks ties consistently,
and the system extends to any future stack count or debuff type. Cards of the **same** mechanic are
distinct by construction; cards of **different** mechanics may coincidentally share values (that's fine).

## Converting vp → card stats

Each mechanic has its own `Base` and unit rate:

- **1 damage (single-target) = 1 vp**
- **1 block = 1.25 vp** (block is worth ~25% more than damage)
- AoE damage, draw, energy, etc. are priced within their own mechanic group (the group's `Base` absorbs
  the mechanic premium; the `Δ = P·(stacks−1) + sev` offset is universal).

For attack+block cards, spend the vp across damage and block **freely** — scale one, or trade between
them — choosing a split that gives each card in a group a distinct statline.

### The baseline anchor: Jaded·1 = "+1 cost"

Self-**Jaded** costs you ~1 energy on a later turn, so a self-Jaded card should be worth **at least a
normal card that costs one more**. This sets every group's `Base`: **a self-Jaded·1 card at cost C is
statted like a normal (non-self-debuff) card at cost C+1.** Since `vp(Jaded·1) = Base + sev(J) = Base + 1`,
`Base(mech, C) = normalValue(mech, C+1) − 1`. From there the universal `P·(stacks−1) + sev` offsets set
every other card. Moderate cost premium: **+1 cost ≈ +7 damage / +6 block-vp**.

Normal (non-self-debuff) value curve used for the anchor:

| Mechanic | cost 1 | cost 2 | cost 3 |
|---|:--:|:--:|:--:|
| Single-hit ST damage | ~9 | ~16 | ~24 |
| Attack+block (total vp) | ~15 | ~22 | ~29 |
| Block-only (block) | ~11 | ~17 | — |
| AoE damage | ~5 | ~8 | ~11 |

## Current values

`Δ = P·(stacks−1) + sev(debuff)`. Anchors (marked *) keep their pre-existing values and set each group's
`Base`.

(Card names below are the current music/creative-theme names; the mechanic and values are unchanged
from the original theatrical-theme cards unless noted.)

### Attack + Block, 2-cost Uncommon — Base 28 vp
| Card | debuff · stacks | Δ | vp | Damage / Block |
|---|:---:|:---:|:---:|:---:|
| Freeze Up | Weak · 1 | −1 | 27 | 12 / 12 |
| Heart Ache | Vulnerable · 1 | +2 | 30 | 10 / 16 |

The redesign retired the 1-cost Common attack+block group (Running on Fumes / The Shakes) and Blackout;
damage+block hybrids now cost ≥2, so Freeze Up moved up into this 2-cost group (re-statted 10/8 → 12/12
to match Base 28). Freeze Up's Weak·1 (−1) and Heart Ache's Vulnerable·1 (+2) stay distinct by the
severity offset.

### Single-hit Attack, 1-cost — Base 15 vp
| Card | debuff · stacks | Δ | vp | Damage |
|---|:---:|:---:|:---:|:---:|
| Break a Leg | Vulnerable · 1 | +2 | 17 | 17 |
| Desperate Strike | Weak · 2 | +5 | 20 | 20 |

(A Jaded·1 single-hit here would be 16 damage = a normal 2-cost attack.)

### AoE Attack, 1-cost Uncommon — Base 6 vp
| Card | debuff · stacks | Δ | vp | AoE Damage |
|---|:---:|:---:|:---:|:---:|
| Stage Fright | Shaken · 2 | +6 | 12 | 12 |

### Energy
Missed Cue (Shaken · 1) is now a 1-cost Attack that deals 6 and gains 2 → net +1 energy plus the strike.
Must Go On (Jaded · 2) gains 2 at 0 cost → net +2, clearing Missed Cue by its 2-stack + worse-debuff margin.

## Not priced by a group (same-mechanic singletons)

These have no same-mechanic peer, so there is no duplication constraint; they take the same baseline
lift, spending it mostly on stats while a rider (draw / energy / Invert / Vigor / remove-Unplayable /
Tuned / Planned) absorbs the rest. Current values: Writer's Block (block-only, Weak · 2, **18 block**),
The Wall (block-only, Vulnerable · 1, **16 block**), All-Nighter (attack + draw, Jaded · 1, **11 dmg**),
Procrastinate (attack + Planned, Jaded · 2, **13 dmg**), Buy Time (attack + remove-Unplayable,
Limited · 1, **9 dmg**), Crib Notes (attack + Tuned, Limited · 1, **10 dmg**), Joke (attack + Invert,
Vulnerable · 1, **7 dmg**), Wind Up (attack + Vigor, Weak · 2, **7 dmg**), Missed Cue (attack + energy,
Shaken · 1, **7 dmg** — the +2 energy dominates the budget). Payoff-is-the-rider cards left as-is:
Drawing Blanks (draw 3), Must Go On (+2 energy), Start Over (remove-Unplayable all), Center Stage
(all-5 build-around), Pathos (also debuffs enemies).
