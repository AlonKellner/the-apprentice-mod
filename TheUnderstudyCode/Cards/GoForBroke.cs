using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// All or nothing: unjam your ENTIRE deck, at the cost of Shaken. (Shaken self-debuff downside.)
public class GoForBroke : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:GoForBroke";

    public GoForBroke() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
        WithInvertibleTip(typeof(ShakenPower));
        WithVar(new SelfDebuffVar("Shaken", 2));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var deck = player.Piles.SelectMany(p => p.Cards).Where(c => c != this).ToList();
        foreach (var card in deck.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
        await EmotionalExpression.ApplyShakenToSelf(context, player.Creature, (int)DynamicVars["Shaken"].BaseValue, this);
    }
}
