using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Granted 1 stack whenever an Order is obeyed (The Second Lesson). Never decreases or is removed
// on its own — the stacks accumulate and the full Amount is re-applied every turn as one
// priority-then-randomly-selected buff.
//
// That application lives here rather than on SecondLessonPower because this power is a singleton
// while SecondLessonPower is Instanced: several Lessons may feed this one counter, and resolving it
// from their hook would apply the accumulated buff once per Lesson. Owning it here makes "once per
// turn" fall out of there only ever being one Rewarded power, with no cross-instance coordination.
public class RewardedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Rewarded",
        "Every turn, apply this many of a random [gold]invertible[/gold] buff to yourself.",
        "Every turn, apply {Amount} of a random [gold]invertible[/gold] buff to yourself.");

    // Same hook SecondLessonPower assigns Orders in (post energy-refill and draw), so the buff still
    // lands at the point in the turn it always has.
    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player || Amount <= 0) return;

        var categories = EmotionalExpression.BuildCategories(Owner);
        var pair = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority,
            candidates => Owner.Player!.RunState.Rng.CombatCardSelection.NextItem(candidates)!);
        int turn = player.PlayerCombatState?.TurnNumber ?? -1;
        Log.Info($"RewardedPower[turn {turn}]: applying Reward buff {pair.Name} x{Amount}");
        await pair.ApplyBuffSide(context, Owner, Amount);
    }
}
