using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StandingRoom : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StandingRoom";

    public StandingRoom() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(6);
        WithTip(typeof(WeakPower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var creature = cardPlay.Card.Owner.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        if (amount <= 0 || CombatState == null) return;
        foreach (var enemy in CombatState.HittableEnemies)
            await PowerCmd.Apply<WeakPower>(context, enemy, amount, creature, cardPlay.Card, false);
    }
}
