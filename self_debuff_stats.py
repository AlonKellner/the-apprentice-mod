#!/usr/bin/env python3
"""Scan TheUnderstudyCode/Cards for every card that inflicts an invertible
debuff (Weak/Vulnerable/Shaken/Limited/Jaded/Frail) on its own owner, and
report exact counts by debuff, rarity, card type, and stack amount, plus a
full per-card detail table.

A card counts only if it calls one of EmotionalExpression's Apply*ToSelf
helpers (ApplyWeakToSelf, ApplyVulnerableToSelf, ApplyShakenToSelf,
ApplyLimitedToSelf, ApplyJadedToSelf, ApplyFrailToSelf) — those helpers are
inherently self-targeting, so a match is unambiguous. Cards that inflict a
debuff on an enemy (e.g. via PowerCmd.Apply<WeakPower>(context, cardPlay.Target, ...))
are excluded by construction.
"""

import re
from pathlib import Path
from collections import defaultdict

ROOT = Path(__file__).parent
CARDS_DIR = ROOT / "TheUnderstudyCode" / "Cards"

DEBUFFS = ["Weak", "Vulnerable", "Shaken", "Limited", "Jaded", "Frail"]

CONSTRUCTOR_RE = re.compile(
    r":\s*base\(\s*(\d+)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)\s*,\s*TargetType\.(\w+)"
)
SELF_APPLY_RE = re.compile(
    r"EmotionalExpression\.Apply(" + "|".join(DEBUFFS) + r")ToSelf\(\s*context\s*,\s*[^,]+,\s*([^,]+),"
)
# Matches a debuff/amount var declaration, whether a plain IntVar or the display-only SelfDebuffVar
# (self-debuff stack counts are declared as SelfDebuffVar("<Type>", N)).
INT_VAR_DECL_RE = re.compile(r'new (?:IntVar|SelfDebuffVar)\(\s*"(\w+)"\s*,\s*(\d+)\s*\)')
# Matches an inline read of a card var: `(int)DynamicVars["Shaken"].BaseValue` passed straight to Apply.
DIRECT_DYNVAR_RE = re.compile(r'DynamicVars\[\s*"(\w+)"\s*\]\.BaseValue')
LOCAL_VAR_FROM_DYNVAR_RE = re.compile(
    r'\bint\s+(\w+)\s*=\s*\(int\)DynamicVars\["(\w+)"\]\.BaseValue'
)
# `int amount = (int)DynamicVars.Power<SomePower>().BaseValue;` — the WithPower<T>(base, upgrade)
# idiom (e.g. DressRehearsal), where one declared amount drives both a card-level effect and its
# granted Power's Amount. The dynvar key here is the Power's own type name, not a string literal.
LOCAL_VAR_FROM_POWER_RE = re.compile(
    r'\bint\s+(\w+)\s*=\s*\(int\)DynamicVars\.Power<(\w+)>\(\)\.BaseValue'
)
WITH_POWER_DECL_RE = re.compile(r'WithPower<(\w+)>\(\s*(\d+)\s*(?:,\s*(\d+)\s*)?\)')
UPGRADE_NAMED_RE = re.compile(r'DynamicVars\["(\w+)"\]\.UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)m?\s*\)')
UPGRADE_STD_RE = re.compile(r'DynamicVars\.(Damage|Block|Cards)\.UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)m?\s*\)')

WITH_DAMAGE_RE = re.compile(r'WithDamage\(\s*(\d+)\s*\)')
WITH_BLOCK_RE = re.compile(r'WithBlock\(\s*(\d+)\s*\)')
WITH_CARDS_RE = re.compile(r'WithCards\(\s*(\d+)\s*\)')
ENERGY_VAR_RE = re.compile(r'new EnergyVar\(\s*(\d+)\s*\)')
MULTI_HIT_RE = re.compile(r'CardAttack\([^)]*?,\s*(\d+)\s*\)')
CLASS_RE = re.compile(r'public class (\w+)\s*:\s*UnderstudyCard')

NON_CARD_FILES = {"UnderstudyCard", "UnderstudyKeywords", "EmotionalExpression"}


def resolve_amount(expr: str, text: str) -> tuple[int, str | None]:
    """Return (amount, dynvar_key). dynvar_key is None for a hardcoded literal
    (which therefore never changes on upgrade)."""
    expr = expr.strip()
    if expr.isdigit():
        return int(expr), None

    # Variable form: `int jaded = (int)DynamicVars["Jaded"].BaseValue;` then
    # `ApplyJadedToSelf(context, creature, jaded, this)`.
    m = LOCAL_VAR_FROM_DYNVAR_RE.search(text)
    if m and m.group(1) == expr:
        key = m.group(2)
        base_m = INT_VAR_DECL_RE.search(text)
        if base_m and base_m.group(1) == key:
            return int(base_m.group(2)), key
        # fall back: find any IntVar decl matching the key anywhere in file
        for km, kv in INT_VAR_DECL_RE.findall(text):
            if km == key:
                return int(kv), key

    # WithPower<T>(base, upgrade) form: `int amount = (int)DynamicVars.Power<DressRehearsalPower>().BaseValue;`
    m = LOCAL_VAR_FROM_POWER_RE.search(text)
    if m and m.group(1) == expr:
        power_type = m.group(2)
        for pt, base, _upgrade in WITH_POWER_DECL_RE.findall(text):
            if pt == power_type:
                return int(base), f"POWER:{power_type}"

    # Inline form: `ApplyShakenToSelf(..., (int)DynamicVars["Shaken"].BaseValue, this)` — read the key
    # straight from the expression and look up its SelfDebuffVar/IntVar declaration.
    m = DIRECT_DYNVAR_RE.search(expr)
    if m:
        key = m.group(1)
        for km, kv in INT_VAR_DECL_RE.findall(text):
            if km == key:
                return int(kv), key

    raise ValueError(f"Could not resolve debuff amount expression {expr!r}")


def upgraded_value(base: int, dynvar_key: str | None, text: str) -> int:
    if dynvar_key is None:
        return base
    if dynvar_key.startswith("POWER:"):
        power_type = dynvar_key[len("POWER:"):]
        for pt, _base, upgrade in WITH_POWER_DECL_RE.findall(text):
            if pt == power_type:
                return base + int(upgrade or 0)
        return base
    m = UPGRADE_NAMED_RE.search(text)
    if m and m.group(1) == dynvar_key:
        return base + int(float(m.group(2)))
    for km, kv in UPGRADE_NAMED_RE.findall(text):
        if km == dynvar_key:
            return base + int(float(kv))
    return base


def describe_reward(text: str, target_type: str) -> str:
    parts = []

    dmg = WITH_DAMAGE_RE.search(text)
    if dmg:
        base = int(dmg.group(1))
        delta_m = UPGRADE_STD_RE.search(text)
        deltas = {k: int(float(v)) for k, v in UPGRADE_STD_RE.findall(text)}
        upgraded = base + deltas.get("Damage", 0)
        hits = MULTI_HIT_RE.search(text)
        suffix = f" x{hits.group(1)} hits" if hits else ""
        target_suffix = " to ALL enemies" if target_type == "AllEnemies" else \
            (" to a random enemy" if target_type == "RandomEnemy" else "")
        parts.append(f"Deal {base}({upgraded}) damage{target_suffix}{suffix}")

    blk = WITH_BLOCK_RE.search(text)
    if blk:
        base = int(blk.group(1))
        deltas = {k: int(float(v)) for k, v in UPGRADE_STD_RE.findall(text)}
        upgraded = base + deltas.get("Block", 0)
        parts.append(f"Gain {base}({upgraded}) Block")

    crd = WITH_CARDS_RE.search(text)
    if crd:
        base = int(crd.group(1))
        deltas = {k: int(float(v)) for k, v in UPGRADE_STD_RE.findall(text)}
        upgraded = base + deltas.get("Cards", 0)
        parts.append(f"Draw {base}({upgraded}) cards")

    energy = ENERGY_VAR_RE.search(text)
    if energy:
        parts.append(f"Gain {energy.group(1)} Energy")

    # Any other named IntVar not already accounted for as a debuff amount
    # (e.g. Vigor) — best-effort extra reward.
    accounted_keys = {d for d in DEBUFFS}
    for key, base in INT_VAR_DECL_RE.findall(text):
        if key in accounted_keys:
            continue
        base = int(base)
        upgraded = upgraded_value(base, key, text)
        parts.append(f"Gain {base}({upgraded}) {key}")

    return ", ".join(parts) if parts else "(no numeric reward detected)"


def parse_cards() -> list[dict]:
    cards = []
    for cs_file in sorted(CARDS_DIR.glob("*.cs")):
        name = cs_file.stem
        if name in NON_CARD_FILES:
            continue
        text = cs_file.read_text()

        applies = SELF_APPLY_RE.findall(text)
        if not applies:
            continue

        ctor = CONSTRUCTOR_RE.search(text)
        if not ctor:
            raise ValueError(f"{name}: could not parse base(...) constructor")
        cost, card_type, rarity, target = ctor.groups()

        class_m = CLASS_RE.search(text)
        class_name = class_m.group(1) if class_m else name

        debuffs = []
        for debuff_name, amount_expr in applies:
            amount, dynvar_key = resolve_amount(amount_expr, text)
            upgraded = upgraded_value(amount, dynvar_key, text)
            debuffs.append({
                "debuff": debuff_name,
                "amount": amount,
                "upgraded_amount": upgraded,
                "amount_changes_on_upgrade": upgraded != amount,
            })

        cards.append({
            "name": class_name,
            "cost": int(cost),
            "type": card_type,
            "rarity": rarity,
            "target": target,
            "debuffs": debuffs,
            "reward": describe_reward(text, target),
        })
    return cards


def section(title: str, width: int = 70) -> str:
    return f"\n{'=' * width}\n{title}\n{'=' * width}"


def report(cards: list[dict]) -> str:
    lines = []
    lines.append("THE UNDERSTUDY — SELF-INFLICTED INVERTIBLE DEBUFF REPORT")
    lines.append("=" * 70)
    lines.append(f"\nTotal cards that self-inflict an invertible debuff: {len(cards)}")

    total_instances = sum(len(c["debuffs"]) for c in cards)
    lines.append(f"Total debuff-application instances (a 5-debuff card counts 5): {total_instances}")

    # --- By debuff type ---
    lines.append(section("BY DEBUFF TYPE"))
    by_debuff: dict[str, list[str]] = defaultdict(list)
    for c in cards:
        for d in c["debuffs"]:
            by_debuff[d["debuff"]].append(c["name"])
    for debuff in DEBUFFS:
        names = by_debuff.get(debuff, [])
        lines.append(f"  {debuff:<12} {len(names):>3}   {', '.join(names) if names else '—'}")

    # --- Debuff x Rarity cross-tab ---
    lines.append(section("DEBUFF x RARITY (instances; a card in multiple cells means it applies multiple debuffs)"))
    rarities = ["Common", "Uncommon", "Rare"]
    header = f"  {'Debuff':<12}" + "".join(f"{r:>10}" for r in rarities) + f"{'Total':>10}" + f"{'Excl. shared':>16}"
    lines.append(header)
    lines.append("  " + "-" * (len(header) - 2))
    shared_cards = [c["name"] for c in cards if len(c["debuffs"]) > 1]
    for debuff in DEBUFFS:
        row = f"  {debuff:<12}"
        total = 0
        excl_total = 0
        for r in rarities:
            count = sum(1 for c in cards if c["rarity"] == r for d in c["debuffs"] if d["debuff"] == debuff)
            excl_count = sum(
                1 for c in cards if c["rarity"] == r and c["name"] not in shared_cards
                for d in c["debuffs"] if d["debuff"] == debuff
            )
            total += count
            excl_total += excl_count
            row += f"{count:>10}"
        row += f"{total:>10}"
        row += f"{excl_total:>16}"
        lines.append(row)
    lines.append(
        "\n  \"Excl. shared\" = count after dropping cards that apply more than one debuff\n"
        "  (i.e. DressRehearsal), to show how much *dedicated* representation each debuff\n"
        "  has at each rarity on its own."
    )

    # --- Debuff x Target cross-tab ---
    lines.append(section("DEBUFF x TARGET TYPE (instances)"))
    targets = ["None", "AnyEnemy", "AllEnemies", "RandomEnemy"]
    header = f"  {'Debuff':<12}" + "".join(f"{t:>13}" for t in targets)
    lines.append(header)
    lines.append("  " + "-" * (len(header) - 2))
    for debuff in DEBUFFS:
        row = f"  {debuff:<12}"
        for t in targets:
            count = sum(1 for c in cards if c["target"] == t for d in c["debuffs"] if d["debuff"] == debuff)
            row += f"{count:>13}"
        lines.append(row)

    # --- By rarity ---
    lines.append(section("BY RARITY (cards, not instances)"))
    by_rarity: dict[str, list[str]] = defaultdict(list)
    for c in cards:
        by_rarity[c["rarity"]].append(c["name"])
    for rarity in ["Common", "Uncommon", "Rare"]:
        names = by_rarity.get(rarity, [])
        lines.append(f"  {rarity:<12} {len(names):>3}   {', '.join(names) if names else '—'}")

    # --- By card type ---
    lines.append(section("BY CARD TYPE (cards, not instances)"))
    by_type: dict[str, list[str]] = defaultdict(list)
    for c in cards:
        by_type[c["type"]].append(c["name"])
    for t in sorted(by_type.keys()):
        names = by_type[t]
        lines.append(f"  {t:<12} {len(names):>3}   {', '.join(names) if names else '—'}")

    # --- By amount applied (instance-level) ---
    lines.append(section("BY BASE STACK AMOUNT (instances)"))
    by_amount: dict[int, list[str]] = defaultdict(list)
    for c in cards:
        for d in c["debuffs"]:
            by_amount[d["amount"]].append(f"{c['name']}:{d['debuff']}")
    max_amount = max(by_amount.keys()) if by_amount else 0
    for amt in range(1, max(3, max_amount) + 1):
        instances = by_amount.get(amt, [])
        label = f"{amt} stack" + ("s" if amt != 1 else "")
        lines.append(f"  {label:<12} {len(instances):>3}   {', '.join(instances) if instances else '—'}")
    more = [inst for a, insts in by_amount.items() if a > 2 for inst in insts]
    lines.append(f"  {'>2 stacks':<12} {len(more):>3}   {', '.join(more) if more else '—'}")

    # --- Full detail table ---
    lines.append(section("FULL DETAIL TABLE"))
    header = f"  {'Card':<18}{'Type':<8}{'Rarity':<10}{'Cost':<5}{'Self-Debuff(s)':<40}{'Reward'}"
    lines.append(header)
    lines.append("  " + "-" * (len(header) + 20))
    for c in sorted(cards, key=lambda c: c["name"]):
        debuff_str = ", ".join(
            f"{d['debuff']} {d['amount']}"
            + (f"->{d['upgraded_amount']}" if d["amount_changes_on_upgrade"] else "")
            for d in c["debuffs"]
        )
        debuff_col = f"{debuff_str:<40}" if len(debuff_str) < 40 else debuff_str + "  "
        lines.append(
            f"  {c['name']:<18}{c['type']:<8}{c['rarity']:<10}{c['cost']:<5}{debuff_col}{c['reward']}"
        )

    return "\n".join(lines)


if __name__ == "__main__":
    cards = parse_cards()
    print(report(cards))
