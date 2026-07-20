using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Buff mirror of base-game Doom (an execute). Doom kills the owner at its turn boundary if HP <= X; Undoom
// instead HEALS the owner up to X at the end of the OPPONENT's turn (a player-owned Undoom heals after being
// attacked). Owner-relative (side != Owner.Side), so it works on either side when Swap/Invert moves it.
// Standing Counter — does NOT remove itself, mirroring Doom. Invertible buff side of the Doom pair; Swappable.
public class UndoomPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Undoom",
        "At the end of the opponent's turn, if this creature has less than [blue]this much[/blue] HP, it heals up to that much. [gold]Invertible[/gold]. [gold]Swappable[/gold].",
        "At the end of the opponent's turn, if this creature has less than [blue]{Amount}[/blue] HP, it heals up to that much. [gold]Invertible[/gold]. [gold]Swappable[/gold].");

    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        // End of the opponent's turn (for a player owner, the enemy's turn), while alive and below X.
        if (side == Owner.Side || Owner.IsDead) return;
        if (Owner.CurrentHp < Amount)
        {
            Flash();
            await CreatureCmd.Heal(Owner, Amount - Owner.CurrentHp);
        }
    }
}
