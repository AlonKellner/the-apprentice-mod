using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Pure Invert: nothing but the deck's signature debuff->buff flip. The designated (very rare)
// Invert 3 card alongside Center Stage.
public class LivingTheDream : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:LivingTheDream";

    public LivingTheDream() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new IntVar("Invert", 2));
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertEach(context, cardPlay.Card.Owner.Creature, invertAmount);
    }
}
