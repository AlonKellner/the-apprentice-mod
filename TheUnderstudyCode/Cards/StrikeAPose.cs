using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Strike a pose: three quick hits at random enemies, then flip a debuff. Random multi-hit sets it
// apart from Comeback (its single-target twin). (Invert = Self / Positive / Fun theme.)
public class StrikeAPose : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StrikeAPose";

    private const int Hits = 3;

    public StrikeAPose() : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
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
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, Hits).Execute(context);
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertEach(context, cardPlay.Card.Owner.Creature, invertAmount);
    }
}
