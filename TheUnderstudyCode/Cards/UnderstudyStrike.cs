using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class UnderstudyStrike : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:UnderstudyStrike";

    public UnderstudyStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
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
