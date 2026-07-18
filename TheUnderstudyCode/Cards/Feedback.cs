using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Loud and two-edged: strike and shield, paid for in Vigor. (Vigor = Sounds theme.)
public class Feedback : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Feedback";

    public Feedback() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(9);
        WithBlock(9);
        WithTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.CardBlock(this, cardPlay);
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, -3, creature, this, false);
    }
}
