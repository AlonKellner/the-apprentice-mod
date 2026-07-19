using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Obtained by swapping in when the player takes Touch of Orobas — see UnderstudyStarterRelic and
// ConstantStruggle.GetUpgradeReplacement(). Deliberately does NOT override GetUpgradeReplacement
// itself: it defaults to null (no further chain), so a second Touch of Orobas falls back to the
// base game's own generic Circlet fallback, which is correct — there is no upgrade beyond this.
public class ConstantGrowth : UnderstudyStarterRelic
{
    protected override int SelectCount => 2;

    protected override LocString SelectionPrompt =>
        new("relics", "THEUNDERSTUDY-CONSTANT_GROWTH.selectionPrompt");

    // Show the Unweak keyword tooltip on hover (mirrors Constant Struggle's Weak tip).
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<UnweakPower>() };

    protected override async Task ApplyOnTurnOneSetup(PlayerChoiceContext ctx, Creature creature) =>
        await EmotionalExpression.ApplyUnweakToSelf(ctx, creature, 2, null);
}
