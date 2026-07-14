#!/usr/bin/env python3
"""Scan TheUnderstudyCode/Cards for every card that interacts with a fixed set
of named mechanics, and report exact counts overall, by card type, and by
rarity for each.

Scope/methodology (mirrors self_debuff_stats.py): a card counts for a
mechanic only if ITS OWN top-level Cards/*.cs source directly calls the
action's canonical API. Cards/Powers/*.cs (granted Power effects, e.g.
UnweakPower, StandingByPower) are NOT scanned — if a card's ongoing effect
happens via a granted Power's hook rather than a direct call in the card's
own OnPlay, it is intentionally excluded here, same as TakeNotes (grants
TakeNotesPower, which references Vigor) is excluded from "Gain Vigor" and
StandingBy (grants StandingByPower, which removes Unplayable reactively) is
excluded from "Remove Unplayable". This keeps the count reproducible from
source text alone, at the cost of undercounting indirect/passive effects.

Each mechanic's detection regex was chosen to match its *stated* effect and
avoid false positives from incidental/plumbing uses of the same API:
  - "Remove Unplayable" matches `UnplayableModifier.Remove(` specifically —
    NOT the `CardModifier.DirectModifiers(card).Remove(stillUnplayable)` calls
    inside Encore/CurtainCall/Remix/Performance's own Planned-auto-play loop,
    which only exist to let a multi-slot Planned card play more than once and
    are not that card's advertised mechanic.
  - "Draw cards" matches `CommonActions.Draw(`, not `WithCards(`/CardsVar,
    since several cards use a CardsVar purely as a "how many cards to select"
    count (CramSession, TakeTwo, TouchUp, Rewrite, Performance) with no
    drawing involved at all.
"""

import re
from pathlib import Path
from collections import defaultdict

ROOT = Path(__file__).parent
CARDS_DIR = ROOT / "TheUnderstudyCode" / "Cards"

CONSTRUCTOR_RE = re.compile(
    r":\s*base\(\s*(\d+)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)\s*,\s*TargetType\.(\w+)"
)
CLASS_RE = re.compile(r"public class (\w+)\s*:\s*UnderstudyCard")

NON_CARD_FILES = {"UnderstudyCard", "UnderstudyKeywords", "EmotionalExpression"}

MECHANICS: dict[str, re.Pattern] = {
    "Deal damage":            re.compile(r"WithDamage\("),
    "Gain block":              re.compile(r"WithBlock\("),
    "Draw cards":               re.compile(r"CommonActions\.Draw\("),
    "Gain energy":              re.compile(r"PlayerCmd\.GainEnergy\("),
    "Remove Unplayable":        re.compile(r"UnplayableModifier\.Remove\("),
    "Invert debuffs":           re.compile(r"EmotionalExpression\.(?:InvertEach|InvertLastModified|InvertLastModifiedWithBonus)\("),
    # Self-debuffs are NOT part of this simple regex-membership dict — see
    # SELF_DEBUFF_PATTERNS below. Whether a card lands in one of the 5
    # single-type buckets or the "Apply all self-debuffs" bucket depends on
    # how MANY distinct types its own source applies, which this dict's
    # one-regex-per-mechanic model can't express on its own.
    "Apply enemy debuff":       re.compile(r"PowerCmd\.Apply<(?:Weak|Vulnerable|Shaken|Limited|Jaded|Frail)Power>\(\s*context\s*,\s*(?:cardPlay\.Target|enemy)\b"),
    "Apply Planned":            re.compile(r"PlannedModifier\.Apply\("),
    "Apply Tuned":            re.compile(r"TunedModifier\.Apply\("),
    "Gain Vigor":               re.compile(r"PowerCmd\.Apply<VigorPower>\("),
    "Play all Planned":         re.compile(r"PlannedModifier\.GetSorted\("),
    "Grant Un-X buff":          re.compile(r"PowerCmd\.Apply<Un(?:weak|vulnerable|shaken|jaded|limited|frail)Power>\("),
}

# Self-debuffs: detect which of the 5 types (Frail omitted — no card self-inflicts
# it) a card's own source applies, then classify by COUNT, not just presence:
# exactly one type -> that type's own bucket; two or more (currently only
# DressRehearsal, which applies all 5) -> the shared "applies multiple at once"
# bucket instead, kept separate so a kitchen-sink card never gets counted as
# if it were a dedicated single-debuff card.
SELF_DEBUFF_TYPES = ["Weak", "Vulnerable", "Shaken", "Limited", "Jaded"]
SELF_DEBUFF_PATTERNS = {
    t: re.compile(rf"EmotionalExpression\.Apply{t}ToSelf\(") for t in SELF_DEBUFF_TYPES
}
SELF_DEBUFF_ALL_NAME = "Apply all self-debuffs"

# Full ordered list of every mechanic name this report tracks — the simple
# regex ones from MECHANICS plus the specially-classified self-debuff ones,
# spliced in where "Apply self debuff" used to sit.
ALL_MECHANICS: list[str] = [
    "Deal damage",
    "Gain block",
    "Draw cards",
    "Gain energy",
    "Remove Unplayable",
    "Invert debuffs",
    *[f"Apply self-{t}" for t in SELF_DEBUFF_TYPES],
    SELF_DEBUFF_ALL_NAME,
    "Apply enemy debuff",
    "Apply Planned",
    "Apply Tuned",
    "Gain Vigor",
    "Play all Planned",
    "Grant Un-X buff",
]


def parse_cards() -> list[dict]:
    cards = []
    for cs_file in sorted(CARDS_DIR.glob("*.cs")):
        name = cs_file.stem
        if name in NON_CARD_FILES:
            continue
        text = cs_file.read_text()
        ctor = CONSTRUCTOR_RE.search(text)
        if not ctor:
            continue  # non-card helper file with no base(...) constructor
        cost, card_type, rarity, target = ctor.groups()
        class_m = CLASS_RE.search(text)
        class_name = class_m.group(1) if class_m else name

        mechanics = [m for m, pat in MECHANICS.items() if pat.search(text)]

        matched_debuffs = [t for t, pat in SELF_DEBUFF_PATTERNS.items() if pat.search(text)]
        if len(matched_debuffs) == 1:
            mechanics.append(f"Apply self-{matched_debuffs[0]}")
        elif len(matched_debuffs) > 1:
            mechanics.append(SELF_DEBUFF_ALL_NAME)

        cards.append({
            "name": class_name,
            "cost": int(cost),
            "type": card_type,
            "rarity": rarity,
            "target": target,
            "mechanics": mechanics,
        })
    return cards


def section(title: str, width: int = 70) -> str:
    return f"\n{'=' * width}\n{title}\n{'=' * width}"


def report(cards: list[dict]) -> str:
    lines = []
    lines.append("THE UNDERSTUDY — MECHANIC INTERACTION REPORT")
    lines.append("=" * 70)
    lines.append(f"\nTotal cards scanned (all rarities incl. Basic): {len(cards)}")

    rarities = ["Basic", "Common", "Uncommon", "Rare"]
    types = ["Attack", "Skill", "Power"]

    lines.append(section("OVERVIEW: CARDS PER MECHANIC"))
    header = f"  {'Mechanic':<20}{'# Cards':>9}"
    lines.append(header)
    lines.append("  " + "-" * (len(header) - 2))
    for mechanic in ALL_MECHANICS:
        matching = [c for c in cards if mechanic in c["mechanics"]]
        lines.append(f"  {mechanic:<20}{len(matching):>9}")

    for mechanic in ALL_MECHANICS:
        matching = [c for c in cards if mechanic in c["mechanics"]]
        lines.append(section(f'MECHANIC: "{mechanic}"  ({len(matching)} cards)'))
        names = ", ".join(sorted(c["name"] for c in matching)) if matching else "—"
        lines.append(f"  Cards: {names}")

        lines.append("\n  By type:")
        for t in types:
            count = sum(1 for c in matching if c["type"] == t)
            lines.append(f"    {t:<10}{count:>3}")

        lines.append("\n  By rarity:")
        for r in rarities:
            count = sum(1 for c in matching if c["rarity"] == r)
            lines.append(f"    {r:<10}{count:>3}")

    # --- Grand totals for sanity: type & rarity distribution of the whole pool ---
    lines.append(section("FOR REFERENCE: WHOLE POOL BY TYPE / RARITY"))
    lines.append("  By type:")
    for t in types:
        count = sum(1 for c in cards if c["type"] == t)
        lines.append(f"    {t:<10}{count:>3}")
    lines.append("  By rarity:")
    for r in rarities:
        count = sum(1 for c in cards if c["rarity"] == r)
        lines.append(f"    {r:<10}{count:>3}")

    return "\n".join(lines)


if __name__ == "__main__":
    cards = parse_cards()
    print(report(cards))
