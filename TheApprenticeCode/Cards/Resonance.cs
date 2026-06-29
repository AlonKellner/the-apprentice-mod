using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Resonance : ApprenticeCard
{
    public const string CardId = "TheApprentice:Resonance";

    public Resonance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        int plannedCount = hand.Cards
            .Where(c => c != cardPlay.Card && c.TryGetModifier<PlannedModifier>(out _))
            .Count();

        if (plannedCount > 0)
            await PlayerCmd.GainEnergy(plannedCount, player);
    }
}
