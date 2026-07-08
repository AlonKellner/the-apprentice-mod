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
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(ShakenPower));
    }

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
