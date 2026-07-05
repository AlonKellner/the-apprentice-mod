using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class EverythingIveGot : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:EverythingIveGot";

    protected override bool HasEnergyCostX => true;

    public EverythingIveGot() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Invert);
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        int invertAmount = IsUpgraded ? 2 : 1;
        await EmotionalExpression.InvertLastModifiedWithBonus(context, cardPlay.Card.Owner.Creature, invertAmount, x);
    }
}
