using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
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
    // Matches the sibling modifiers' convention (PlannedModifier/TunedModifier) and is the prefix
    // this modifier's entries use in the card_modifiers loc table.
    public const string ModifierId = "TheUnderstudy:Order";

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

    // ── Text ─────────────────────────────────────────────────────────────────────────────────
    // The words themselves live in localization/eng/card_modifiers.json so they can be edited (and
    // translated) without touching code. Only the formatting stays here.
    private const string LocTable = "card_modifiers";

    private static string Text(string key) => new LocString(LocTable, ModifierId + "." + key).GetFormattedText();

    // Flavor-only Orders never have a mechanical effect — they are pure narrative. The lines are
    // numbered from 0 in the loc file and read by scanning upward until an index is missing, the
    // same contiguous-index convention BaseLib uses for ancient dialogue. Adding a line means
    // appending the next number; leaving a gap silently truncates the list at the gap.
    public static IReadOnlyList<string> FlavorLines
    {
        get
        {
            var lines = new List<string>();
            for (int i = 0; LocString.Exists(LocTable, $"{ModifierId}.flavor.{i}"); i++)
                lines.Add(Text($"flavor.{i}"));
            return lines;
        }
    }

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
    // The OrderModifier clause is not redundant with the affliction one: CardCmd.Afflict reports
    // refusal by returning null, so a card can end up carrying an Order with no affliction to show
    // for it (an Unplayable card, for one — AfflictionModel.CanAfflict rejects those). Keying only
    // off the affliction would let such a card take a second Order and display two contradictory
    // commands at once.
    public static bool CanApplyTo(CardModel card) =>
        (card.Type == CardType.Attack || card.Type == CardType.Skill)
        && !card.IsStable()
        && card.Affliction == null
        && !card.TryGetModifier<OrderModifier>(out _);

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

    // Pure formatting, kept in code on purpose: the Order's styling is not something a translator
    // should have to reproduce, and keeping it separate leaves a seam that can be unit-tested
    // without a loaded loc table.
    public static string Decorate(string text, string description) =>
        string.IsNullOrEmpty(text) ? description : $"[red][sine]{text}[/sine][/red]\n" + description;

    public override void ModifyDescriptionPost(Creature? creature, ref string description)
    {
        string text = OrderKind switch
        {
            Kind.PlayThis => Text("playThis"),
            Kind.DontPlayThis => Text("dontPlayThis"),
            Kind.FlavorOnly => FlavorText ?? "",
            _ => ""
        };
        description = Decorate(text, description);
    }
}
