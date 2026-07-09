using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MissedCue : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MissedCue";

    public MissedCue() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        // Starts Tense 1 instead of Exhaust — "reset button" reads just as well as a one-off
        // that locks (and can be freed again, unlike Exhaust) as one that vanishes outright.
        // Upgrade is now a cost reduction (was previously "remove Exhaust", which no longer
        // applies) — the "always costs 2 Shaken" self-cost stays fixed, matching TableRead/Encore/
        // HeldNote's own Rare-Skill cost-reduction convention.
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Tense);
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(ShakenPower));
    }

    public override bool IsPreTense => true;

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(Owner.Piles.SelectMany(p => p.Cards).Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        foreach (var card in player.Piles.SelectMany(p => p.Cards).Where(c => c != this && UnplayableModifier.CanApplyTo(c)).ToList())
            UnplayableModifier.Remove(card);

        await EmotionalExpression.ApplyShakenToSelf(context, player.Creature, 2, this);
    }
}
