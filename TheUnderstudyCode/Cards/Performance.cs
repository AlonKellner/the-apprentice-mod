using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Performance : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Performance";

    public Performance() : base(0, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.None);
        WithVars(new CardsVar("Select", 2));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        // Step 1: Play all currently-Planned cards in queue order, consuming each slot.
        // A card with two slots is played twice.
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);
            await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.None, false, false);
        }
        PlannedModifier.InvokeChanged();

        // Step 2: Apply Planned to 0-N cards selected from hand (sets up next turn's queue).
        var maxSelect = IsUpgraded ? 3 : 2;
        var selectedRaw = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PERFORMANCE.selectionPrompt"), 0, maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c),
            this);

        if (selectedRaw == null) return;
        foreach (var card in selectedRaw)
            PlannedModifier.Apply(card, PlannedModifier.RelevantCards(player));

        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, cardPlay.Card, false);
    }
}
