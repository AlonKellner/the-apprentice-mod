using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class BareItAll : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BareItAll";

    public BareItAll() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(15);
        WithTip(typeof(VulnerablePower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var creature = cardPlay.Card.Owner.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        await PowerCmd.Apply<VulnerablePower>(context, cardPlay.Target!, amount, creature, cardPlay.Card, false);
    }
}
