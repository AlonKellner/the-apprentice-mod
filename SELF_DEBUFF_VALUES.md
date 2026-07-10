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

### Attack + Block, 1-cost Common — Base 16 vp
| Card | debuff · stacks | Δ | vp | Damage / Block |
|---|:---:|:---:|:---:|:---:|
| Freeze Up * | Weak · 1 | −1 | 15 | 5 / 8 |
| Matinee | Jaded · 1 | +1 | 17 | 7 / 8 |
| Butterflies | Shaken · 2 | +6 | 22 | 7 / 12 |

### Attack + Block, 2-cost Uncommon — Base 23.5 vp
| Card | debuff · stacks | Δ | vp | Damage / Block |
|---|:---:|:---:|:---:|:---:|
| Overcommit * | Vulnerable · 1 | +2 | 25.5 | 8 / 14 |
| Overexert | Limited · 2 | +4 | 27.5 | 10 / 14 |

Overexert's 2 mild-Limited stacks only edge out Overcommit's 1 harsh-Vulnerable stack (the smallest
2-vs-1 gap, +2 vp) — the severity offset partly cancels the stack premium, exactly as intended.

### Single-hit Attack, 1-cost — Base 13 vp
| Card | debuff · stacks | Δ | vp | Damage |
|---|:---:|:---:|:---:|:---:|
| Quick Nap * | Jaded · 1 | +1 | 14 | 14 |
| Downstage | Vulnerable · 1 | +2 | 15 | 15 |
| Stage Whisper | Weak · 2 | +5 | 18 | 18 |

### AoE Attack, 1-cost Uncommon — Base 4 vp
| Card | debuff · stacks | Δ | vp | AoE Damage |
|---|:---:|:---:|:---:|:---:|
| Wide Open * | Vulnerable · 1 | +2 | 6 | 6 |
| Take Center Stage | Shaken · 2 | +6 | 10 | 10 |

### Energy
Opening Number (Shaken · 1) gains 2 at 1 cost with Exhaust → net +1 energy. All-Nighter (Jaded · 2) gains
2 at 0 cost → net +2, clearing Opening Number by its 2-stack + worse-debuff margin.

## Not priced by a group (same-mechanic singletons)

These have no same-mechanic peer, so there is no duplication constraint; they keep their current values,
and the schema is the reference if a peer is ever added: Understatement (block-only), Fast Forward (draw),
Wind Up (attack + Vigor), True Colors (Invert), Off Script (random multi-hit), Missed Cue and Dress
Rehearsal (Rare build-arounds), Ensemble (also debuffs enemies). Flourish (6×2, Limited · 1) and Rerun
(8×2, Jaded · 2) already sit on the schema across their rarity/stack difference.
