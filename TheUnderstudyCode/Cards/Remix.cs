using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Remix : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Remix";

    // Never targeted by the player — every card in the plan gets its own independently
    // randomized target below, so there's nothing for a player-picked target to feed into.
    public Remix() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        player.RunState.Rng.CombatCardSelection.Shuffle(planned);
        int expectedPlays = planned.Count;
        int actualPlays = 0;
        Log.Info($"Remix.OnPlay: playing {expectedPlays} Planned slot(s) in shuffled order, each independently retargeted");
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);
            // RemoveSlot only clears UnplayableModifier once ALL of a card's Planned slots are
            // gone, but a multi-slot card must still be playable on EACH of its own plays in this
            // loop — CardCmd.AutoPlay silently no-ops if the card still carries Unplayable.
            if (card.TryGetModifier<UnplayableModifier>(out var stillUnplayable))
                CardModifier.DirectModifiers(card).Remove(stillUnplayable);

            // Always pass null rather than reusing any target across cards: CardCmd.AutoPlay
            // itself rolls a fresh random living enemy for an AnyEnemy card whenever its target
            // argument is null, so this re-randomizes independently for every single card played,
            // not just when the previous target has died.
            await CardCmd.AutoPlay(context, card, null, AutoPlayType.None, false, false);
            actualPlays++;
        }
        Invariants.CheckEqual(expectedPlays, actualPlays, nameof(Remix) + "." + nameof(OnPlay),
            "Planned cards auto-played");
        PlannedModifier.InvokeChanged();
    }
}
