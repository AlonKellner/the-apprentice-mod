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

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// "Obey my Orders. When you do, you'll be Rewarded. When you don't, you'll be Punished."
//
// Every player turn, in AfterPlayerTurnStartLate (after energy refill and draw have already
// happened — deliberately, so a fresh Reward/Punish debuff/buff never bites the turn it's
// granted, only the next one): resolve Reward, then Punish (in that fixed order, both from this
// one method, so the ordering can't depend on cross-power hook iteration), then assign this
// turn's new Orders to cards drawn this turn.
public class SecondLessonPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "The Second Lesson",
        "Each turn, some cards will have an [red]Order[/red]. Obeying grants [gold]Rewarded[/gold]; disobeying grants [gold]Punished[/gold].",
        "Each turn, some cards will have an [red]Order[/red]. Obeying grants [gold]Rewarded[/gold]; disobeying grants [gold]Punished[/gold].");

    private readonly List<CardModel> _drawnThisTurn = new();

    // Cards this power currently has an active Order attached to, tracked by direct reference —
    // a played card leaves the Hand pile before BeforeSideTurnEnd fires, but the modifier/
    // affliction stay attached to the CardModel object regardless of which pile it's in.
    private readonly List<CardModel> _activeOrders = new();

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (fromHandDraw && card.Owner == Owner.Player) _drawnThisTurn.Add(card);
        return Task.CompletedTask;
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
        Log.Info($"SecondLessonPower[turn {turn}]: AfterPlayerTurnStartLate firing; " +
                  $"Rewarded={Owner.GetPowerAmount<RewardedPower>()}, Punished={Owner.GetPowerAmount<PunishedPower>()}, " +
                  $"drawnThisTurn={_drawnThisTurn.Count}, activeOrders={_activeOrders.Count}");

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
        var debuff = EmotionalExpression.PickByPriority(categories, EmotionalExpression.RewardPriority, PickRandom);
        Log.Info($"SecondLessonPower[turn {turn}]: applying Reward buff {debuff} x{rewarded.Amount}");
        await EmotionalExpression.ApplyBuffSide(ctx, Owner, debuff, rewarded.Amount);
    }

    private async Task ResolvePunishTurn(PlayerChoiceContext ctx, int turn)
    {
        var punished = Owner.GetPower<PunishedPower>();
        if (punished == null || punished.Amount <= 0) return;
        var categories = EmotionalExpression.BuildCategories(Owner);
        bool firstLessonActive = Owner.GetPowerAmount<TheFirstLessonPower>() > 0;
        var filtered = EmotionalExpression.ExcludeForPunishIfFirstLessonActive(categories, firstLessonActive);
        var debuff = EmotionalExpression.PickByPriority(filtered, EmotionalExpression.PunishPriority, PickRandom);
        Log.Info($"SecondLessonPower[turn {turn}]: applying Punish debuff {debuff} x{punished.Amount}");
        await EmotionalExpression.ApplyDebuffSide(ctx, Owner, debuff, punished.Amount);
    }

    private InvertibleDebuff PickRandom(IReadOnlyList<InvertibleDebuff> candidates) =>
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
