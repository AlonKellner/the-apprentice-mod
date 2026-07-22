using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Patches;

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
        // SecondLessonPower is Instanced, so this stands up a second Lesson with its own Orders
        // rather than stacking onto the first. Nothing is reset here on purpose: the new instance is
        // a fresh clone that has never tracked anything, and touching the existing Lesson would strip
        // the Orders it already has live on the board this turn.
        await CommonActions.Apply<SecondLessonPower>(context, cardPlay.Card.Owner.Creature, this);
        // Pre-compile the Order overlay shader off-screen now, a full turn before the earliest a
        // card could actually need it (Orders are assigned next turn's AfterPlayerTurnStartLate) —
        // see OrderOverlayPatch.Schedule for why the first-ever render of this shader stalls a frame.
        OrderOverlayPatch.Schedule();
    }
}
