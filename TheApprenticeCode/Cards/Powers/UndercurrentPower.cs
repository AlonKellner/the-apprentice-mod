using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class UndercurrentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Undercurrent",
        "At the end of your turn, if you are [gold]Vulnerable[/gold], deal 8 damage to ALL enemies.",
        "At the end of your turn, if you are [gold]Vulnerable[/gold], deal 12 damage to ALL enemies.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;
        if (Owner.GetPowerAmount<VulnerablePower>() <= 0) return;

        decimal damage = Amount >= 2 ? 12m : 8m;
        await CreatureCmd.Damage(context, Owner.CombatState!.HittableEnemies, damage, ValueProp.Unpowered, Owner, null);
    }
}
