using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class TensionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Tension",
        "At the end of your turn, take damage equal to [gold]Tension[/gold]. Then [gold]Tension[/gold] is removed.",
        "At the end of your turn, take damage equal to [gold]Tension[/gold]. Then [gold]Tension[/gold] is removed.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;

        if (Owner.GetPowerAmount<FortissimoPower>() > 0)
        {
            // Redirect all Tension damage to enemies; upgraded Fortissimo (Amount>=2) deals it twice.
            int hits = Owner.GetPowerAmount<FortissimoPower>() >= 2 ? 2 : 1;
            for (int i = 0; i < hits; i++)
                await CreatureCmd.Damage(context, Owner.CombatState!.HittableEnemies, Amount, ValueProp.Unpowered, Owner, null);
            await PowerCmd.Apply<TensionPower>(context, Owner, -Amount, Owner, null, false);
            return;
        }

        if (Owner.GetPowerAmount<DeceptiveCadencePower>() > 0)
        {
            // Upgraded Deceptive Cadence: grant 1 block per Tension carried over
            if (Owner.GetPowerAmount<DeceptiveCadencePower>() >= 2)
                await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
            // Carry over: skip damage, keep Tension stacks, consume the marker
            int dcAmount = (int)Owner.GetPowerAmount<DeceptiveCadencePower>();
            await PowerCmd.Apply<DeceptiveCadencePower>(context, Owner, -dcAmount, Owner, null, false);
            return;
        }

        // Default: self-damage (blockable), then remove all stacks
        await CreatureCmd.Damage(context, Owner, Amount, ValueProp.Unpowered, Owner, null);
        await PowerCmd.Apply<TensionPower>(context, Owner, -Amount, Owner, null, false);
    }
}
