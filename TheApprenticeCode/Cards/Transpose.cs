using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Transpose : ApprenticeCard
{
    public const string CardId = "TheApprentice:Transpose";

    public Transpose() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        // Step 1: choose a Planned card to un-plan
        var toRemove = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-TRANSPOSE.selectionPrompt1"), 0, 1),
            c => c.TryGetModifier<PlannedModifier>(out _),
            this);

        var unplanned = toRemove?.FirstOrDefault();
        if (unplanned == null) return;

        if (unplanned.TryGetModifier<PlannedModifier>(out var mod))
            CardModifier.DirectModifiers(unplanned).Remove(mod);

        // Step 2: choose a non-Planned card to make Planned
        var toAdd = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-TRANSPOSE.selectionPrompt2"), 0, 1),
            c => c != cardPlay.Card && PlannedModifier.CanApplyTo(c),
            this);

        var target = toAdd?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));

        if (IsUpgraded)
            await CommonActions.Draw(cardPlay.Card, context);
    }
}
