using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ClearMind : ApprenticeCard
{
    public const string CardId = "TheApprentice:ClearMind";

    public ClearMind() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var toExhaust = player.Piles
            .SelectMany(p => p.Cards)
            .Where(c => c != cardPlay.Card && IsUnplayable(c))
            .ToList();

        foreach (var card in toExhaust)
            await CardCmd.Exhaust(context, card, false, false);
    }

    static bool IsUnplayable(CardModel card)
    {
        var kw = new HashSet<CardKeyword>();
        foreach (var mod in CardModifier.DirectModifiers(card))
            mod.TryModifyKeywordsInCombat(card, kw);
        return kw.Contains(CardKeyword.Unplayable);
    }
}
