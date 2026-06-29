using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Improvise : ApprenticeCard
{
    public const string CardId = "TheApprentice:Improvise";

    public Improvise() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1);
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.Add);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        var planned = hand.Cards
            .Where(c => c != cardPlay.Card && c.TryGetModifier<PlannedModifier>(out _))
            .ToList();

        foreach (var card in planned)
            PlannedModifier.Remove(card, allCards);

        for (int i = 0; i < planned.Count; i++)
            await CommonActions.Draw(this, context);
    }
}
