using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Multiplayer Power: conduct the whole cast — every Vigor you gain, the team gains too. Turns a solo
// Vigor engine into a team anthem. (Vigor = Sounds theme.)
public class Conductor : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Conductor";

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Conductor() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1); // upgrade: cost 1 -> 0
        WithPowerNoTip<ConductorPower>(1); // marker stack; the echo mirrors the gained amount, not this
        WithMarkedTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<ConductorPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
