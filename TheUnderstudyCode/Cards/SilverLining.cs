using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Gain Block, then flip a debuff into its buff. (Invert = Self / Positive / Fun theme.)
public class SilverLining : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SilverLining";

    public SilverLining() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(8);
        WithVars(new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertEach(context, cardPlay.Card.Owner.Creature, invertAmount);
    }
}
