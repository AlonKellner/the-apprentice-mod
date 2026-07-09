using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Character;

public class TheUnderstudyCardPoolTests
{
    [Fact]
    public void UnderstudyStrike_CardId_MatchesExpected()
    {
        Assert.Equal("TheUnderstudy:UnderstudyStrike", UnderstudyStrike.CardId);
    }

    [Fact]
    public void UnderstudyDefend_CardId_MatchesExpected()
    {
        Assert.Equal("TheUnderstudy:UnderstudyDefend", UnderstudyDefend.CardId);
    }

    [Fact]
    public void Performance_CardId_MatchesExpected()
    {
        Assert.Equal("TheUnderstudy:Performance", Performance.CardId);
    }

    [Fact]
    public void Intention_CardId_MatchesExpected()
    {
        Assert.Equal("TheUnderstudy:Intention", Intention.CardId);
    }

    [Fact]
    public void Pool_HasExactly16CommonCards() => Assert.Equal(16, CountBCardsByRarity("Common"));

    [Fact]
    public void Pool_HasExactly33UncommonCards() => Assert.Equal(33, CountBCardsByRarity("Uncommon"));

    [Fact]
    public void Pool_HasExactly26RareCards() => Assert.Equal(26, CountBCardsByRarity("Rare"));

    [Fact]
    public void UnderstudyCard_IsPrePlannedOverriddenOnlyByPromptAndTableRead()
    {
        // The pre-planned mechanic (starting a combat already Planned, originally from the
        // Apprentice's Signature/Prelude cards) is deliberately reused by exactly these two B
        // cards. Verify no other B card type overrides IsPrePlanned.
        var bCardTypes = typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPrePlanned");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "Prompt", "TableRead" }, bCardTypes);
    }

    [Fact]
    public void UnderstudyCard_IsPreIntenseOverriddenOnlyByExpectedCards()
    {
        // The pre-Intense mechanic (starting a combat already carrying Intense 1, mirroring
        // IsPrePlanned's shape) is deliberately reused by exactly these B cards — "big one-off
        // moment" cards that read thematically as "one shining chance, then it's spent."
        var bCardTypes = typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPreIntense");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "CleanSlate", "MissedCue", "Showstopper" }, bCardTypes);
    }

    [Fact]
    public void MissedCue_BeforeCombatStart_DoesNotAttachIntenseModifier_WhenBare()
    {
        // MissedCue.IsPreIntense is true, but a bare-instantiated card has no Pile (Pile == null,
        // so IsCombatPile() is false) — the guard in ApplyPreIntenseIfNeeded must no-op safely
        // rather than crash trying to reach Owner/CombatState on a canonical card.
        var card = new MissedCue();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<IntenseModifier>(out _));
    }

    [Fact]
    public void UnderstudyCard_NoHasExpend_Overrides()
    {
        // B cards manage Unplayable via IntenseModifier (like Expend), not the HasExpend flag.
        var bCardTypes = typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_HasExpend");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .ToList();

        Assert.Empty(bCardTypes);
    }

    [Fact]
    public void UnderstudyStrike_BeforeCombatStart_DoesNotAttachPlannedModifier()
    {
        // UnderstudyStrike.IsPrePlanned stays false (the default) — BeforeCombatStart's
        // pre-planned wiring on UnderstudyCard is a no-op for any card that doesn't override it.
        var card = new UnderstudyStrike();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void TableRead_BeforeCombatStart_DoesNotAttachPlannedModifier_WhenBare()
    {
        // TableRead.IsPrePlanned is true, but a bare-instantiated card has no Pile (Pile == null,
        // so IsCombatPile() is false) — the guard in ApplyPrePlannedIfNeeded must no-op safely
        // rather than crash trying to reach Owner/RelevantCards on a canonical card.
        var card = new TableRead();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void Prompt_BeforeCombatStart_DoesNotAttachPlannedModifier_WhenBare()
    {
        var card = new Prompt();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    private static int CountBCardsByRarity(string rarity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var cardsDir = Path.Combine(root, "TheUnderstudyCode", "Cards");
        var skip = new HashSet<string> { "UnderstudyCard" };
        var bCardPattern = new System.Text.RegularExpressions.Regex(@":\s*UnderstudyCard\b");
        var rarityPattern = new System.Text.RegularExpressions.Regex(
            @":\s*base\(\s*\d+\s*,\s*CardType\.\w+\s*,\s*CardRarity\." + rarity + @"\b");
        return Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !skip.Contains(Path.GetFileNameWithoutExtension(f)))
            .Where(f => bCardPattern.IsMatch(File.ReadAllText(f)))
            .Count(f => rarityPattern.IsMatch(File.ReadAllText(f)));
    }
}
