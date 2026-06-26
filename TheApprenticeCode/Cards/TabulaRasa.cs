using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class TabulaRasa : ApprenticeCard
{
    public const string CardId = "TheApprentice:TabulaRasa";

    public TabulaRasa() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        foreach (var card in player.Piles.SelectMany(p => p.Cards).ToList())
            if (card.TryGetModifier<PlannedModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);
        PlannedModifier.InvokeChanged();

        if (IsUpgraded)
            await CommonActions.Draw(cardPlay.Card, context);
    }
}
