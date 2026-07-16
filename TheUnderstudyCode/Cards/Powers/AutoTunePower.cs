using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "At the start of your turn, increase all Tuned by {Amount}." The per-turn pump raises each
// already-Tuned card's stacks by this Power's Amount (Counter, so replaying the card stacks the
// amount and shows the count).
public class AutoTunePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Auto Tune",
        "At the start of your turn, increase all [gold]Tuned[/gold] by {Amount}.",
        "At the start of your turn, increase all [gold]Tuned[/gold] by {Amount}.");

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return Task.CompletedTask;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var card in allCards.Where(c => c.TryGetModifier<TunedModifier>(out _)).ToList())
            for (int i = 0; i < (int)Amount; i++)
                TunedModifier.Apply(card, Owner.CombatState!, allCards);
        return Task.CompletedTask;
    }
}
