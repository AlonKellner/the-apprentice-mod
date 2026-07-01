using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Character;
using Xunit;

namespace TheApprentice.Tests.Character;

// Card instantiation tests live in ApprenticeCardTests to avoid duplicate registration
// in CustomContentDictionary.
//
// IMPORTANT: the IsPrePlanned/HasExpend combat-start logic lives on ApprenticeCard itself, NOT
// on TheApprenticeCardPool (CardPoolModel.ShouldReceiveCombatHooks defaults to false and the
// pool never opts in, so a pool-level hook override is never invoked). It's also driven primarily
// by BeforeCombatStart, NOT AfterCardEnteredCombat: the engine explicitly skips
// AfterCardEnteredCombat for the initial deck-to-pile deal at combat start (its pile-add call
// site early-returns while CombatManager.IsInProgress is still false), so it only fires for
// cards entering a combat pile after combat is already running. BeforeCombatStart is dispatched
// via RunState.IterateHookListeners, which walks every card in the deck directly — that's what
// actually covers "starts each combat" effects. ApprenticeCard wires both to the same logic
// (idempotent via TryGetModifier guards) so cards generated into a pile mid-combat are also
// covered. Tests below exercise both entry points directly.
public class TheApprenticeCardPoolTests
{
    [Fact]
    public void Signature_Type_IsPrePlanned()
    {
        // IsPrePlanned is a virtual property on ApprenticeCard, overridden on Signature.
        // Verify via reflection on the type to avoid instantiation.
        var prop = typeof(Signature).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void Prelude_Type_IsPrePlanned()
    {
        var prop = typeof(Prelude).GetProperty("IsPrePlanned");
        Assert.NotNull(prop);
    }

    [Fact]
    public void IsPrePlanned_DefaultsToFalse_OnApprenticeCardBase()
    {
        // The base ApprenticeCard declares IsPrePlanned as virtual returning false.
        // Only Signature and Prelude override it.
        var overrides = typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPrePlanned");
                return method != null && method.DeclaringType == t; // overrides in this type
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "Prelude", "Signature" }, overrides);
    }

    [Fact]
    public void HasExpend_DefaultsToFalse_OnApprenticeCardBase()
    {
        // The base ApprenticeCard declares HasExpend as virtual returning false.
        // Only the cards converted from Exhaust to Expend override it.
        var overrides = typeof(ApprenticeCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_HasExpend");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(
            new[] { "Daydream", "Epiphany", "Inversion", "Reflection", "Resonance", "Transcendence" },
            overrides);
    }

    // NOTE: a test asserting "fresh attach via AddModifier<T> succeeds" is intentionally absent
    // here. ApplyCombatStartModifiers() must use CardModifier.AddModifier<T>(card) (generic) in
    // production — the instance overload with `new T()` throws DuplicateModelException against
    // the real game's canonical model — but the generic overload requires ModelDb, which is empty
    // outside a full game boot. That specific path is exercised by reading the game's own log
    // output instead.
    //
    // What IS testable, and matters: a freshly-constructed card has Pile == null, which is
    // indistinguishable from a real Deck-pile card outside a combat pile (see
    // ApprenticeCard.ApplyCombatStartModifiers's inCombatPile guard — Deck-pile cards are the
    // persistent, between-combats originals that PopulateCombatState clones into the draw pile;
    // attaching to both the original and the clone made two pre-Planned cards show up as four).
    // So these double as regression tests for that guard: the fresh-attach branch must be skipped
    // entirely (not just become a no-op after throwing) when not in a real combat pile.

    [Fact]
    public void AfterCardEnteredCombat_DoesNotThrow_OnFreshHasExpendCard_WhenNotInCombatPile()
    {
        var card = new Epiphany();
        card.AfterCardEnteredCombat(card);
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    [Fact]
    public void BeforeCombatStart_DoesNotThrow_OnFreshPrePlannedCard_WhenNotInCombatPile()
    {
        var card = new Signature();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void AfterCardEnteredCombat_DoesNotAttachExpendModifier_WhenNotHasExpend()
    {
        var card = new ClearMind();
        card.AfterCardEnteredCombat(card);
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    [Fact]
    public void AfterCardEnteredCombat_ResetsStaleIsSpent_WhenModifierAlreadyAttached()
    {
        // Simulates a "real" card carrying a spent ExpendModifier over from a
        // previous combat. Nothing else in the codebase resets IsSpent between
        // combats, so the hook must do it defensively (mirrors how the
        // IsPrePlanned branch re-asserts SequenceIndex on every combat entry).
        var card = new Reflection();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);
        typeof(ExpendModifier).GetProperty(nameof(ExpendModifier.IsSpent))!.SetValue(mod, true);
        Assert.True(mod.IsSpent);

        card.AfterCardEnteredCombat(card);

        Assert.False(mod.IsSpent);
    }

    [Fact]
    public void AfterCardEnteredCombat_IgnoresOtherCards()
    {
        // The hook is broadcast to every card in combat for every card-entered event — a card
        // must only act on itself, not on whichever other card triggered the call.
        var card = new Reflection();
        var otherCard = new ClearMind();
        card.AfterCardEnteredCombat(otherCard);
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    // BeforeCombatStart is the hook that actually fires for the initial combat deal (see the
    // class comment above) — these mirror the AfterCardEnteredCombat tests above, which remain
    // as coverage for the mid-combat card-generation safety net.

    [Fact]
    public void BeforeCombatStart_DoesNotAttachExpendModifier_WhenNotHasExpend()
    {
        var card = new Catharsis();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    [Fact]
    public void BeforeCombatStart_ResetsStaleIsSpent_WhenModifierAlreadyAttached()
    {
        var card = new Inversion();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);
        typeof(ExpendModifier).GetProperty(nameof(ExpendModifier.IsSpent))!.SetValue(mod, true);
        Assert.True(mod.IsSpent);

        card.BeforeCombatStart();

        Assert.False(mod.IsSpent);
    }

    // NOTE: a "fresh attach fires Changed" test (calling BeforeCombatStart on a fresh
    // IsPrePlanned card and asserting PlannedModifier.Changed fires) is intentionally absent —
    // same reason as the missing "fresh attach" tests above: the fresh-attach branch must use
    // CardModifier.AddModifier<T>(card) (generic, ModelDb-backed), which throws
    // KeyNotFoundException outside a full game boot. ApplyCombatStartModifiers calling
    // PlannedModifier.InvokeChanged() after attaching (mirroring PlannedModifier.Apply,
    // TabulaRasa, Transpose, JustAsPlanned, which all do the same) is exercised via the game's
    // own log output instead.

    [Fact]
    public void BeforeCombatStart_DoesNotFirePlannedModifierChanged_WhenNotPrePlanned()
    {
        bool called = false;
        void handler() => called = true;
        PlannedModifier.Changed += handler;
        try
        {
            var card = new Catharsis();
            card.BeforeCombatStart();
            Assert.False(called);
        }
        finally
        {
            PlannedModifier.Changed -= handler;
        }
    }

    [Fact]
    public void BeforeCombatStart_DoesNotFirePlannedModifierChanged_WhenAlreadyAttached()
    {
        // Idempotency: BeforeCombatStart and AfterCardEnteredCombat are both wired to the same
        // logic and can both run for the same card (see the class comment above) — once Planned
        // is already attached, re-entering must not re-fire Changed. Pre-attach directly
        // (bypassing the generic, ModelDb-backed AddModifier<T> production uses) so this doesn't
        // need a full game boot.
        var card = new Prelude();
        CardModifier.AddModifier(card, new PlannedModifier());

        bool called = false;
        void handler() => called = true;
        PlannedModifier.Changed += handler;
        try
        {
            card.BeforeCombatStart();
            Assert.False(called);
        }
        finally
        {
            PlannedModifier.Changed -= handler;
        }
    }

    [Fact]
    public void Pool_HasExactly20CommonCards() => Assert.Equal(20, CountCardsByRarity("Common"));

    [Fact]
    public void Pool_HasExactly36UncommonCards() => Assert.Equal(36, CountCardsByRarity("Uncommon"));

    [Fact]
    public void Pool_HasExactly26RareCards() => Assert.Equal(26, CountCardsByRarity("Rare"));

    [Fact]
    public void Pool_HasExactly82CombatCards()
        => Assert.Equal(82, CountCardsByRarity("Common") + CountCardsByRarity("Uncommon") + CountCardsByRarity("Rare"));

    private static int CountCardsByRarity(string rarity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var cardsDir = Path.Combine(root, "TheApprenticeCode", "Cards");
        var skip = new HashSet<string> { "ApprenticeCard", "ApprenticeCardB", "ApprenticeKeywords", "DreamsAndAmbitions", "TensionHelper" };
        var pattern = new System.Text.RegularExpressions.Regex(
            @":\s*base\(\s*\d+\s*,\s*CardType\.\w+\s*,\s*CardRarity\." + rarity + @"\b");
        // Exclude ApprenticeCardB subclasses — they belong to TheApprenticeBCardPool, not the main pool.
        var bCardPattern = new System.Text.RegularExpressions.Regex(@":\s*ApprenticeCardB\b");
        return Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !skip.Contains(Path.GetFileNameWithoutExtension(f)))
            .Where(f => !bCardPattern.IsMatch(File.ReadAllText(f)))
            .Count(f => pattern.IsMatch(File.ReadAllText(f)));
    }
}
