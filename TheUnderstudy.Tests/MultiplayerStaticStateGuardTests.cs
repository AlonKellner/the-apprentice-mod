using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests;

// Enforcement for the multiplayer-determinism strategy (see the fixes plan). Card/power OnPlay hooks run
// in lockstep on every client for every player, and the game-state checksum only sees the model graph —
// so any hidden `static` mutable field that influences a state mutation is a desync waiting to happen
// (this is exactly how the Planned slot counter and the Swap recency counter diverged). Presentation-only
// static state (Godot UI patches under Patches/) is fine and out of scope here.
//
// This test fails when a NEW mutable static field appears under TheUnderstudyCode/Cards (incl. Powers,
// Modifiers) that isn't in the vetted allowlist below. When it fails, either:
//   • refactor the field away (derive the value from the model graph / CombatManager.Instance.History,
//     or store it on a model so it's synced & checksummed — the base-game pattern), OR
//   • if it is genuinely safe (a transient re-entrancy guard, an immutable-after-init cache, or a
//     diagnostic with no gameplay authority), add it here WITH a one-line justification.
//
// `const` and `static readonly IReadOnly*` fields are immutable and auto-exempt (not listed).
public class MultiplayerStaticStateGuardTests
{
    private static string CardsDir => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "TheUnderstudyCode", "Cards"));

    // Vetted mutable static fields, keyed "File.cs::fieldName", each with why it's safe.
    private static readonly HashSet<string> Allowlist = new()
    {
        // Transient [ThreadStatic] re-entrancy guards — single synchronous scope, never cross-client.
        "PlannedModifier.cs::_evaluatingQueueTarget",
        "TunedModifier.cs::_evaluatingTunedTarget",
        "StableEnforcer.cs::Enforcing",
        // Ordered-resolver "prompt for the ally" flag. Set by AutoPlayOrdered and CONSUMED (read+cleared) at
        // the top of the ally card's OnPlay before any pause, so its value is set/read by the same lockstep
        // code on every client and never lingers across a choice pause to leak into another player's
        // interleaved auto-play. Control-flow only (prompt vs random ally), no persisted gameplay state.
        "AllyTargeting.cs::PromptAutoPlayedAlly",
        // Init-once keyword identities (registered from ModelDb at load, then read-only — like enum values,
        // identical on every client). Not per-combat gameplay state.
        "UnderstudyKeywords.cs::Planned",
        "UnderstudyKeywords.cs::Tuned",
        "UnderstudyKeywords.cs::Stable",
        "UnderstudyKeywords.cs::Invert",
        "UnderstudyKeywords.cs::Invertible",
        "UnderstudyKeywords.cs::Swap",
        "UnderstudyKeywords.cs::Swappable",
        "UnderstudyKeywords.cs::SwapAndInvert",
        // Immutable reflection-metadata caches (readonly PropertyInfo / FieldInfo).
        "UnderstudyCard.cs::TipDescriptionProperty",
        "UnderstudyCard.cs::ConstructedHoverTipsField",
        // Notification callback hook, wired once at init (dispatch point, not stored gameplay state).
        "DebuffClearNotifier.cs::DebuffCleared",
        // Immutable-after-init lazy caches of canonical models / registry entries (read-only once built).
        "SceneStealing.cs::_swappableDebuffs",
        "SceneStealing.cs::_swappableBuffs",
        "SceneStealing.cs::_signFlipBuffs",
        "SceneStealing.cs::_swappableEntries",
        // Diagnostic-only once-per-turn registry + its combat token — no gameplay authority (the real
        // guard is the per-instance _resolvedThisTurn field); must never gain any.
        "PlayAllPlannedCard.cs::_resolvedThisTurnGlobal",
        "PlayAllPlannedCard.cs::_diagCombat",
        // Per-player, per-combat guard keyed by Player (deterministic; ConditionalWeakTable, no order state).
        "PrePlannedSetup.cs::_assignedFor",
        // KNOWN SMELL (flagged in the plan): Fate Knocking's running-damage cache + its combat token.
        // Currently consistent across clients (populated from deterministic DamageResults) but should be
        // migrated to a CombatManager.Instance.History-derived sum. Allowlisted, not endorsed.
        "FateKnocking.cs::_damageDealt",
        "FateKnocking.cs::_lastCombat",
    };

    // Matches a single-line static FIELD declaration and captures its name. The [^()=;] guard between
    // `static` and the terminator means a method (name followed by `(`) never matches. The terminator is
    // `;` or an `=` NOT followed by `>`, so an expression-bodied property/accessor (`Name => ...`) is
    // excluded too — only real fields (`Name;` / `Name = ...`) match. `event` declarations are skipped
    // separately (they are dispatch points, a distinct concern from stored state).
    private static readonly Regex FieldDecl = new(
        @"\bstatic\b[^()=;]*?\b(?<name>\w+)\s*(?:=(?!>)|;)", RegexOptions.Compiled);

    [Fact]
    public void NoUnvettedMutableStaticFields_UnderCards()
    {
        Assert.True(Directory.Exists(CardsDir), $"Cards dir not found: {CardsDir}");

        var offenders = new List<string>();
        foreach (var path in Directory.EnumerateFiles(CardsDir, "*.cs", SearchOption.AllDirectories))
        {
            string fileName = Path.GetFileName(path);
            foreach (var raw in File.ReadLines(path))
            {
                string line = raw.Trim();
                if (!line.Contains("static")) continue;
                if (Regex.IsMatch(line, @"\bevent\b")) continue; // events are dispatch points, not stored state
                var m = FieldDecl.Match(line);
                if (!m.Success) continue;

                // Auto-exempt immutable declarations: const, or a readonly IReadOnly* view.
                bool isConst = Regex.IsMatch(line, @"\bconst\b");
                bool isReadonlyImmutableView = Regex.IsMatch(line, @"\breadonly\b") && line.Contains("IReadOnly");
                if (isConst || isReadonlyImmutableView) continue;

                string key = $"{fileName}::{m.Groups["name"].Value}";
                if (!Allowlist.Contains(key))
                    offenders.Add($"{key}   ({line})");
            }
        }

        Assert.True(offenders.Count == 0,
            "Unvetted mutable static field(s) under TheUnderstudyCode/Cards — these are multiplayer-desync " +
            "risks (hidden state outside the checksummed model graph). Refactor to derive from the model " +
            "graph/History, or add to the allowlist in this test with a justification:\n  " +
            string.Join("\n  ", offenders));
    }
}
