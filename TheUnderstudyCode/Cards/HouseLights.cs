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

    public HouseLights() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(10);
        WithTip(typeof(VulnerablePower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        var creature = cardPlay.Card.Owner.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        if (amount <= 0 || CombatState == null) return;
        foreach (var enemy in CombatState.HittableEnemies)
            await PowerCmd.Apply<VulnerablePower>(context, enemy, amount, creature, cardPlay.Card, false);
    }
}
