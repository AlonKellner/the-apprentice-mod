using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Swing: at the start of each of your turns, flip the polarity of every invertible pair you hold —
// debuffs become their buff form and buffs become their debuff form (see
// EmotionalExpression.InvertAllBidirectional). Persistent (unlike CenterStage's one-shot invert).
public class SwingPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Swing",
        "At the start of your turn, [gold]Invert[/gold] all invertible buffs and debuffs.",
        "At the start of your turn, [gold]Invert[/gold] all invertible buffs and debuffs.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        await EmotionalExpression.InvertAllBidirectional(context, Owner);
    }
}
