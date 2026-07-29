using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Multiplayer Power: whenever an ally plays a card, gain Vigor. (Vigor = Sounds theme; the whole
// ensemble feeds your voice.)
public class Ensemble : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Ensemble";

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Ensemble() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPowerNoTip<EnsemblePower>(1, 1); // gain 1 Vigor per ally card; upgrade -> 2
        WithMarkedTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<EnsemblePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
