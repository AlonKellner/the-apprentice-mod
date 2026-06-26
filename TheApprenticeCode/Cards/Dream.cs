using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TokenCardPool))]
public class Dream : ConstructedCardModel
{
    public const string CardId = "TheApprentice:Dream";

    public Dream() : base(0, CardType.Skill, CardRarity.Token, TargetType.None, showInCardLibrary: false)
    {
        WithCalculatedBlock(0, 1,
            static (card, _) => card.Owner?.Piles.SelectMany(p => p.Cards).Count(c => c is Dream) ?? 0m,
            ValueProp.Move, 0, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        CardModifier.AddModifier<SpentModifier>(cardPlay.Card);
    }
}
