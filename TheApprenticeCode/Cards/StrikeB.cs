using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class StrikeB : ApprenticeCardB
{
    public const string CardId = "TheApprentice:StrikeB";

    public StrikeB() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithDamage(6);
        WithTags(CardTag.Strike);
        WithIntenseTip();
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
