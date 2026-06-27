using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Rehearsal : ApprenticeCard
{
    public const string CardId = "TheApprentice:Rehearsal";

    public Rehearsal() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        if (IsUpgraded)
        {
            await CommonActions.Draw(cardPlay.Card, context);
            await CommonActions.Draw(cardPlay.Card, context);
        }

        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        var targets = player.Piles
            .FirstOrDefault(p => p.Type == PileType.Hand)
            ?.Cards
            .Where(c => c != cardPlay.Card && PlannedModifier.CanApplyTo(c))
            .ToList();

        if (targets == null) return;
        foreach (var card in targets)
            PlannedModifier.Apply(card, allCards);
    }
}
