using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Conductor: at the end of your turn, every teammate (other players) gains Vigor equal to your current
// Vigor — you do not gain from your own Vigor. Reading your
// Vigor ONCE per turn (instead of reacting to each gain) makes it loop-proof — even if several players
// carry Conductor, there is no cross-player echo to spiral. Single-stack marker; its Amount is cosmetic.
public class ConductorPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;
        int vigor = Owner.GetPowerAmount<VigorPower>();
        if (vigor <= 0) return;   // only spread a positive Vigor total

        // Every OTHER player (teammates only — you don't gain from your own Vigor).
        var teammates = CombatState.PlayerCreatures.Where(c => (c?.IsAlive ?? false) && c != Owner).ToList();
        if (teammates.Count == 0) return;
        Flash();
        await PowerCmd.Apply<VigorPower>(context, teammates, vigor, Owner, null, false);
    }
}
