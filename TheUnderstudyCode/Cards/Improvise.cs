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

    public Improvise() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCards = PlannedModifier.RelevantCards(player).ToList();
        var plannedCards = allCards.Where(c => c != this && c.TryGetModifier<PlannedModifier>(out _)).ToList();

        if (plannedCards.Count > 0)
            await CardPileCmd.Draw(context, plannedCards.Count, player);

        // PlannedModifier.Remove already strips UnplayableModifier as part of clearing the last
        // slot, so "remove Unplayable, then remove Planned" happens for free in the right order.
        foreach (var card in plannedCards)
            PlannedModifier.Remove(card, allCards);
    }
}
