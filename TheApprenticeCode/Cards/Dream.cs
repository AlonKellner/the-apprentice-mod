using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TokenCardPool))]
public class Dream : ConstructedCardModel
{
    public const string CardId = "TheApprentice:Dream";
    public const int BaseBlock = 0;

    public Dream() : base(0, CardType.Skill, CardRarity.Token, TargetType.None, showInCardLibrary: false)
    {
        WithBlock(BaseBlock);
        WithKeyword(ApprenticeKeywords.Dreamy, ConstructedCardModel.UpgradeType.None);
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
    }
}
