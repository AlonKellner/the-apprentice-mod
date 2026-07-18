using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Improvise : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Improvise";

    public Improvise() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Planned);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCards = PlannedModifier.RelevantCards(player).ToList();
        var plannedCards = allCards.Where(c => c != this && c.TryGetModifier<PlannedModifier>(out _)).ToList();

        if (plannedCards.Count > 0)
            await CardPileCmd.Draw(context, plannedCards.Count, player);

        // Keep the cards Planned but make them playable now by stripping Unplayable, so they can be
        // played this turn and still resolve from the Planned queue.
        foreach (var card in plannedCards)
            UnplayableModifier.Remove(card);
    }
}
