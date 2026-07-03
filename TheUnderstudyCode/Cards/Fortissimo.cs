using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Fortissimo : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Fortissimo";

    public Fortissimo() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPower<FortissimoPower>(1, 1);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<FortissimoPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
