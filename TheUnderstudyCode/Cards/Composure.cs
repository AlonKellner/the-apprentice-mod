using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Regain your composure: block, and clear every jammed card in hand. Remove-Unplayable is
// deliberately uncapped (whole hand).
public class Composure : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Composure";

    public Composure() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(8);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    // Glow only when there's actually something to free: an Unplayable ATTACK or SKILL in hand. Uses the
    // same predicate as OnPlay (UnplayableModifier.CanApplyTo), so Unplayable statuses/curses (e.g. Infection)
    // don't make it glow when it can't touch them.
    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        var handCards = PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.Where(c => c != this).ToList();
        foreach (var card in handCards.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
    }
}
