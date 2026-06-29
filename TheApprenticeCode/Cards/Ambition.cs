using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TokenCardPool))]
public class Ambition : ConstructedCardModel
{
    public const string CardId = "TheApprentice:Ambition";
    public const int BaseDamage = 0;

    public Ambition() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, showInCardLibrary: false)
    {
        WithDamage(BaseDamage);
        WithKeyword(ApprenticeKeywords.Ambitous, ConstructedCardModel.UpgradeType.None);
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay.Target, 1).Execute(context);
    }
}
