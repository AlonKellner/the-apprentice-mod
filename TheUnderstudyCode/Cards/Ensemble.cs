using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Ensemble : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Ensemble";

    public Ensemble() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(4);
        WithInvertibleTip(typeof(VulnerablePower));
        WithInvertibleTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 2, this);
        await EmotionalExpression.ApplyWeakToSelf(context, creature, 2, this);

        var enemies = creature.CombatState!.HittableEnemies;
        await PowerCmd.Apply<VulnerablePower>(context, enemies, 2, creature, cardPlay.Card, false);
        await PowerCmd.Apply<WeakPower>(context, enemies, 2, creature, cardPlay.Card, false);
    }
}
