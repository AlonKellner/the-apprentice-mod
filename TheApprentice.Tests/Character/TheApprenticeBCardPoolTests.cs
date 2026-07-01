using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests.Character;

public class TheApprenticeBCardPoolTests
{
    [Fact]
    public void StrikeB_CardId_MatchesExpected()
    {
        Assert.Equal("TheApprentice:StrikeB", StrikeB.CardId);
    }

    [Fact]
    public void DefendB_CardId_MatchesExpected()
    {
        Assert.Equal("TheApprentice:DefendB", DefendB.CardId);
    }

    [Fact]
    public void Performance_CardId_MatchesExpected()
    {
        Assert.Equal("TheApprentice:Performance", Performance.CardId);
    }

    [Fact]
    public void Intention_CardId_MatchesExpected()
    {
        Assert.Equal("TheApprentice:Intention", Intention.CardId);
    }

    [Fact]
    public void BPool_HasExactly6CommonCards() => Assert.Equal(6, CountBCardsByRarity("Common"));

    [Fact]
    public void BPool_HasExactly2UncommonCards() => Assert.Equal(2, CountBCardsByRarity("Uncommon"));

    [Fact]
    public void ApprenticeCardB_NoIsPrePlanned_Overrides()
    {
        // B cards do not use the pre-planned mechanic (that's for the original Apprentice's
        // Signature/Prelude cards). Verify no B card type overrides IsPrePlanned.
        var bCardTypes = typeof(ApprenticeCardB).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCardB)) && !t.IsAbstract)
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
    public void ApprenticeCardB_NoHasExpend_Overrides()
    {
        // B cards manage Unplayable via IntenseModifier (like Expend), not the HasExpend flag.
        var bCardTypes = typeof(ApprenticeCardB).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ApprenticeCardB)) && !t.IsAbstract)
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
    public void StrikeB_BeforeCombatStart_DoesNotAttachPlannedModifier()
    {
        // B cards have no IsPrePlanned — BeforeCombatStart on ApprenticeCardB does nothing for Planned.
        var card = new StrikeB();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void DefendB_BeforeCombatStart_DoesNotAttachExpendModifier()
    {
        var card = new DefendB();
        card.BeforeCombatStart();
        Assert.False(card.TryGetModifier<ExpendModifier>(out _));
    }

    private static int CountBCardsByRarity(string rarity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var cardsDir = Path.Combine(root, "TheApprenticeCode", "Cards");
        var skip = new HashSet<string> { "ApprenticeCardB" };
        var bCardPattern = new System.Text.RegularExpressions.Regex(@":\s*ApprenticeCardB\b");
        var rarityPattern = new System.Text.RegularExpressions.Regex(
            @":\s*base\(\s*\d+\s*,\s*CardType\.\w+\s*,\s*CardRarity\." + rarity + @"\b");
        return Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !skip.Contains(Path.GetFileNameWithoutExtension(f)))
            .Where(f => bCardPattern.IsMatch(File.ReadAllText(f)))
            .Count(f => rarityPattern.IsMatch(File.ReadAllText(f)));
    }
}
