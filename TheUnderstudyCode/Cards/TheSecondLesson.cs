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
        var power = await CommonActions.Apply<SecondLessonPower>(context, cardPlay.Card.Owner.Creature, this);
        // PowerCmd.Apply reuses an existing same-type instance for stacking rather than always
        // constructing fresh — see SecondLessonPower.ResetTracking for why playing this card must
        // always start its Order-tracking state clean regardless of which instance came back.
        power?.ResetTracking();
        // Pre-compile the Order overlay shader off-screen now, a full turn before the earliest a
        // card could actually need it (Orders are assigned next turn's AfterPlayerTurnStartLate) —
        // see OrderOverlayPatch.Schedule for why the first-ever render of this shader stalls a frame.
        OrderOverlayPatch.Schedule();
    }
}
