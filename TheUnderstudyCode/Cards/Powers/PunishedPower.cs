using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Granted 1 stack whenever an Order is disobeyed (The Second Lesson). Never decreases or is removed
// on its own — the mirror of RewardedPower, applying the accumulated Amount as one debuff each turn
// and owning that application for the same reason: it is a singleton fed by any number of Instanced
// Lessons, so resolving it here makes "once per turn" structural.
public class PunishedPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Punished",
        "Every turn, apply this many of a random [gold]invertible[/gold] debuff to yourself.",
        "Every turn, apply {Amount} of a random [gold]invertible[/gold] debuff to yourself.");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player || Amount <= 0) return;

        var categories = EmotionalExpression.BuildCategories(Owner);
        bool firstLessonActive = Owner.GetPowerAmount<TheFirstLessonPower>() > 0;
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(categories, firstLessonActive);
        var pair = EmotionalExpression.PickByPriority(filtered, EmotionalExpression.PunishPriority,
            candidates => Owner.Player!.RunState.Rng.CombatCardSelection.NextItem(candidates)!);
        int turn = player.PlayerCombatState?.TurnNumber ?? -1;
        Log.Info($"PunishedPower[turn {turn}]: applying Punish debuff {pair.Name} x{Amount}");
        await pair.ApplyDebuffSide(context, Owner, Amount);
    }
}
