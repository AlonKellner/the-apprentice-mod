using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class ObsessionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Obsession",
        "At the end of your turn, gain {Amount} [gold]Block[/gold] per [gold]Planned[/gold] card.",
        "");

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext context, CombatSide side, System.Collections.Generic.IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;

        int plannedCount = PlannedModifier.CountIn(Owner.Player!.Piles.SelectMany(p => p.Cards));
        if (plannedCount == 0) return;

        await CreatureCmd.GainBlock(Owner, Amount * plannedCount, ValueProp.Unpowered, null, false);
    }
}
