using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Afflictions;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "Obey my Orders. When you do, you'll be Rewarded. When you don't, you'll be Punished."
//
// Orders are assigned post-draw, in AfterPlayerTurnStartLate (after energy refill and draw have
// already happened — deliberately, so a fresh Reward/Punish debuff/buff never bites the turn it's
// granted, only the next one), from the cards actually drawn this turn (_drawnThisTurn, tracked via
// AfterCardDrawn). This used to be done pre-draw instead (peeking the draw pile in
// BeforeHandDrawLate before the turn's hand draw), to avoid the overlay popping in a frame late —
// but CardPileCmd.Draw can reshuffle the discard pile into the draw pile mid-draw, strictly after
// that peek, so it could under-count eligible cards and silently skip an Order for the turn
// (observed in-game: a turn with only PlayThis assigned, breaking the invariant that
// Rewarded+Punished always grows by exactly 2 per turn — see the even-sum Invariants.Check below).
// Post-draw assignment can't miss this way, since it only ever looks at cards that were actually
// drawn. The pop-in is instead smoothed over by OrderOverlayPatch fading the overlay in rather than
// snapping it to full opacity.
//
// Also in AfterPlayerTurnStartLate: resolve Reward, then Punish (in that fixed order, both from
// this one method, so the ordering can't depend on cross-power hook iteration), then assign this
// turn's new Orders to cards drawn this turn.
public class SecondLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "The Second Lesson",
        "Each turn, some cards will have an [red][sine]Order[/sine][/red]. Obeying grants [gold]Rewarded[/gold]; disobeying grants [gold]Punished[/gold].",
        "Each turn, some cards will have an [red][sine]Order[/sine][/red]. Obeying grants [gold]Rewarded[/gold]; disobeying grants [gold]Punished[/gold].");

    private readonly List<CardModel> _drawnThisTurn = new();

    // Cards this power currently has an active Order attached to, tracked by direct reference —
    // a played card leaves the Hand pile before BeforeSideTurnEnd fires, but the modifier/
    // affliction stay attached to the CardModel object regardless of which pile it's in.
    private readonly List<CardModel> _activeOrders = new();

    // Rewarded+Punished total observed at the previous turn start, so the per-turn growth can be
    // bounds-checked. Re-baselined in ResetTracking so a combat reload/replay (fresh powers at 0, or
    // a same-combat second play with powers already accumulated) never reads as a spurious jump.
    private int _lastResolvedTotal;

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (fromHandDraw && card.Owner == Owner.Player) _drawnThisTurn.Add(card);
        return Task.CompletedTask;
    }

    // Defensive reset, called every time TheSecondLesson is played (see TheSecondLesson.OnPlay).
    // PowerCmd.Apply<T> reuses an existing same-type Power instance for stacking
    // (FindExistingInstanceForStacking) rather than always constructing fresh — observed in-game: a
    // combat retried/reloaded after Orders had already been assigned in an abandoned prior attempt
    // left a leftover SecondLessonPower instance whose _activeOrders still referenced that attempt's
    // cards; playing the card again in the new attempt reused that same instance, and those stale
    // Orders immediately "resolved as unresolved" at the very next turn end. Clearing both tracking
    // lists AND stripping the OrderModifier/Order affliction they reference keeps a card from being
    // stuck showing an Order overlay that nothing would otherwise ever resolve or remove.
    public void ResetTracking()
    {
        foreach (var card in _activeOrders)
        {
            if (card.TryGetModifier<OrderModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod!);
            if (card.Affliction is Order)
                CardCmd.ClearAffliction(card);
        }
        _activeOrders.Clear();
        _drawnThisTurn.Clear();
        // Re-baseline the growth check to whatever the powers currently read (0 in a fresh combat, or
        // the already-accumulated total if the card is replayed mid-combat) so the next turn's delta
        // is measured from a truthful starting point rather than a stale one.
        _lastResolvedTotal = Owner.GetPowerAmount<RewardedPower>() + Owner.GetPowerAmount<PunishedPower>();
    }

    // Pure: given the ordered list of cards drawn this turn, which gets "Play this card" (the
    // first eligible one), which gets "Don't play this card" (the second eligible one), and what's
    // left over as candidates for the 25%-chance flavor Order.
    public static (CardModel? playThis, CardModel? dontPlayThis, IReadOnlyList<CardModel> remainingEligible)
        SelectFirstTwoEligible(IReadOnlyList<CardModel> drawnThisTurn)
    {
        var eligible = drawnThisTurn.Where(OrderModifier.CanApplyTo).ToList();
        CardModel? playThis = eligible.Count > 0 ? eligible[0] : null;
        CardModel? dontPlayThis = eligible.Count > 1 ? eligible[1] : null;
        var remaining = eligible.Count > 2 ? eligible.Skip(2).ToList() : new List<CardModel>();
        return (playThis, dontPlayThis, remaining);
    }

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;

        int turn = player.PlayerCombatState?.TurnNumber ?? -1;
        int rewarded = Owner.GetPowerAmount<RewardedPower>();
        int punished = Owner.GetPowerAmount<PunishedPower>();
        Log.Info($"SecondLessonPower[turn {turn}]: AfterPlayerTurnStartLate firing; " +
                  $"Rewarded={rewarded}, Punished={punished}, " +
                  $"drawnThisTurn={_drawnThisTurn.Count}, activeOrders={_activeOrders.Count}");
        Invariants.CheckEqual(_activeOrders.Count, _activeOrders.Count(c => c.TryGetModifier<OrderModifier>(out _)),
            nameof(SecondLessonPower) + "." + nameof(AfterPlayerTurnStartLate),
            "tracked active-Order cards vs. cards still actually carrying OrderModifier");
        // Each assigned Order grants exactly one Reward or Punish stack when it resolves (FlavorOnly
        // always resolves to Resolution.None, granting nothing), and Orders are the ONLY source of
        // those two powers. At most one PlayThis + one DontPlayThis are assigned per turn — and a turn
        // may assign fewer, or none, when the cards drawn hold under two eligible (non-Stable
        // Attack/Skill) cards. So between turn starts the total can only grow, by 0, 1, or 2.
        int resolvedTotal = rewarded + punished;
        int grew = resolvedTotal - _lastResolvedTotal;
        Invariants.Check(grew >= 0 && grew <= 2,
            nameof(SecondLessonPower) + "." + nameof(AfterPlayerTurnStartLate),
            $"Rewarded+Punished may grow by 0-2 per turn (≤2 Orders resolve) and never shrink — " +
            $"grew by {grew} (from {_lastResolvedTotal} to {resolvedTotal}); Rewarded={rewarded}, Punished={punished}");
        _lastResolvedTotal = resolvedTotal;

        await ResolveRewardTurn(context, turn);
        await ResolvePunishTurn(context, turn);
        await AssignOrders(context, player, turn);

        _drawnThisTurn.Clear();
    }

    private async Task ResolveRewardTurn(PlayerChoiceContext ctx, int turn)
    {
        var rewarded = Owner.GetPower<RewardedPower>();
        if (rewarded == null || rewarded.Amount <= 0) return;
        var categories = EmotionalExpression.BuildCategories(Owner);
        var pair = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, PickRandom);
        Log.Info($"SecondLessonPower[turn {turn}]: applying Reward buff {pair.Name} x{rewarded.Amount}");
        await pair.ApplyBuffSide(ctx, Owner, rewarded.Amount);
    }

    private async Task ResolvePunishTurn(PlayerChoiceContext ctx, int turn)
    {
        var punished = Owner.GetPower<PunishedPower>();
        if (punished == null || punished.Amount <= 0) return;
        var categories = EmotionalExpression.BuildCategories(Owner);
        bool firstLessonActive = Owner.GetPowerAmount<TheFirstLessonPower>() > 0;
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(categories, firstLessonActive);
        var pair = EmotionalExpression.PickByPriority(filtered, EmotionalExpression.PunishPriority, PickRandom);
        Log.Info($"SecondLessonPower[turn {turn}]: applying Punish debuff {pair.Name} x{punished.Amount}");
        await pair.ApplyDebuffSide(ctx, Owner, punished.Amount);
    }

    private InvertiblePair PickRandom(IReadOnlyList<InvertiblePair> candidates) =>
        Owner.Player!.RunState.Rng.CombatCardSelection.NextItem(candidates);

    private async Task AssignOrders(PlayerChoiceContext ctx, Player player, int turn)
    {
        var (playThis, dontPlayThis, remaining) = SelectFirstTwoEligible(_drawnThisTurn);

        if (playThis != null) await AttachOrder(playThis, OrderModifier.Kind.PlayThis, null, turn);
        if (dontPlayThis != null) await AttachOrder(dontPlayThis, OrderModifier.Kind.DontPlayThis, null, turn);

        if (remaining.Count > 0 && player.RunState.Rng.CombatCardSelection.NextInt(4) == 0)
        {
            var flavorCard = player.RunState.Rng.CombatCardSelection.NextItem(remaining)!;
            var flavorText = player.RunState.Rng.CombatCardSelection.NextItem(OrderModifier.FlavorLines)!;
            await AttachOrder(flavorCard, OrderModifier.Kind.FlavorOnly, flavorText, turn);
        }
    }

    private async Task AttachOrder(CardModel card, OrderModifier.Kind kind, string? flavorText, int turn)
    {
        CardModifier.AddModifier<OrderModifier>(card);
        if (card.TryGetModifier<OrderModifier>(out var mod))
        {
            mod!.OrderKind = kind;
            mod.FlavorText = flavorText;
        }
        await CardCmd.Afflict<Order>(card, 1);
        _activeOrders.Add(card);
        Log.Info($"SecondLessonPower[turn {turn}]: attached Order({kind}) to {card.Id}");
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;

        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        Invariants.CheckEqual(_activeOrders.Count, _activeOrders.Count(c => c.TryGetModifier<OrderModifier>(out _)),
            nameof(SecondLessonPower) + "." + nameof(BeforeSideTurnEnd),
            "tracked active-Order cards vs. cards still actually carrying OrderModifier");
        foreach (var card in _activeOrders)
        {
            if (card.TryGetModifier<OrderModifier>(out var mod) && !mod!.Resolved)
            {
                var resolution = OrderModifier.OnTurnEndIfUnresolved(mod.OrderKind);
                Log.Info($"SecondLessonPower[turn {turn}]: {card.Id}'s Order({mod.OrderKind}) went unresolved to turn end -> {resolution}");
                if (resolution == OrderModifier.Resolution.Reward)
                    await PowerCmd.Apply<RewardedPower>(context, Owner, 1, Owner, null, false);
                else if (resolution == OrderModifier.Resolution.Punish)
                    await PowerCmd.Apply<PunishedPower>(context, Owner, 1, Owner, null, false);
            }

            if (card.TryGetModifier<OrderModifier>(out var toRemove))
                CardModifier.DirectModifiers(card).Remove(toRemove!);
            if (card.Affliction is Order)
                CardCmd.ClearAffliction(card);
        }

        _activeOrders.Clear();
    }
}
