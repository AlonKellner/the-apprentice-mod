using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A duet: lift your partner to your own level. Grant a teammate Vigor equal to yours — so a Vigor build
// carries the whole cast, not just its owner. (Vigor = Sounds theme.) The old "Best of Both for a target"
// Duet is now Trading Fours.
public class Duet : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Duet";

    // Only obtainable/playable in co-op — it targets an ally.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // AnyAlly: a teammate other than yourself (giving yourself Vigor equal to your own is a no-op).
    public Duet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
        WithCostUpgradeBy(-1); // upgrade: cost 1 -> 0
        WithMarkedTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target) return;
        var self = cardPlay.Card.Owner.Creature;
        int vigor = self.GetPowerAmount<VigorPower>();
        if (vigor > 0)
            await PowerCmd.Apply<VigorPower>(context, target, vigor, self, cardPlay.Card, false);
    }
}
