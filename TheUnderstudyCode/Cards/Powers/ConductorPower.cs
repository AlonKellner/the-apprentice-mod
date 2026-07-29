using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Conductor: whenever the OWNER gains Vigor, echo that same amount to every ally. Single-stack marker —
// its own Amount is cosmetic; it echoes the gained delta, not a stack count.
public class ConductorPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // Held true across the whole echo loop so an ally who also has Conductor can't bounce Vigor back into an
    // infinite exchange (same guard shape as base BeaconOfHope and the mod's DoubleTimePower re-entrancy).
    private bool _echoing;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // Only my own positive Vigor gain, and not while I'm mid-echo.
        if (_echoing || power is not VigorPower || power.Owner != Owner || amount <= 0m) return;

        var allies = CombatState.GetTeammatesOf(Owner)
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != Owner)
            .ToList();
        if (allies.Count == 0) return;

        _echoing = true;
        try
        {
            Flash();
            foreach (var ally in allies)
                await PowerCmd.Apply<VigorPower>(choiceContext, ally, amount, Owner, cardSource, false);
        }
        finally { _echoing = false; }
    }
}
