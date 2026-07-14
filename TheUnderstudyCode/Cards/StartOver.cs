using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StartOver : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StartOver";

    public StartOver() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        // Starts Tuned 1 instead of Exhaust — "reset button" reads just as well as a one-off
        // that locks (and can be freed again, unlike Exhaust) as one that vanishes outright.
        // Upgrade is now a cost reduction (was previously "remove Exhaust", which no longer
        // applies) — the "always costs 2 Shaken" self-cost stays fixed, matching Playlist/DaCapo/
        // HeldNote's own Rare-Skill cost-reduction convention.
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(ShakenPower));
        WithVar(new SelfDebuffVar("Shaken", 2));
    }

    public override bool IsPreTuned => true;

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(Owner.Piles.SelectMany(p => p.Cards).Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        foreach (var card in player.Piles.SelectMany(p => p.Cards).Where(c => c != this && UnplayableModifier.CanApplyTo(c)).ToList())
            UnplayableModifier.Remove(card);

        await EmotionalExpression.ApplyShakenToSelf(context, player.Creature, (int)DynamicVars["Shaken"].BaseValue, this);
    }
}
