using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Bounce back from a jam: two quick hits, then clear every Unplayable in hand. Remove-Unplayable is
// deliberately uncapped (whole hand).
public class Comeback : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Comeback";

    public Comeback() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(3);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        CardExtensions.AnyUnplayable(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, 2).Execute(context);
        var handCards = PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.Where(c => c != this).ToList();
        foreach (var card in handCards.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
    }
}
