using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class LoosenUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:LoosenUp";

    public LoosenUp() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(5);
        // Live "(Hits N times)" preview: N = number of Unplayable cards in hand (each drives one hit).
        // CalculationBase 0 + CalculationExtra 1 * count. Mirrors base-game Flechettes.
        WithVars(
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m),
            new CalculatedVar("CalculatedHits").WithMultiplier(static (card, _) => UnplayableHandCount(card)));
        WithTip(CardKeyword.Unplayable);
    }

    // Unplayable cards in hand (excluding this card) — the number of hits this deals. Shared by the live
    // preview var above and OnPlay so the shown count and the real hit count can't drift. Static so the
    // CalculatedVar multiplier delegate captures no instance.
    private static int UnplayableHandCount(CardModel card) =>
        PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card && c.IsUnplayable());

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override bool ShouldGlowGoldInternal =>
        CardExtensions.AnyUnplayable(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var handCards = PileType.Hand.GetPile(player).Cards.Where(c => c != this).ToList();

        // Damage counts every Unplayable card in hand, regardless of type — but only attacks and
        // skills actually have their Unplayable removed (Planned/Tuned/Free's usual scope). The count
        // comes from the same static helper the preview var uses, so text and effect always agree.
        int unplayableCount = UnplayableHandCount(cardPlay.Card);
        var toFree = handCards.Where(UnplayableModifier.CanApplyTo).ToList();
        Invariants.Check(toFree.Count <= unplayableCount, nameof(LoosenUp) + "." + nameof(OnPlay),
            $"freeing {toFree.Count} card(s), more than the {unplayableCount} counted for damage — toFree must be a subset");
        foreach (var card in toFree)
            UnplayableModifier.Remove(card);

        if (unplayableCount == 0) return;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, unplayableCount).Execute(context);
    }
}
