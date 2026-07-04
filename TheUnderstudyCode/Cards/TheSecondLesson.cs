using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TheSecondLesson : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TheSecondLesson";

    public TheSecondLesson() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.Add);
        WithPower<SecondLessonPower>(1);
        WithTip(typeof(RewardedPower));
        WithTip(typeof(PunishedPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int turn = cardPlay.Card.Owner.Creature.Player?.PlayerCombatState?.TurnNumber ?? -1;
        Log.Info($"TheSecondLesson[turn {turn}]: played, granting SecondLessonPower");
        await CommonActions.Apply<SecondLessonPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
