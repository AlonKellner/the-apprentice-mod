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

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-TRANSPOSE.selectionPrompt"), 2, 2),
            c => c != cardPlay.Card,
            this);

        if (selected == null) return;
        var cards = selected.ToList();
        if (cards.Count < 2) return;

        var allCards = player.Piles.SelectMany(p => p.Cards);
        var a = cards[0];
        var b = cards[1];
        bool aPlanned = a.TryGetModifier<PlannedModifier>(out var modA);
        bool bPlanned = b.TryGetModifier<PlannedModifier>(out var modB);

        if (aPlanned && bPlanned)
        {
            var aSlots = modA!.SequenceIndices.ToList();
            modA.SequenceIndices.Clear();
            modA.SequenceIndices.AddRange(modB!.SequenceIndices);
            modB.SequenceIndices.Clear();
            modB.SequenceIndices.AddRange(aSlots);
            PlannedModifier.RefreshVisualIndices(allCards);
            PlannedModifier.InvokeChanged();
        }
        else if (aPlanned)
        {
            PlannedModifier.Remove(a, allCards);
            PlannedModifier.Apply(b, allCards);
        }
        else if (bPlanned)
        {
            PlannedModifier.Remove(b, allCards);
            PlannedModifier.Apply(a, allCards);
        }

        if (IsUpgraded)
            await CommonActions.Draw(this, context);
    }
}
