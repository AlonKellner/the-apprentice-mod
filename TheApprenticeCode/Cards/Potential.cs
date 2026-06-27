using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TokenCardPool))]
public class Potential : ConstructedCardModel
{
    public const string CardId = "TheApprentice:Potential";

    public Potential() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, showInCardLibrary: false)
    {
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
        WithCalculatedDamage(0, 2,
            static (card, _) => card.Owner?.Piles.SelectMany(p => p.Cards).Count(c => c is Ambition) ?? 0m,
            ValueProp.Move, 0, 2);
        WithCalculatedBlock("PotBlock", 0, 2,
            static (card, _) => card.Owner?.Piles.SelectMany(p => p.Cards).Count(c => c is Dream) ?? 0m,
            ValueProp.Move, 0, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (DynamicVars.CalculatedDamage.Calculate(cardPlay.Target) > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, 1).Execute(context);

        if (DynamicVars.TryGetValue("PotBlock", out DynamicVar potBlock))
            await CommonActions.CardBlock(this, potBlock, cardPlay);

        CardModifier.AddModifier<SpentModifier>(cardPlay.Card);
    }
}
