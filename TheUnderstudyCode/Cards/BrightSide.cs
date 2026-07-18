using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class BrightSide : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BrightSide";

    public BrightSide() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<BrightSidePower>(1);
        WithTip(UnderstudyKeywords.Invert);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<BrightSidePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
