using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "Cards without Tuned cannot become Tuned. At the start of your turn, apply Tuned to all Tuned
// cards." The "cannot become Tuned" clause is enforced centrally in TunedModifier.Apply (the one
// point every first-time Tuned application funnels through), which checks IsActive below. This
// Power supplies that presence flag plus the per-turn pump.
public class AutoTunePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Auto Tune",
        "Cards without [gold]Tuned[/gold] cannot become [gold]Tuned[/gold]. At the start of your turn, apply [gold]Tuned[/gold] to all [gold]Tuned[/gold] cards.",
        "Cards without [gold]Tuned[/gold] cannot become [gold]Tuned[/gold]. At the start of your turn, apply [gold]Tuned[/gold] to all [gold]Tuned[/gold] cards.");

    public static bool IsActive(Creature? creature) => creature?.GetPower<AutoTunePower>() != null;

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var card in allCards.Where(c => c.TryGetModifier<TunedModifier>(out _)).ToList())
            TunedModifier.Apply(card, Owner.CombatState!, allCards);
        return Task.CompletedTask;
    }
}
