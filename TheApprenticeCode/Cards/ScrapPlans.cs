using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ScrapPlans : ConstructedCardModel
{
    public const string CardId = "TheApprentice:ScrapPlans";

    public ScrapPlans() : base(1, CardType.Skill, CardRarity.Common, TargetType.None, true, false)
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var handPile = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (handPile == null) return;

        var planned = handPile.Cards
            .Where(c => c.TryGetModifier<PlannedModifier>(out _))
            .ToList();

        // Remove modifier before discarding so cards don't stay Planned in the discard pile
        foreach (var card in planned)
            if (card.TryGetModifier<PlannedModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);

        await CardCmd.DiscardAndDraw(context, planned, planned.Count);
    }
}
