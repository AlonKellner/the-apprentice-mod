using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Prodigy : ApprenticeCard
{
    public const string CardId = "TheApprentice:Prodigy";

    public Prodigy() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<ProdigyPower>(context, creature, 1m, creature, cardPlay.Card, false);
    }
}
