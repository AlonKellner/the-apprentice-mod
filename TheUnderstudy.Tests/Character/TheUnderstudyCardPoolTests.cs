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
    public void Pool_HasExactly8CommonCards() => Assert.Equal(8, CountBCardsByRarity("Common"));

    [Fact]
    public void Pool_HasExactly2UncommonCards() => Assert.Equal(2, CountBCardsByRarity("Uncommon"));

    [Fact]
    public void UnderstudyCard_NoIsPrePlanned_Overrides()
    {
        // B cards do not use the pre-planned mechanic (that's for the original Understudy's
        // Signature/Prelude cards). Verify no B card type overrides IsPrePlanned.
        var bCardTypes = typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Where(t =>
            {
                var method = t.GetMethod("get_IsPrePlanned");
                return method != null && method.DeclaringType == t;
            })
            .Select(t => t.Name)
            .ToList();

        Assert.Empty(bCardTypes);
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
        // B cards have no IsPrePlanned — BeforeCombatStart on UnderstudyCard does nothing for Planned.
        var card = new UnderstudyStrike();
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
