using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Draft : ApprenticeCard
{
    public const string CardId = "TheApprentice:Draft";

    public Draft() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        int draws = IsUpgraded ? 2 : 1;
        var before = new HashSet<CardModel>(hand.Cards);

        for (int i = 0; i < draws; i++)
            await CommonActions.Draw(cardPlay.Card, context);

        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var drawnCard in hand.Cards.Where(c => !before.Contains(c)).ToList())
            PlannedModifier.Apply(drawnCard, allCards);
    }
}
