using BaseLib.Abstracts;
using TheApprentice.TheApprenticeCode;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using Xunit;

namespace TheApprentice.Tests;

// AppendPlannedFallback is registered against BaseLib's DescriptionOverrides.CustomizeDescriptionPost
// event in MainFile.Initialize, which only fires via a Harmony patch on CardModel.GetDescriptionForPile
// — not invokable without a full game boot. The method itself has no such dependency, so it's
// exercised directly here instead.
public class MainFileTests
{
    [Fact]
    public void AppendPlannedFallback_AppendsPlannedText_WhenIsPrePlanned_AndNoModifierAttached()
    {
        var card = new Signature();
        string description = "Deal 12 damage.";

        MainFile.AppendPlannedFallback(card, ref description);

        Assert.Equal("Deal 12 damage.\n[gold]Planned[/gold].", description);
    }

    [Fact]
    public void AppendPlannedFallback_DoesNotAppend_WhenNotPrePlanned()
    {
        var card = new ClearMind();
        string description = "Some description.";

        MainFile.AppendPlannedFallback(card, ref description);

        Assert.Equal("Some description.", description);
    }

    [Fact]
    public void AppendPlannedFallback_DoesNotAppend_WhenModifierAlreadyAttached()
    {
        // Once the real PlannedModifier is attached (i.e. combat has started — see
        // ApprenticeCard.BeforeCombatStart), the modifier's own ModifyDescriptionPost shows the
        // dynamic "Planned #N" text instead — this fallback must not also fire, or the card
        // would show "Planned" twice.
        var card = new Prelude();
        CardModifier.AddModifier(card, new PlannedModifier());
        string description = "Gain 10 Block. Draw 2 cards.";

        MainFile.AppendPlannedFallback(card, ref description);

        Assert.Equal("Gain 10 Block. Draw 2 cards.", description);
    }
}
