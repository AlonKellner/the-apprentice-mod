using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Commands;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Foresight : ApprenticeCard
{
    public const string CardId = "TheApprentice:Foresight";

    public Foresight() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var selected = await MultiPileCardSelect.Select(
            context, player,
            new CardSelectorPrefs(
                new LocString("cards", "THEAPPRENTICE-FORESIGHT.selectionPrompt"), 0, 1),
            c => PlannedModifier.CanApplyTo(c),
            PileType.Draw);

        if (selected != null)
        {
            var allCards = player.Piles.SelectMany(p => p.Cards);
            foreach (var card in selected)
                PlannedModifier.Apply(card, allCards);
        }

        int draws = IsUpgraded ? 2 : 1;
        for (int i = 0; i < draws; i++)
            await CommonActions.Draw(cardPlay.Card, context);
    }
}
