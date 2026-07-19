using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A cheap idea that arrives pre-tuned. (Tuned = Preparations theme.)
public class ShowerThought : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:ShowerThought";

    public ShowerThought() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(2);
        WithCards(1);
        WithTip(UnderstudyKeywords.Tuned);
    }

    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.Draw(this, context);
    }
}
