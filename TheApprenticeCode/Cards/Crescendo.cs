using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Crescendo : ApprenticeCard
{
    public const string CardId = "TheApprentice:Crescendo";

    public Crescendo() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(4);
        WithCostUpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        var tokens = hand?.Cards
            .Where(c => c != cardPlay.Card && (c is Dream || c is Ambition))
            .ToList();

        if (tokens == null || tokens.Count == 0) return;

        foreach (var card in tokens)
            await CardCmd.Exhaust(context, card, false, false);

        await CommonActions.CardAttack(cardPlay.Card, cardPlay, tokens.Count).Execute(context);
    }
}
