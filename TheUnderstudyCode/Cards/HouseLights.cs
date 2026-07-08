using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class HouseLights : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:HouseLights";

    public HouseLights() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(6);
        WithBlock(7);
        WithInvertibleTip(typeof(VulnerablePower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.SumOfInvertibleDebuffs(Owner.Creature) > 0;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.CardBlock(this, cardPlay);

        var creature = cardPlay.Card.Owner.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        if (amount <= 0 || CombatState == null) return;
        foreach (var enemy in CombatState.HittableEnemies)
            await PowerCmd.Apply<VulnerablePower>(context, enemy, amount, creature, cardPlay.Card, false);
    }
}
