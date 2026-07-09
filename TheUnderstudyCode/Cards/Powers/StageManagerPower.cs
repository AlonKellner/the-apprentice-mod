using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class StageManagerPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "Stage Manager",
        "When you end your turn without playing any card, play all [gold]Planned[/gold] cards.",
        "When you end your turn without playing any card, play all [gold]Planned[/gold] cards.");

    public static bool ShouldAutoPlayPlanned(int cardsPlayedThisTurn) => cardsPlayedThisTurn == 0;

    private class Data
    {
        public int CardsPlayedThisTurn;
    }

    protected override object InitInternalData() => new Data();

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature == Owner && !cardPlay.IsAutoPlay && cardPlay.IsLastInSeries)
            GetInternalData<Data>().CardsPlayedThisTurn++;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player == Owner.Player)
            GetInternalData<Data>().CardsPlayedThisTurn = 0;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner)) return;
        if (!ShouldAutoPlayPlanned(GetInternalData<Data>().CardsPlayedThisTurn)) return;

        var player = Owner.Player!;
        var combatState = Owner.CombatState!;
        var allCards = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCards);
        Log.Info($"StageManagerPower.AfterSideTurnEnd: no card played this turn — auto-playing {planned.Count} Planned slot(s)");
        Creature? currentTarget = null;

        foreach (var (card, _, slotSeqIdx) in planned)
        {
            if (card.Pile == null)
            {
                Log.Info($"StageManagerPower.AfterSideTurnEnd: {card.Id} is no longer in any pile — skipped");
                continue;
            }

            if (card.TryGetModifier<UnplayableModifier>(out var stillUnplayable))
                CardModifier.DirectModifiers(card).Remove(stillUnplayable);
            if (card.TryGetModifier<PlannedModifier>(out var stillPlanned) && stillPlanned.SequenceIndices.Contains(slotSeqIdx))
                PlannedModifier.RemoveSlot(card, slotSeqIdx, allCards);

            if (card.TargetType == TargetType.AnyEnemy && (currentTarget == null || currentTarget.IsDead))
            {
                var previousTarget = currentTarget;
                currentTarget = player.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
                Log.Info($"StageManagerPower.AfterSideTurnEnd: target {previousTarget?.LogName ?? "(none)"} is no longer available — " +
                          $"re-targeted to {currentTarget?.LogName ?? "(none)"}");
            }

            await CardCmd.AutoPlay(context, card, currentTarget, AutoPlayType.None, false, false);
        }
        PlannedModifier.InvokeChanged();
    }
}
