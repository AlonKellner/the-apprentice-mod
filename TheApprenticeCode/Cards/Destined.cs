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

public class Destined : ApprenticeCard
{
    public const string CardId = "TheApprentice:Destined";

    public Destined() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(ApprenticeKeywords.Planned);
        WithDreamTips();
        WithAmbitionTips();
    }

    // Planned stacks: a card can occupy multiple queue slots, so re-applying to an already-Planned
    // token is intentional and supported.
    public static bool CanReceivePlanned(CardModel card) => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        var tokens = hand.Cards
            .Where(c => c != cardPlay.Card && (c is Dream || c is Ambition) && CanReceivePlanned(c))
            .ToList();

        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var token in tokens)
            PlannedModifier.Apply(token, allCards);
        await Task.CompletedTask;
    }
}
