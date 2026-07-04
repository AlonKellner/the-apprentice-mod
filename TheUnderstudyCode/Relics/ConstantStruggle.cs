using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

public class ConstantStruggle : UnderstudyStarterRelic
{
    protected override int SelectCount => 1;

    protected override LocString SelectionPrompt =>
        new("relics", "THEUNDERSTUDY-CONSTANT_STRUGGLE.selectionPrompt");

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<ConstantGrowth>();

    protected override async Task ApplyOnTurnOneSetup(PlayerChoiceContext ctx, Creature creature) =>
        await EmotionalExpression.ApplyWeakToSelf(ctx, creature, 1, null);
}
