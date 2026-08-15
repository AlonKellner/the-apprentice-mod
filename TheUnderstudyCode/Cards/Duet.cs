using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A duet: gain Vigor, then lift your partner to your own level. You gain a flat Vigor first (so the card
// does something even from zero), then grant a teammate Vigor equal to your new total — a Vigor build
// carries the whole cast, not just its owner. (Vigor = Sounds theme.) The old "Best of Both for a target"
// Duet is now Pass the Mic.
public class Duet : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Duet";

    // Only obtainable/playable in co-op — it targets an ally.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // AnyAlly: a teammate other than yourself.
    public Duet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
        WithCostUpgradeBy(-1); // upgrade: cost 1 -> 0
        WithVars(new IntVar("Vigor", 4));
        WithMarkedTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var self = cardPlay.Card.Owner.Creature;
        // Gain the flat Vigor first, so "equal to yours" below includes it.
        await PowerCmd.Apply<VigorPower>(context, self, (int)DynamicVars["Vigor"].BaseValue, self, cardPlay.Card, false);

        // Reticle target on a manual play; a prompt here when an ordered resolver auto-plays this; a random
        // ally under Remix. Null only when there is no living ally.
        if (await AllyTargeting.ResolveTarget(context, cardPlay) is not { } target) return;
        int vigor = self.GetPowerAmount<VigorPower>();
        if (vigor > 0)
            await PowerCmd.Apply<VigorPower>(context, target, vigor, self, cardPlay.Card, false);
    }
}
