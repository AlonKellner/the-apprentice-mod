using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Let loose: three quick hits, then flip a debuff. (Invert = Self / Positive / Fun theme.)
public class Freestyle : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Freestyle";

    public Freestyle() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(3);
        WithVars(new IntVar("Invert", 1));
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
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, 3).Execute(context);
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertEach(context, cardPlay.Card.Owner.Creature, invertAmount);
    }
}
