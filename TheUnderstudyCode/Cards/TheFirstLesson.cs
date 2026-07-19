using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TheFirstLesson : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TheFirstLesson";

    public TheFirstLesson() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPowerNoTip<TheFirstLessonPower>(1);
        WithMarkedTip(typeof(WeakPower));
        WithMarkedTip(typeof(VulnerablePower));
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<TheFirstLessonPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
