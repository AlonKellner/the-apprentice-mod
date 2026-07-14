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

## Current values

`Δ = P·(stacks−1) + sev(debuff)`. Anchors (marked *) keep their pre-existing values and set each group's
`Base`.

(Card names below are the current music/creative-theme names; the mechanic and values are unchanged
from the original theatrical-theme cards unless noted.)

### Attack + Block, 1-cost Common — Base 16 vp
| Card | debuff · stacks | Δ | vp | Damage / Block |
|---|:---:|:---:|:---:|:---:|
| Freeze Up * | Weak · 1 | −1 | 15 | 5 / 8 |
| Running on Fumes | Jaded · 1 | +1 | 17 | 7 / 8 |
| The Shakes | Shaken · 2 | +6 | 22 | 7 / 12 |

### Attack + Block, 2-cost Uncommon — Base 23.5 vp
| Card | debuff · stacks | Δ | vp | Damage / Block |
|---|:---:|:---:|:---:|:---:|
| Heart Ache * | Vulnerable · 1 | +2 | 25.5 | 8 / 14 |
| Blackout | Limited · 2 | +4 | 27.5 | 10 / 14 |

Blackout's 2 mild-Limited stacks only edge out Heart Ache's 1 harsh-Vulnerable stack (the smallest
2-vs-1 gap, +2 vp) — the severity offset partly cancels the stack premium, exactly as intended.

### Single-hit Attack, 1-cost — Base 13 vp
| Card | debuff · stacks | Δ | vp | Damage |
|---|:---:|:---:|:---:|:---:|
| Break a Leg * | Vulnerable · 1 | +2 | 15 | 15 |
| Desperate Strike | Weak · 2 | +5 | 18 | 18 |

### AoE Attack, 1-cost Uncommon — Base 4 vp
| Card | debuff · stacks | Δ | vp | AoE Damage |
|---|:---:|:---:|:---:|:---:|
| Stage Fright * | Shaken · 2 | +6 | 10 | 10 |

### Energy
Missed Cue (Shaken · 1) is now a 1-cost Attack that deals 6 and gains 2 → net +1 energy plus the strike.
Must Go On (Jaded · 2) gains 2 at 0 cost → net +2, clearing Missed Cue by its 2-stack + worse-debuff margin.

## Not priced by a group (same-mechanic singletons)

These have no same-mechanic peer, so there is no duplication constraint; they keep their current values,
and the schema is the reference if a peer is ever added: Writer's Block and The Wall (block-only —
The Wall trades a lower block for a harsher Vulnerable · 1), Drawing Blanks (draw), Wind Up (attack +
Vigor), Joke (attack + Invert + self-Vulnerable), All-Nighter (attack + draw + self-Jaded), Buy Time
(attack + remove-Unplayable + self-Limited), Crib Notes (attack + self-Tuned + self-Limited),
Procrastinate (attack + self-Planned + self-Jaded), Center Stage (Rare build-around), Pathos (also
debuffs enemies).
