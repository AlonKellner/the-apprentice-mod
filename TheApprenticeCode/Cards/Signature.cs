using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Signature : ApprenticeCard
{
    public const string CardId = "TheApprentice:Signature";

    public override bool IsPrePlanned => true;

    public Signature() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(12);
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(6m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
