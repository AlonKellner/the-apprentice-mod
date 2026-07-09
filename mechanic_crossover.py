#!/usr/bin/env python3
"""Build a mechanic x mechanic crossover matrix for The Understudy's card pool.

Reuses mechanics_stats.parse_cards()/MECHANICS (same detection rules, same
scoping notes — see that module's docstring), then for every pair of
mechanics counts how many cards trigger BOTH. The diagonal holds each
mechanic's SOLO count — cards that trigger that mechanic and no other
tracked mechanic — so every card that has at least one tracked mechanic
shows up somewhere in its row/column, not just the ones with crossovers.

Self-debuffs are split into 5 separate mechanics (one per debuff type —
Weak/Vulnerable/Shaken/Limited/Jaded; Frail is omitted since no card
self-inflicts it) rather than one combined "Apply self debuff" bucket, so
crossovers between individual debuff types are visible too.

Note: any card that triggers 3+ tracked mechanics at once (e.g.
`DressRehearsal`, which applies all 5 self-debuff types plus Gain energy)
contributes to multiple off-diagonal cells within a single column — that
column's (diagonal + off-diagonal) total can therefore exceed the
mechanic's unique card count from mechanics_stats.py.
"""

from pathlib import Path

import mechanics_stats as ms

# Short column headers so the matrix fits in a terminal; row labels stay full-length.
ABBREV = {
    "Deal damage":        "Dmg",
    "Gain block":          "Blk",
    "Draw cards":           "Draw",
    "Gain energy":          "Enrg",
    "Remove Unplayable":    "RmUnp",
    "Invert debuffs":       "Invert",
    "Apply self-Weak":      "SlfWeak",
    "Apply self-Vulnerable":"SlfVuln",
    "Apply self-Shaken":    "SlfShkn",
    "Apply self-Limited":   "SlfLtd",
    "Apply self-Jaded":     "SlfJad",
    "Apply all self-debuffs": "SlfAll",
    "Apply enemy debuff":   "EnmyDeb",
    "Apply Planned":        "AppPl",
    "Apply Tense":        "AppTen",
    "Gain Vigor":           "Vigor",
    "Play all Planned":     "PlayPl",
    "Grant Un-X buff":      "GrantUnX",
}


def build_matrix(cards: list[dict]) -> tuple[dict[str, dict[str, list[str]]], dict[str, list[str]]]:
    mechanics = list(ms.ALL_MECHANICS)
    matrix: dict[str, dict[str, list[str]]] = {a: {b: [] for b in mechanics} for a in mechanics}
    solo: dict[str, list[str]] = {m: [] for m in mechanics}
    for c in cards:
        present = [m for m in mechanics if m in c["mechanics"]]
        for a in present:
            for b in present:
                if a != b:
                    matrix[a][b].append(c["name"])
        if len(present) == 1:
            solo[present[0]].append(c["name"])
    return matrix, solo


def render(matrix: dict[str, dict[str, list[str]]], solo: dict[str, list[str]]) -> str:
    mechanics = list(ms.ALL_MECHANICS)
    col_labels = [ABBREV[m] for m in mechanics]
    row_label_w = max(len(m) for m in mechanics) + 2
    col_w = max(max(len(c) for c in col_labels), 3) + 2

    lines = []
    header = " " * row_label_w + "".join(f"{c:>{col_w}}" for c in col_labels)
    lines.append(header)
    lines.append("-" * len(header))
    for a in mechanics:
        row = f"{a:<{row_label_w}}"
        for b in mechanics:
            cell = str(len(solo[a])) if a == b else str(len(matrix[a][b]))
            row += f"{cell:>{col_w}}"
        lines.append(row)
    return "\n".join(lines)


def render_percent_by_column(matrix: dict[str, dict[str, list[str]]], solo: dict[str, list[str]]) -> str:
    """Each column b is normalized so its values (diagonal solo count + every
    off-diagonal crossover) sum to 100% — i.e. cell[a][b] = what share of
    mechanic b's cards are solo (a == b) vs. crossed over with a (a != b).
    A column whose mechanic never appears at all (column sum 0) prints as
    all '-' since there's nothing to take a share of."""
    mechanics = list(ms.ALL_MECHANICS)
    col_labels = [ABBREV[m] for m in mechanics]
    row_label_w = max(len(m) for m in mechanics) + 2
    col_w = max(max(len(c) for c in col_labels), 3) + 2

    col_sums = {
        b: len(solo[b]) + sum(len(matrix[a][b]) for a in mechanics if a != b)
        for b in mechanics
    }

    lines = []
    header = " " * row_label_w + "".join(f"{c:>{col_w}}" for c in col_labels)
    lines.append(header)
    lines.append("-" * len(header))
    for a in mechanics:
        row = f"{a:<{row_label_w}}"
        for b in mechanics:
            count = len(solo[b]) if a == b else len(matrix[a][b])
            if col_sums[b] == 0:
                cell = "-"
            else:
                pct = 100 * count / col_sums[b]
                cell = f"{pct:.0f}%"
            row += f"{cell:>{col_w}}"
        lines.append(row)
    lines.append("-" * len(header))
    totals_row = f"{'Column total (n)':<{row_label_w}}"
    for b in mechanics:
        totals_row += f"{col_sums[b]:>{col_w}}"
    lines.append(totals_row)
    return "\n".join(lines)


def render_over_threshold(
    matrix: dict[str, dict[str, list[str]]], solo: dict[str, list[str]], threshold: float = 35.0
) -> str:
    """List every cell (including diagonal/solo cells) whose column-normalized
    percentage exceeds `threshold`, with the specific cards in that cell.
    A column with total 0 is skipped (nothing to take a share of)."""
    mechanics = list(ms.ALL_MECHANICS)
    col_sums = {
        b: len(solo[b]) + sum(len(matrix[a][b]) for a in mechanics if a != b)
        for b in mechanics
    }

    hits = []
    for b in mechanics:
        if col_sums[b] == 0:
            continue
        for a in mechanics:
            count = len(solo[b]) if a == b else len(matrix[a][b])
            if count == 0:
                continue
            pct = 100 * count / col_sums[b]
            if pct > threshold:
                cards = solo[b] if a == b else matrix[a][b]
                hits.append((pct, a, b, count, col_sums[b], cards))

    if not hits:
        return f"  (no cells over {threshold:.0f}%)"

    hits.sort(key=lambda h: -h[0])
    lines = []
    for pct, a, b, count, total, cards in hits:
        row_label = f"{a} (solo)" if a == b else a
        names = ", ".join(sorted(cards))
        lines.append(f"  {row_label} within {b}: {pct:.0f}% ({count} of {total}) — {names}")
    return "\n".join(lines)


def render_legend() -> str:
    lines = ["Legend:"]
    for full, abbrev in ABBREV.items():
        lines.append(f"  {abbrev:<10} {full}")
    return "\n".join(lines)


def render_nonzero_detail(matrix: dict[str, dict[str, list[str]]]) -> str:
    mechanics = list(ms.ALL_MECHANICS)
    lines = []
    seen = set()
    for a in mechanics:
        for b in mechanics:
            if a == b:
                continue
            pair = frozenset((a, b))
            if pair in seen or not matrix[a][b]:
                continue
            seen.add(pair)
            names = ", ".join(sorted(matrix[a][b]))
            lines.append(f"  {a} x {b} ({len(matrix[a][b])}): {names}")
    return "\n".join(lines) if lines else "  (no crossovers)"


if __name__ == "__main__":
    cards = ms.parse_cards()
    matrix, solo = build_matrix(cards)

    print("THE UNDERSTUDY — MECHANIC CROSSOVER MATRIX")
    print("=" * 70)
    print(f"\nOff-diagonal = # cards that trigger BOTH the row's and column's mechanic.")
    print(f"Diagonal = # cards that trigger ONLY that mechanic (no other tracked one) — every card with a tracked mechanic is now represented somewhere in its row/column.\n")
    print(render_legend())
    print()
    print(render(matrix, solo))
    print()
    print("=" * 70)
    print("NORMALIZED: % OF EACH COLUMN (columns sum to 100%)")
    print("=" * 70)
    print("Cell = of all cards with the COLUMN mechanic, what % are solo (diagonal) vs. also have the ROW mechanic.\n")
    print(render_percent_by_column(matrix, solo))
    print()
    print("=" * 70)
    print("NON-ZERO CROSSOVER DETAIL (each unordered pair listed once)")
    print("=" * 70)
    print(render_nonzero_detail(matrix))
    print()
    print("=" * 70)
    print("CELLS OVER 35% (column-normalized share, including solo/diagonal cells)")
    print("=" * 70)
    print(render_over_threshold(matrix, solo))
