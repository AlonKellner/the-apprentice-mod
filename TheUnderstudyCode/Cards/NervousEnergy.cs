using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Nervy tempo: strike, refund energy, and leave yourself exposed. (Vulnerable = Emotional theme.)
public class NervousEnergy : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:NervousEnergy";

    public NervousEnergy() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(6);
        WithVars(new EnergyVar(2));
        WithCostUpgradeBy(-1);
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithInvertibleTip(typeof(VulnerablePower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
        await EmotionalExpression.ApplyVulnerableToSelf(context, player.Creature, 2, this);
    }
}
