using BaseLib.Abstracts;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class DestinedTests
{
    [Fact]
    public void Destined_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Destined", Destined.CardId);

    [Fact]
    public void CanReceivePlanned_SpentDream_ReturnsTrue()
    {
        var card = new Dream();
        var mod = new ExpendModifier();
        CardModifier.AddModifier(card, mod);
        typeof(ExpendModifier).GetProperty(nameof(ExpendModifier.IsSpent))!.SetValue(mod, true);
        // Unplayable is now tracked via UnplayableModifier; add it directly to simulate OnPlay.
        var unplayable = new UnplayableModifier();
        CardModifier.AddModifier(card, unplayable);

        Assert.True(card.IsUnplayable());
        // Planned now also accepts Attacks and Skills that are Unplayable (same type check).
        Assert.True(Destined.CanReceivePlanned(card));
    }

    [Fact]
    public void CanReceivePlanned_UnspentAmbition_ReturnsTrue()
    {
        var card = new Ambition();
        Assert.True(Destined.CanReceivePlanned(card));
    }

    [Fact]
    public void CanReceivePlanned_AlreadyPlanned_ReturnsTrue()
    {
        // Planned stacks: re-applying to an already-Planned token adds a second queue slot.
        var card = new Potential();
        var plannedMod = new PlannedModifier();
        plannedMod.SequenceIndices.Add(0);
        CardModifier.AddModifier(card, plannedMod);
        Assert.True(Destined.CanReceivePlanned(card));
    }
}
