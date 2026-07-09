using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class NervousEnergy : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:NervousEnergy";

    public NervousEnergy() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithVars(new EnergyVar(1));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithTip(UnderstudyKeywords.Tense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        var hand = PileType.Hand.GetPile(player);
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in hand.Cards.Where(c => c != this && TenseModifier.CanApplyTo(c)).ToList())
            TenseModifier.Apply(card, CombatState!, allCards);

        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
    }
}
