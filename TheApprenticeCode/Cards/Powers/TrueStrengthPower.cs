using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class TrueStrengthPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "True Strength",
        Amount >= 2
            ? "At the [i]start[/i] of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold] and up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold]."
            : "At the end of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold] and up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold].",
        Amount >= 2
            ? "At the [i]start[/i] of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold] and up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold]."
            : "At the end of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold] and up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold].");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (Amount < 2) return;
        if (player.Creature != Owner) return;
        await Convert(context);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (Amount >= 2) return;
        if (side != CombatSide.Player) return;
        await Convert(context);
    }

    private async Task Convert(PlayerChoiceContext context)
    {
        await EmotionalExpression.ConvertWeakToUnweak(context, Owner, 5);
        await EmotionalExpression.ConvertVulnerableToUnvulnerable(context, Owner, 5);
    }
}
