using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FinalBar : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FinalBar";

    public FinalBar() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVars(
            new CalculationBaseVar(0m),
            new ExtraDamageVar(5m).WithUpgrade(2m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
                PlannedModifier.CountIn(PlannedModifier.RelevantCards(card.Owner))));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var plannedCards = allCardsList.Where(c => c.TryGetModifier<PlannedModifier>(out _)).ToList();
        foreach (var card in plannedCards)
        {
            PlannedModifier.Remove(card, allCardsList);
            if (card.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard)
                await CardCmd.Exhaust(context, card);
        }
    }
}
