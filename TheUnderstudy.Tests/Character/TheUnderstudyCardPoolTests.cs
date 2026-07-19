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
        Assert.Equal("TheUnderstudy:Workshop", Workshop.CardId);
    }

    [Fact]
    public void Buildup_CardId_MatchesExpected()
    {
        Assert.Equal("TheUnderstudy:Practice", Practice.CardId);
    }

    [Fact]
    public void Pool_HasExactly20CommonCards() => Assert.Equal(20, CountBCardsByRarity("Common"));

    [Fact]
    public void Pool_HasAtLeast36UncommonCards() => Assert.True(CountBCardsByRarity("Uncommon") >= 36,
        $"Expected >= 36 Uncommon, got {CountBCardsByRarity("Uncommon")}");

    [Fact]
    public void Pool_HasAtLeast26RareCards() => Assert.True(CountBCardsByRarity("Rare") >= 26,
        $"Expected >= 26 Rare, got {CountBCardsByRarity("Rare")}");

    [Fact]
    public void UnderstudyCard_IsPrePlannedOverriddenOnlyByPromptAndTableRead()
    {
        // The pre-planned mechanic (starting a combat already Planned, originally from the
        // Apprentice's Signature/Prelude cards) is deliberately reused by exactly this B card
        // (Playlist, the other user, was retired). Verify no other B card type overrides IsPrePlanned.
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

        Assert.Equal(new[] { "Playlist", "Signature" }, bCardTypes);
    }

    [Fact]
    public void UnderstudyCard_IsPreTunedOverriddenOnlyByExpectedCards()
    {
        // The pre-Tuned mechanic (starting a combat already carrying Tuned 1, mirroring
        // IsPrePlanned's shape) is deliberately reused by exactly these B cards — "big one-off
        // moment" cards that read thematically as "one shining chance, then it's spent."
        var bCardTypes = typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPreTuned");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(new[] { "CleanSlate", "FateKnocking", "OneUp", "Practice", "ShowerThought", "Showstopper", "Signature" }, bCardTypes);
    }

    [Fact]
    public void PreTunedCard_BeforeCombatStart_DoesNotAttachTunedModifier_WhenBare()
    {
        // CleanSlate.IsPreTuned is true, but a bare-instantiated card has no Pile (Pile == null,
        // so IsCombatPile() is false) — the guard in ApplyPreTunedIfNeeded must no-op safely
        // rather than crash trying to reach Owner/CombatState on a canonical card.
        var card = new CleanSlate();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<TunedModifier>(out _));
    }

    [Fact]
    public void UnderstudyCard_NoHasExpend_Overrides()
    {
        // B cards manage Unplayable via TunedModifier (like Expend), not the HasExpend flag.
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
    public void Signature_BeforeCombatStart_DoesNotAttachPlannedModifier_WhenBare()
    {
        var card = new Signature();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    private static int CountBCardsByRarity(string rarity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var cardsDir = Path.Combine(root, "TheUnderstudyCode", "Cards");
        var skip = new HashSet<string> { "UnderstudyCard", "PlayAllPlannedCard" };
        // Match the abstract resolver base too — Curtain Call/DaCapo/Remix inherit UnderstudyCard through
        // PlayAllPlannedCard, so their source declares ": PlayAllPlannedCard".
        var bCardPattern = new System.Text.RegularExpressions.Regex(@":\s*(?:UnderstudyCard|PlayAllPlannedCard)\b");
        var rarityPattern = new System.Text.RegularExpressions.Regex(
            @":\s*base\(\s*\d+\s*,\s*CardType\.\w+\s*,\s*CardRarity\." + rarity + @"\b");
        return Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !skip.Contains(Path.GetFileNameWithoutExtension(f)))
            .Where(f => bCardPattern.IsMatch(File.ReadAllText(f)))
            .Count(f => rarityPattern.IsMatch(File.ReadAllText(f)));
    }
}
