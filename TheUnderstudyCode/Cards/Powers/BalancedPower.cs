using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Random variant — applied by the unupgraded Standing By card. Each trigger frees Amount random
// eligible cards in hand. Stacks independently of BalancedChoicePower (the upgraded, player-choice
// variant), so a deck can run both and free some at random plus some by choice per trigger.
public class BalancedPower : BalancedPowerBase
{
    protected override string SelectionFragment => "random attack or skill";

    protected override Task<IReadOnlyList<CardModel>> SelectCards(
        PlayerChoiceContext context, Player player, IReadOnlyList<CardModel> candidates, int count)
    {
        var pool = candidates.ToList();
        var picked = new List<CardModel>(count);
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            var pick = player.RunState.Rng.CombatCardSelection.NextItem(pool);
            if (pick == null) break;
            picked.Add(pick);
            pool.Remove(pick);
        }
        return Task.FromResult<IReadOnlyList<CardModel>>(picked);
    }
}
