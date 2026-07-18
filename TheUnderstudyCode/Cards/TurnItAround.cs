using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A full comeback swing: hit, flip your debuffs to buffs, and clear the whole jammed hand.
public class TurnItAround : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TurnItAround";

    public TurnItAround() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithVars(new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Invert);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.InvertEach(context, creature, (int)DynamicVars["Invert"].BaseValue);
        var handCards = PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.Where(c => c != this).ToList();
        foreach (var card in handCards.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
    }
}
