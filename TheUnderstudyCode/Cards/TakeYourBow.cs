using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TakeYourBow : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TakeYourBow";

    public TakeYourBow() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        WithDamage(5);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var handCards = PileType.Hand.GetPile(player).Cards.Where(c => c != this).ToList();

        // Damage counts every Unplayable card in hand, regardless of type — but only attacks and
        // skills actually have their Unplayable removed (Planned/Intense/Free's usual scope).
        int unplayableCount = handCards.Count(c => c.IsUnplayable());
        var toFree = handCards.Where(UnplayableModifier.CanApplyTo).ToList();
        Invariants.Check(toFree.Count <= unplayableCount, nameof(TakeYourBow) + "." + nameof(OnPlay),
            $"freeing {toFree.Count} card(s), more than the {unplayableCount} counted for damage — toFree must be a subset");
        foreach (var card in toFree)
            UnplayableModifier.Remove(card);

        if (unplayableCount == 0) return;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, unplayableCount).Execute(context);
    }
}
