using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TellItLikeItIs : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TellItLikeItIs";

    public TellItLikeItIs() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithVars(new EnergyVar(1));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithInvertibleTip(typeof(WeakPower));
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.SumOfInvertibleDebuffs(Owner.Creature) > 0;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);

        var creature = player.Creature;
        int amount = EmotionalExpression.SumOfInvertibleDebuffs(creature);
        await PowerCmd.Apply<WeakPower>(context, cardPlay.Target!, amount, creature, cardPlay.Card, false);
    }
}
