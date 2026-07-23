using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
// granted, only the next one), from the cards actually drawn this turn (DrawnThisTurn, tracked via
// AfterCardDrawn). This used to be done pre-draw instead (peeking the draw pile in
// BeforeHandDrawLate before the turn's hand draw), to avoid the overlay popping in a frame late —
// but CardPileCmd.Draw can reshuffle the discard pile into the draw pile mid-draw, strictly after
// that peek, so it could under-count eligible cards and silently skip an Order for the turn.
// Post-draw assignment can't miss this way, since it only ever looks at cards that were actually
// drawn. The pop-in is instead smoothed over by OrderOverlayPatch fading the overlay in rather than
// snapping it to full opacity. Assigning fewer than two Orders is legitimate regardless of that
// history — a turn simply may not draw two eligible cards — so the growth check below bounds how far
// Rewarded+Punished can move per turn instead of demanding a fixed step.
//
// The power is Instanced: two plays mean two Lessons side by side, each with its own drawn-card
// tracking and its own pair of Orders per turn (Orders never overlap: OrderModifier.CanApplyTo skips
// any card already carrying an Order or affliction, and SelectFirstTwoEligible picks two cards that
// are distinct by reference). The Rewarded/Punished counters they feed are singletons that
// apply themselves on their own turn-start hook, so their once-per-turn application needs no
// coordination between Lessons no matter how many are live.
public class SecondLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // Two plays of The Second Lesson are two separate Lessons, not one louder one: each instance
    // keeps its own DrawnThisTurn/ActiveOrders and hands out its own pair of Orders every turn.
    // PowerCmd.FindExistingInstanceForStacking returns null for Instanced, so every application
    // constructs a fresh instance instead of stacking Amount onto the existing one (same mechanism
    // the base game's TheBombPower uses to keep each bomb ticking down separately).
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;


    // ── Per-instance state ───────────────────────────────────────────────────────────────────
    // This MUST live behind InitInternalData/GetInternalData rather than in ordinary fields, and the
    // reason is not stylistic. A power reaches combat via ToMutable() -> MutableClone(), which is a
    // MemberwiseClone: reference-type fields are copied BY REFERENCE, so every clone would go on
    // sharing one List with the canonical model and with each other. PowerModel.DeepCloneFields is
    // the only thing that re-isolates state, and all it re-creates is _dynamicVars and _internalData
    // ("isolated to this instance of the power... reset on clones of this power").
    //
    // Sharing is exactly what went wrong before: two Lessons appended to a single DrawnThisTurn, so
    // a 10-card hand recorded 20 draws with every card listed twice, one card was handed both Orders,
    // whichever Lesson ran first cleared the list out from under the other, and clearing the shared
    // ActiveOrders stripped Orders the "other" Lesson had live on the board.
    //
    // Base-game OrbitPower is the reference implementation of this pattern.
    private sealed class Data
    {
        // Cards drawn this turn, the pool Orders are assigned from.
        public readonly List<CardModel> DrawnThisTurn = new();

        // Cards this power currently has an active Order attached to, tracked by direct reference —
        // a played card leaves the Hand pile before BeforeSideTurnEnd fires, but the modifier/
        // affliction stay attached to the CardModel object regardless of which pile it's in.
        public readonly List<CardModel> ActiveOrders = new();

        // Shared Rewarded+Punished total observed at the previous turn start, so the per-turn growth
        // can be bounds-checked. Null until this instance has seen one turn start: a Lesson played on
        // turn 5 into an already-accumulated total must baseline against what it finds rather than
        // against zero, or its very first reading would look like one huge illegal jump.
        public int? LastResolvedTotal;
    }

    protected override object InitInternalData() => new Data();

    private List<CardModel> DrawnThisTurn => GetInternalData<Data>().DrawnThisTurn;
    private List<CardModel> ActiveOrders => GetInternalData<Data>().ActiveOrders;

    private int? LastResolvedTotal
    {
        get => GetInternalData<Data>().LastResolvedTotal;
        set => GetInternalData<Data>().LastResolvedTotal = value;
    }

    // ── Diagnostics ──────────────────────────────────────────────────────────────────────────
    // Identity of this specific power object, carried so the per-instance isolation above stays
    // observable rather than assumed. Each live Lesson should report a distinct id AND its own
    // drawn/active counts; two ids moving in lockstep would mean state is being shared again.
    private int InstanceId => RuntimeHelpers.GetHashCode(this);

    // Every SecondLessonPower currently on the creature, by identity. A repeated id would mean one
    // object listed twice — Creature.ApplyPowerInternal appends without an identity check and only
    // rejects duplicates for non-Instanced powers, so nothing structurally forbids it.
    private string InstanceRoster() =>
        string.Join(",", Owner.GetPowerInstances<SecondLessonPower>().Select(RuntimeHelpers.GetHashCode));

    private int Turn => Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (fromHandDraw && card.Owner == Owner.Player)
        {
            // Logged only when it actually repeats, so this stays quiet in the normal case. A card
            // genuinely can be drawn twice before the list is next cleared, which is legitimate and
            // what the reference-distinct selection tolerates. What this is really watching for is the
            // pathological version: every card in the hand recorded twice, which is what shared state
            // between Lessons looked like before the internal-data isolation above.
            int alreadyTracked = DrawnThisTurn.Count(c => ReferenceEquals(c, card));
            if (alreadyTracked > 0)
                Log.Info($"SecondLessonPower[turn {Turn}]: duplicate draw record id={InstanceId}, " +
                          $"card={card.Id} already tracked {alreadyTracked}x, " +
                          $"drawnThisTurn={DrawnThisTurn.Count}, lessons=[{InstanceRoster()}]");
            DrawnThisTurn.Add(card);
        }
        return Task.CompletedTask;
    }

    // There is deliberately no reset-on-play here. It used to exist because PowerCmd.Apply<T> reused
    // an existing same-type instance for stacking, so a combat retried after Orders had been assigned
    // could hand back an instance whose ActiveOrders still referenced the abandoned attempt's cards.
    // Instanced removes that entirely: FindExistingInstanceForStacking returns null, so every play
    // gets a ToMutable() clone that has never tracked anything. Reinstating a reset would be actively
    // wrong — it would strip the Orders a Lesson already has live on the board this turn.

    // Pure: given the ordered list of cards drawn this turn, which gets "Play this card" (the
    // first eligible one), which gets "Don't play this card" (the second eligible one), and what's
    // left over as candidates for the 25%-chance flavor Order.
    public static (CardModel? playThis, CardModel? dontPlayThis, IReadOnlyList<CardModel> remainingEligible)
        SelectFirstTwoEligible(IReadOnlyList<CardModel> drawnThisTurn)
    {
        // Distinct by reference, because the same card really can appear in drawnThisTurn more than
        // once — it is drawn, leaves the hand, and is drawn again before the list is next cleared.
        // Picking eligible[0] and eligible[1] off a list holding a card twice handed BOTH Orders to
        // that one card, which is why "Play this card" and "Don't play this card" were seen together
        // on a single card. Reference equality specifically: two separate copies of the same card are
        // different cards and must each stay individually eligible.
        var eligible = new List<CardModel>();
        foreach (var card in drawnThisTurn)
            if (OrderModifier.CanApplyTo(card) && !eligible.Any(seen => ReferenceEquals(seen, card)))
                eligible.Add(card);

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
        // Rewarded/Punished are singletons that every Lesson feeds and that apply themselves (see
        // RewardedPower/PunishedPower) — all this power does with them is bounds-check their growth.
        int lessons = Owner.GetPowerInstances<SecondLessonPower>().Count();
        // id / lessons are the identity pair to read together (see InstanceId): two firings this turn
        // sharing an id, or a roster listing one id twice, is the duplicate-listing case. Distinct ids
        // throughout means two genuine Lessons and any doubling came from somewhere else.
        // distinctDrawn vs drawnThisTurn separates "the same cards recorded repeatedly" from "simply
        // a lot of cards drawn", which is the difference between a dispatch problem and a busy turn.
        int distinctDrawn = DrawnThisTurn.Distinct(ReferenceEqualityComparer.Instance).Count();
        Log.Info($"SecondLessonPower[turn {turn}]: AfterPlayerTurnStartLate firing; " +
                  $"id={InstanceId}, lessons=[{InstanceRoster()}], " +
                  $"Rewarded={rewarded}, Punished={punished}, " +
                  $"drawnThisTurn={DrawnThisTurn.Count} ({distinctDrawn} distinct), " +
                  $"activeOrders={ActiveOrders.Count}");
        Invariants.CheckEqual(ActiveOrders.Count, ActiveOrders.Count(c => c.TryGetModifier<OrderModifier>(out _)),
            nameof(SecondLessonPower) + "." + nameof(AfterPlayerTurnStartLate),
            "tracked active-Order cards vs. cards still actually carrying OrderModifier");
        // Each assigned Order grants exactly one Reward or Punish stack when it resolves (FlavorOnly
        // always resolves to Resolution.None, granting nothing), and Orders are the ONLY source of
        // those two powers. Each Lesson assigns at most one PlayThis + one DontPlayThis per turn — and
        // may assign fewer, or none, when the cards drawn hold too few eligible (unafflicted,
        // non-Stable Attack/Skill) ones. So between turn starts the shared total can only grow, by
        // between 0 and two per live Lesson.
        int resolvedTotal = rewarded + punished;
        if (LastResolvedTotal is int previous)
        {
            int grew = resolvedTotal - previous;
            int maxGrowth = 2 * Math.Max(1, lessons);
            Invariants.Check(grew >= 0 && grew <= maxGrowth,
                nameof(SecondLessonPower) + "." + nameof(AfterPlayerTurnStartLate),
                $"Rewarded+Punished may grow by 0-{maxGrowth} per turn (≤2 Orders resolve per Lesson, " +
                $"{lessons} live) and never shrink — grew by {grew} (from {previous} to " +
                $"{resolvedTotal}); Rewarded={rewarded}, Punished={punished}");
        }
        LastResolvedTotal = resolvedTotal;

        await AssignOrders(context, player, turn);

        DrawnThisTurn.Clear();
    }

    private async Task AssignOrders(PlayerChoiceContext ctx, Player player, int turn)
    {
        var (playThis, dontPlayThis, remaining) = SelectFirstTwoEligible(DrawnThisTurn);

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
        // A card that already carries an Order must never take a second one — that is what produced a
        // single card showing both "Play this card" and "Don't play this card". Selection already
        // excludes such cards; this is the last line of defence, and reaching it means some path
        // routed around CanApplyTo, which is worth knowing about.
        if (card.TryGetModifier<OrderModifier>(out var existing))
        {
            Log.Warn($"SecondLessonPower[turn {turn}]: refused to attach Order({kind}) to {card.Id} " +
                      $"id={InstanceId} — it already carries Order({existing!.OrderKind}), " +
                      $"affliction={(card.Affliction == null ? "none" : card.Affliction.GetType().Name)}, " +
                      $"lessons=[{InstanceRoster()}]");
            return;
        }

        CardModifier.AddModifier<OrderModifier>(card);
        if (card.TryGetModifier<OrderModifier>(out var mod))
        {
            mod!.OrderKind = kind;
            mod.FlavorText = flavorText;
        }
        // Afflict reports refusal by returning null rather than throwing, which would leave the card
        // holding an Order with no affliction and so no overlay — invisible to the player, and
        // invisible to any check keyed off the affliction. Worth a line when it happens.
        var affliction = await CardCmd.Afflict<Order>(card, 1);
        ActiveOrders.Add(card);
        Log.Info($"SecondLessonPower[turn {turn}]: attached Order({kind}) to {card.Id} " +
                  $"id={InstanceId}{(affliction == null ? " [WARN: affliction refused, no overlay]" : "")}");
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player) return;

        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? -1;
        Invariants.CheckEqual(ActiveOrders.Count, ActiveOrders.Count(c => c.TryGetModifier<OrderModifier>(out _)),
            nameof(SecondLessonPower) + "." + nameof(BeforeSideTurnEnd),
            "tracked active-Order cards vs. cards still actually carrying OrderModifier");
        foreach (var card in ActiveOrders)
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

        ActiveOrders.Clear();
    }
}
