using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Ensemble: whenever a TEAMMATE plays a card, gain Amount Vigor. Only fires for
// another player on your side — never yourself (that would just be "whenever you play a card") and never
// enemies. A co-op Vigor engine that turns your partners' tempo into your fuel.
public class EnsemblePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var caster = cardPlay.Card.Owner?.Creature;
        if (caster == null || caster == Owner || !caster.IsPlayer || caster.Side != Owner.Side) return;
        Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null, false);
    }
}
