using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Everyone slows down — you included. Block, and Weak on the whole room. (Weak = Physical theme.)
public class DeadWeight : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DeadWeight";

    public DeadWeight() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(12);
        WithInvertibleTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ApplyWeakToSelf(context, creature, 1, this);
        var enemies = creature.CombatState!.HittableEnemies;
        await PowerCmd.Apply<WeakPower>(context, enemies, 1, creature, cardPlay.Card, false);
    }
}
