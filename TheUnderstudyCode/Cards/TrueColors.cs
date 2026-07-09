using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TrueColors : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TrueColors";

    public TrueColors() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new IntVar("Invert", 2));
        WithTip(UnderstudyKeywords.Invert);
        WithInvertibleTip(typeof(VulnerablePower));
        WithVar(new SelfDebuffVar("Vulnerable", 1));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertLastModified(context, creature, invertAmount);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, (int)DynamicVars["Vulnerable"].BaseValue, this);
    }
}
