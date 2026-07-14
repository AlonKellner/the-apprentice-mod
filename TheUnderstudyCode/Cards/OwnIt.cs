using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OwnIt : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OwnIt";

    protected override bool HasEnergyCostX => true;

    public OwnIt() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        int invertAmount = IsUpgraded ? 2 : 1;
        await EmotionalExpression.InvertEachWithBonus(context, cardPlay.Card.Owner.Creature, invertAmount, x);
    }
}
