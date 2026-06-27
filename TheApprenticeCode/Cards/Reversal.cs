using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Reversal : ApprenticeCard
{
    public const string CardId = "TheApprentice:Reversal";

    public Reversal() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithTip(typeof(WeakPower));
        WithTip(typeof(UnweakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int cap = IsUpgraded ? 5 : 3;
        await EmotionalExpression.ConvertWeakToUnweak(context, creature, cap);
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
