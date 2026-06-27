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
using TheApprentice.TheApprenticeCode.Cards;

namespace TheApprentice.TheApprenticeCode.Cards.Powers;

public class TrueStrengthPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "True Strength",
        "At the end of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold] and up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold].",
        "At the end of your turn, convert up to 5 [gold]Weak[/gold] to [gold]Unweak[/gold], up to 5 [gold]Vulnerable[/gold] to [gold]Unvulnerable[/gold], and negative [gold]Strength[/gold] to positive.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;

        await EmotionalExpression.ConvertWeakToUnweak(context, Owner, 5);
        await EmotionalExpression.ConvertVulnerableToUnvulnerable(context, Owner, 5);

        if (Amount >= 2)
        {
            int strengthAmount = Owner.GetPowerAmount<StrengthPower>();
            if (strengthAmount < 0)
            {
                await PowerCmd.Apply<StrengthPower>(context, Owner, -strengthAmount * 2, Owner, null, false);
            }
        }
    }
}
