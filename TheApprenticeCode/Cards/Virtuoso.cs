using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Virtuoso : ApprenticeCard
{
    public const string CardId = "TheApprentice:Virtuoso";

    public Virtuoso() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = (Creature)(object)cardPlay.Card.Owner;
        await PowerCmd.Apply(context, new VirtuosoPower(), creature, 1m, creature, cardPlay.Card, false);
    }
}
