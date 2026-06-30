using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Blueprint : ApprenticeCard
{
    public const string CardId = "TheApprentice:Blueprint";

    public Blueprint() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
        WithDreamKeywordTips();
        WithAmbitionKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        var toExhaust = hand.Cards
            .Where(c => c != cardPlay.Card && c.IsUnplayable())
            .ToList();

        foreach (var card in toExhaust)
            await CardCmd.Exhaust(context, card, false, false);

        int count = toExhaust.Count;
        if (count <= 0) return;

        var dreams = new List<CardModel>(count);
        var ambitions = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            dreams.Add(CombatState!.CreateCard<Dream>(player));
            ambitions.Add(CombatState!.CreateCard<Ambition>(player));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(dreams, PileType.Draw, player);
        await CardPileCmd.AddGeneratedCardsToCombat(ambitions, PileType.Draw, player);
    }
}
