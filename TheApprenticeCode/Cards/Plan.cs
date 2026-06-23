using BaseLib.Abstracts;
using BaseLib.Extensions;
using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Plan : ApprenticeCard
{
    public const string CardId = "TheApprentice:Plan";

    public Plan() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = IsUpgraded ? 2 : 1;

        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-PLAN.selectionPrompt"), 1, maxSelect),
            c => c != this && !c.TryGetModifier<PlannedModifier>(out _),
            this);

        if (selected == null) return;
        foreach (var card in selected)
        {
            int nextIndex = player.Piles.SelectMany(p => p.Cards).Count(c => c.TryGetModifier<PlannedModifier>(out _));
            CardModifier.AddModifier<PlannedModifier>(card);
            if (card.TryGetModifier<PlannedModifier>(out var mod))
                mod.SequenceIndex = nextIndex;
        }
    }
}
