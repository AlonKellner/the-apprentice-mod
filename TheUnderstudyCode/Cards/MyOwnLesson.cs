using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MyOwnLesson : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MyOwnLesson";

    public MyOwnLesson() : base(0, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
        WithPower<MyOwnLessonPower>(1, 1);
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<MyOwnLessonPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
