#!/usr/bin/env python3
"""Generate statistics for The Understudy card set."""

import re
import json
from pathlib import Path
from collections import defaultdict

ROOT = Path(__file__).parent
CARDS_DIR = ROOT / "TheUnderstudyCode" / "Cards"
LOC_FILE = ROOT / "TheUnderstudy" / "localization" / "eng" / "cards.json"

MECHANIC_KEYWORDS = {
    "Planned":                   ["Planned"],
    "Unplayable":                ["Unplayable"],
    "Dreams / Ambitions / Potentials": ["Dream", "Ambition", "Potential"],
    "Tension":                   ["Tension"],
    "Weak / Vulnerable (self-debuffs)": [
        "[gold]Weak[/gold]", "[gold]Vulnerable[/gold]", "Unweak", "Unvulnerable",
    ],
    "Vigor":                     ["Vigor"],
    "Strength manipulation":     ["Strength"],
}

CONSTRUCTOR_RE = re.compile(
    r":\s*base\(\s*(\d+)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)"
)
CLASS_RE = re.compile(r"public class (\w+)\s*:")


def parse_cards() -> list[dict]:
    cards = []
    for cs_file in sorted(CARDS_DIR.glob("*.cs")):
        name = cs_file.stem
        if name in ("UnderstudyKeywords",):
            continue
        text = cs_file.read_text()
        m = CONSTRUCTOR_RE.search(text)
        if not m:
            continue
        cost, card_type, rarity = m.group(1), m.group(2), m.group(3)
        in_library = "showInCardLibrary: false" not in text
        cards.append({
            "name": name,
            "cost": int(cost),
            "type": card_type,
            "rarity": rarity,
            "in_library": in_library,
        })
    return cards


def load_descriptions() -> dict[str, str]:
    with open(LOC_FILE) as f:
        raw = json.load(f)
    return {
        k.replace("THEUNDERSTUDY-", "").replace(".description", ""): v
        for k, v in raw.items()
        if k.endswith(".description")
    }


def loc_key(card_name: str) -> str:
    """Convert CamelCase card name to UPPER_SNAKE_CASE localization key."""
    s = re.sub(r"([A-Z])", r"_\1", card_name).lstrip("_").upper()
    return s


def mechanic_hits(descriptions: dict[str, str]) -> dict[str, list[str]]:
    hits = {}
    for mechanic, keywords in MECHANIC_KEYWORDS.items():
        matching = []
        for key, desc in descriptions.items():
            if any(kw in desc for kw in keywords):
                matching.append(key)
        hits[mechanic] = sorted(matching)
    return hits


def section(title: str, width: int = 60) -> str:
    return f"\n{'='*width}\n{title}\n{'='*width}"


def report(cards: list[dict], descriptions: dict[str, str]) -> str:
    lines = []
    lines.append("THE UNDERSTUDY — CARD STATISTICS REPORT")
    lines.append("=" * 60)

    all_cards = cards
    pool_cards = [c for c in cards if c["rarity"] != "Token"]
    library_cards = pool_cards  # same here; Tokens already excluded

    lines.append(f"\nTotal cards (including tokens): {len(all_cards)}")
    lines.append(f"Total non-token cards:          {len(pool_cards)}")

    # --- By Rarity ---
    lines.append(section("BY RARITY"))
    by_rarity: dict[str, list] = defaultdict(list)
    rarity_order = ["Basic", "Common", "Uncommon", "Rare", "Token"]
    for c in all_cards:
        by_rarity[c["rarity"]].append(c)
    for rarity in rarity_order:
        group = by_rarity.get(rarity, [])
        lines.append(f"  {rarity:<12} {len(group):>3}")

    # --- By Type ---
    lines.append(section("BY CARD TYPE (non-token)"))
    by_type: dict[str, list] = defaultdict(list)
    for c in pool_cards:
        by_type[c["type"]].append(c)
    for t, group in sorted(by_type.items()):
        lines.append(f"  {t:<12} {len(group):>3}")

    # --- Rarity × Type breakdown ---
    lines.append(section("RARITY × TYPE BREAKDOWN (non-token)"))
    types = sorted(by_type.keys())
    header = f"  {'Rarity':<12}" + "".join(f"{t:>10}" for t in types) + f"{'Total':>10}"
    lines.append(header)
    lines.append("  " + "-" * (12 + 10 * len(types) + 10))
    for rarity in rarity_order[:-1]:  # skip Token
        row_cards = [c for c in pool_cards if c["rarity"] == rarity]
        row = f"  {rarity:<12}"
        for t in types:
            count = sum(1 for c in row_cards if c["type"] == t)
            row += f"{count:>10}"
        row += f"{len(row_cards):>10}"
        lines.append(row)
    # Totals row
    lines.append("  " + "-" * (12 + 10 * len(types) + 10))
    total_row = f"  {'Total':<12}"
    for t in types:
        total_row += f"{len(by_type[t]):>10}"
    total_row += f"{len(pool_cards):>10}"
    lines.append(total_row)

    # --- By Cost ---
    lines.append(section("BY ENERGY COST (non-token)"))
    by_cost: dict[int, list] = defaultdict(list)
    for c in pool_cards:
        by_cost[c["cost"]].append(c)
    for cost in sorted(by_cost.keys()):
        lines.append(f"  Cost {cost}: {len(by_cost[cost]):>3}")

    # --- By Mechanic ---
    lines.append(section("BY MECHANIC (cards whose text references mechanic)"))
    hits = mechanic_hits(descriptions)
    for mechanic, card_keys in hits.items():
        # Resolve back to card names (tokens and non-tokens)
        all_names = {loc_key(c["name"]): c["name"] for c in all_cards}
        matched_names = [all_names.get(k, k) for k in card_keys]
        lines.append(f"\n  {mechanic}: {len(card_keys)} cards")
        wrapped = ", ".join(sorted(matched_names))
        # Wrap at ~60 chars
        import textwrap
        for line in textwrap.wrap(wrapped, width=56, initial_indent="    ", subsequent_indent="    "):
            lines.append(line)

    # --- Token cards ---
    lines.append(section("TOKEN CARDS (not in card library)"))
    tokens = [c for c in all_cards if c["rarity"] == "Token"]
    for t in tokens:
        lines.append(f"  {t['name']:<20} {t['type']}")

    return "\n".join(lines)


if __name__ == "__main__":
    cards = parse_cards()
    descriptions = load_descriptions()
    print(report(cards, descriptions))
