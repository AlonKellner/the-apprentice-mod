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
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Performance : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Performance";

    public Performance() : base(0, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithVars(new CardsVar("Select", 2));
        WithTip(UnderstudyKeywords.Planned);
    }

    // Only require a target when the queue actually has a card that needs one — an empty plan,
    // or a plan of only AoE/self/no-target cards, should just play with no reticle. Owner throws
    // on a canonical (not-yet-instantiated) card model, so fall back to the constructor-seeded
    // value there (bare-construction tests, card library previews, etc).
    public override TargetType TargetType =>
        IsMutable
            ? (PlannedModifier.QueueNeedsEnemyTarget(PlannedModifier.RelevantCards(Owner)) ? TargetType.AnyEnemy : TargetType.None)
            : base.TargetType;

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
        int expectedPlays = planned.Count;
        int actualPlays = 0;
        Log.Info($"Performance.OnPlay: playing {expectedPlays} Planned slot(s)");
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);
            await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.None, false, false);
            actualPlays++;
        }
        Invariants.CheckEqual(expectedPlays, actualPlays, nameof(Performance) + "." + nameof(OnPlay),
            "Planned cards auto-played");
        PlannedModifier.InvokeChanged();

        // Step 2: Apply Planned to 0-N cards selected from the discard pile (sets up next turn's queue).
        var maxSelect = IsUpgraded ? 3 : 2;
        var selectedRaw = await CardSelectCmd.FromCombatPile(
            context,
            PileType.Discard.GetPile(player),
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PERFORMANCE.selectionPrompt"), 0, maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c));

        if (selectedRaw == null) return;
        foreach (var card in selectedRaw)
            PlannedModifier.Apply(card, PlannedModifier.RelevantCards(player));

        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, cardPlay.Card, false);
    }
}
