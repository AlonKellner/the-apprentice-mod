using System.Linq;
using System.Runtime.CompilerServices;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
        var creature = cardPlay.Card.Owner.Creature;
        int turn = creature.Player?.PlayerCombatState?.TurnNumber ?? -1;
        // SecondLessonPower is Instanced, so this stands up a second Lesson with its own Orders
        // rather than stacking onto the first. Nothing is reset here on purpose: the new instance is
        // a fresh clone that has never tracked anything, and touching the existing Lesson would strip
        // the Orders it already has live on the board this turn.
        //
        // The before/after rosters and the granted id are the decisive reading for whether a second
        // play really mints a second power object. PowerCmd.Apply<T> is supposed to hand back a fresh
        // ToMutable() clone for an Instanced power, but it is reached through BaseLib's reflection
        // shim, and Creature.ApplyPowerInternal appends without an identity check — so if `granted`
        // repeats an id already in `before`, or `after` lists one id twice, the same object is being
        // registered twice and every hook it receives is doubled.
        string before = Roster(creature);
        var power = await CommonActions.Apply<SecondLessonPower>(context, creature, this);
        Log.Info($"TheSecondLesson[turn {turn}]: granted SecondLessonPower " +
                  $"id={(power == null ? "null" : RuntimeHelpers.GetHashCode(power).ToString())}; " +
                  $"lessons before=[{before}] after=[{Roster(creature)}]");
        // Pre-compile the Order overlay shader off-screen now, a full turn before the earliest a
        // card could actually need it (Orders are assigned next turn's AfterPlayerTurnStartLate) —
        // see OrderOverlayPatch.Schedule for why the first-ever render of this shader stalls a frame.
        OrderOverlayPatch.Schedule();
    }

    private static string Roster(Creature creature) =>
        string.Join(",", creature.GetPowerInstances<SecondLessonPower>().Select(RuntimeHelpers.GetHashCode));
}
