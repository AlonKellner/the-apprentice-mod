using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

// The Second Lesson's per-card Order: carries the state, eligibility, and description text for
// whichever card it's attached to. Paired with a companion Order affliction (Cards/Afflictions/
// Order.cs) purely for the visual overlay — the two attach together and are removed together,
// but this modifier owns everything about *which* Order it is and how it resolves.
public class OrderModifier : CardModifier
{
    public enum Kind
    {
        PlayThis,
        DontPlayThis,
        FlavorOnly
    }

    public enum Resolution
    {
        Reward,
        Punish,
        None
    }

    public Kind OrderKind { get; set; }
    public string? FlavorText { get; set; }
    public bool WasPlayed { get; private set; }

    // True once this Order has granted a Reward/Punish stack (mid-turn, via OnPlay below) — lets
    // SecondLessonPower's end-of-turn sweep skip an Order that already resolved, instead of
    // resolving it a second time via OnTurnEndIfUnresolved.
    public bool Resolved { get; private set; }

    // Flavor-only Orders never have a mechanical effect — this is the closed, easily-edited list
    // of narrative lines a flavor Order can be given.
    public static readonly string[] FlavorLines =
    {
        "Cease this cacophony.",
        "Tone it down.",
        "Be quiet.",
        "Listen to me.",
        "Kill it.",
        "Again.",
        "Stop crying.",
        "Calm down.",
        "Don't be scared.",
        "Look at me.",
        "Stand still.",
        "Wake up.",
        "Can you kill me?",
        "Answer my question.",
        "Replace me.",
        "End my story.",
        "Die."
    };

    // Attacks and Skills only, not Stable-tagged (defined fresh here rather than reusing
    // PlannedModifier.CanApplyTo, so each modifier owns its own eligibility rule), and never a card
    // that already carries an affliction.
    //
    // That last clause is what keeps two SecondLessonPower instances from fighting over the same
    // card: whichever Lesson assigns first afflicts the card with Order, so the next Lesson's
    // selection skips it and reaches for a different one. It also mirrors the game's own rule —
    // AfflictionModel.CanAfflict refuses an already-afflicted card, and CardCmd.Afflict signals that
    // by silently returning null, which would otherwise leave the card holding an OrderModifier with
    // no Order affliction (and so no overlay) for the rest of the turn.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill)
        && !card.IsStable()
        && card.Affliction == null;

    // Resolution the instant the order-carrying card is played this turn: "Play this card" is
    // obeyed (Reward); "Don't play this card" is disobeyed (Punish); flavor-only never resolves.
    public static Resolution OnCardPlayed(Kind kind) => kind switch
    {
        Kind.PlayThis => Resolution.Reward,
        Kind.DontPlayThis => Resolution.Punish,
        _ => Resolution.None
    };

    // Mirror resolution for an order that reached end of turn without ever being played: "Play
    // this card" is disobeyed (Punish); "Don't play this card" is obeyed (Reward).
    public static Resolution OnTurnEndIfUnresolved(Kind kind) => kind switch
    {
        Kind.PlayThis => Resolution.Punish,
        Kind.DontPlayThis => Resolution.Reward,
        _ => Resolution.None
    };

    // Scoped to this modifier's own card by BaseLib (CardModifier.Modifiers(cardPlay.Card) is what
    // gets iterated to dispatch OnPlay) — no manual cardPlay.Card == Owner check needed.
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Resolve at most once per turn. A card carrying an Order can be played more than once in a
        // turn — replayed from a Planned queue (Remix/DaCapo/Workshop/Showtime) or via a Replay — and
        // each extra play would otherwise re-apply the Reward/Punish, breaking SecondLessonPower's
        // "two Orders resolve per turn" parity invariant. WasPlayed is already set from the first play.
        if (Resolved) return;

        WasPlayed = true;
        var resolution = OnCardPlayed(OrderKind);
        int turn = cardPlay.Card.Owner.Creature.Player?.PlayerCombatState?.TurnNumber ?? -1;
        Log.Info($"OrderModifier[turn {turn}]: {cardPlay.Card.Id} with Order({OrderKind}) was played -> {resolution}");
        if (resolution == Resolution.None) return;
        Resolved = true;

        var creature = cardPlay.Card.Owner.Creature;
        if (resolution == Resolution.Reward)
            await PowerCmd.Apply<RewardedPower>(choiceContext, creature, 1, creature, null, false);
        else
            await PowerCmd.Apply<PunishedPower>(choiceContext, creature, 1, creature, null, false);
    }

    public override void ModifyDescriptionPost(Creature? creature, ref string description)
    {
        string text = OrderKind switch
        {
            Kind.PlayThis => "Play this card.",
            Kind.DontPlayThis => "Don't play this card.",
            Kind.FlavorOnly => FlavorText ?? "",
            _ => ""
        };
        if (string.IsNullOrEmpty(text)) return;
        description = $"[red][sine]{text}[/sine][/red]\n" + description;
    }
}
