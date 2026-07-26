using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FateKnocking : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FateKnocking";

    private const int Strikes = 3;

    // Combat-scoped: the ACTUAL damage each FateKnocking's base strikes have dealt this combat (after
    // Strength/Weak/Vulnerable/block — read from the attack's DamageResults). The finisher deals that
    // running sum. Keyed by the card instance; persists across replays of the same card.
    private static ICombatState? _lastCombat;
    private static readonly Dictionary<CardModel, int> _damageDealt = new();

    public FateKnocking() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // 3 strikes, then a finisher equal to the running sum of damage this card has dealt this combat.
        // Not Stable, so Tuned/Planned can attach and scale the strikes/finisher. Strength/Vigor/Weak/
        // Vulnerable apply as usual (creature powers). Upgrade raises the per-strike base damage 1 -> 2.
        WithDamage(1);

        // Display-only, Body-Slam-style preview of the finisher hit. Raw value = CalculationBase(0) +
        // ExtraDamage(1) * (priorSum + Strikes * modified-strike - vigor); CalculatedDamageVar then runs the
        // SAME Hook.ModifyDamage the real finisher runs, so "(Deals N damage)" equals the finisher's actual
        // damage — including the double-scale (Strength re-applies to the total), Vulnerable, and the
        // Intangible cap. Registered AFTER WithDamage so the strike Damage var's PreviewValue (the modified
        // per-strike number) is already computed when this multiplier reads it.
        WithVars(
            new CalculationBaseVar(0m),
            new ExtraDamageVar(1m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
                ComputeFinisherBase(
                    PriorSumThisCombat(card), Strikes,
                    card.DynamicVars.Damage.PreviewValue, VigorConsumedByStrikes(card))));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    // Pure: the finisher's raw (pre-ModifyDamage) base = the accumulated prior sum plus the 3 upcoming
    // strikes at their current modified per-hit damage, minus the Vigor those strikes will consume.
    // CalculatedDamageVar applies ModifyDamage on top.
    //
    // The Vigor subtraction exists to CANCEL a copy, not to remove one. perStrikeDamage already contains
    // Vigor, and ModifyDamage will add it to the total a second time — correct for Strength (the finisher
    // is a real attack and does get +Str) but wrong for Vigor, which the strikes consume before the
    // finisher's separate attack command ever runs. Subtracting it here makes the two cancel:
    // ((base - V) + Str + V) * mult == (base + Str) * mult, so this stays exact under Vulnerable/Weak
    // multipliers rather than only under flat modifiers.
    public static decimal ComputeFinisherBase(
        int priorSum, int strikes, decimal perStrikeDamage, decimal vigorConsumedByStrikes) =>
        priorSum + ExpectedStrikeTotal(strikes, perStrikeDamage) - vigorConsumedByStrikes;

    // What the strikes will actually add up to. Damage lands per hit and Creature.LoseHpInternal casts it
    // to int — a truncating cast — so a fractional per-strike number (any multiplicative modifier will
    // produce one: Weak, Vulnerable, ...) is truncated three separate times, not once at the end.
    // Extrapolating 3 x 4.5 = 13.5 when the hits really deal 4+4+4 = 12 overstated the finisher by the
    // discarded remainder, which is damage the card can never collect.
    //
    // Clamped at zero because damage is clamped at zero on application too, so a per-strike number driven
    // negative (Vigor can go negative under VigorAllowNegativePatch) contributes nothing rather than
    // subtracting from the running sum.
    public static decimal ExpectedStrikeTotal(int strikes, decimal perStrikeDamage) =>
        strikes * Math.Max(0m, decimal.Truncate(perStrikeDamage));

    // The Vigor the strikes will have consumed by the time the finisher resolves, so the preview above can
    // cancel ModifyDamage's re-add. VigorPower latches onto the first attack command it sees and zeroes
    // itself in AfterAttack, so Fate Knocking's strikes take all of it and the finisher — a second, separate
    // command — gets none.
    //
    // Zero while Reverb is up: ReverbVigorRetentionPatch cancels that consumption (and unlatches the power),
    // so the finisher really does keep its Vigor and there is nothing to cancel.
    //
    // Assumes the strikes are what latch the power. That holds because every path out of an attack leaves
    // VigorPower unlatched for the next card — either it consumed itself to 0 and was removed (a later gain
    // mints a fresh instance), or Reverb retained it and explicitly nulled commandToModify.
    private static decimal VigorConsumedByStrikes(CardModel card)
    {
        if (!card.IsMutable) return 0m; // canonical card: Owner throws (unlike CombatState)
        var creature = card.Owner?.Creature;
        if (creature == null || ReverbPower.IsActive(creature)) return 0m;
        return creature.GetPowerAmount<VigorPower>();
    }

    // The damage this card's strikes have dealt in the CURRENT combat (0 on a new combat, clearing stale
    // carryover). Combat-aware so the preview is correct before the card's first play this combat, not only
    // after OnPlay's reset. Used by both the preview multiplier and OnPlay.
    private static int PriorSumThisCombat(CardModel card)
    {
        var combat = card.CombatState; // null-safe on a canonical card (unlike Owner)
        if (combat == null) return 0;
        if (!ReferenceEquals(combat, _lastCombat)) { _lastCombat = combat; _damageDealt.Clear(); }
        return _damageDealt.TryGetValue(card, out int v) ? v : 0;
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        int prior = PriorSumThisCombat(card);

        // The running sum is written under `this` further down but read back through `card`. If a play
        // ever routed a different instance through here (a replay or a Planned-queue resolver handing
        // over a clone), the sum would silently stop accumulating and every preview would read 0.
        Invariants.Check(ReferenceEquals(this, card), nameof(FateKnocking) + "." + nameof(OnPlay),
            "the played card is not this instance, so the accumulated damage would be stored under a " +
            "different key than it is read from");

        // What the card was showing before it resolved: the finisher's previewed damage, and the
        // per-strike number the preview extrapolated it from. Captured up front, because the strikes
        // below change the state both are derived from.
        decimal previewedFinisher = card.DynamicVars["CalculatedDamage"].PreviewValue;
        decimal previewedPerStrike = card.DynamicVars.Damage.PreviewValue;

        // The base strikes — capture the ACTUAL damage they deal (after all modifiers and block). We pass the
        // Damage var EXPLICITLY: CommonActions.CardAttack(card, cardPlay, hitCount) auto-prefers a card's
        // CalculatedDamage var over its Damage var, and ours is only the display-only finisher preview — so
        // the plain multi-hit form would (wrongly) make each strike deal the whole finisher total. This
        // mirrors what BaseLib itself does for a card with just a Damage var.
        var strikes = await CommonActions.CardAttack(
            card, cardPlay, cardPlay.Target,
            ((DynamicVar)card.DynamicVars.Damage).BaseValue, card.DynamicVars.Damage.Props, Strikes).Execute(context);
        int strikeDamage = strikes.Results.SelectMany(r => r).Sum(dr => dr.TotalDamage);

        int total = prior + strikeDamage;
        _damageDealt[this] = total;

        // Finisher: deal that running sum as a single hit. Its own damage is not summed back in, so
        // it doesn't compound play-to-play.
        //
        // Passes cardPlay explicitly, like the strikes above. The shorter overload that takes only a
        // target is deprecated ("will be required for the beta branch") and forwarded with a null
        // CardPlay, so this both clears the warning and stops the finisher being the one attack on
        // this card with no play context. ValueProp.Move is what that overload supplied by default —
        // it is the powered-attack flag, which is what lets Tuned's damage bonus apply.
        if (total > 0)
        {
            var finisher = await CommonActions
                .CardAttack(card, cardPlay, cardPlay.Target, (decimal)total, ValueProp.Move).Execute(context);

            // Overkill is added back for the COMPARISON below only — never for the running sum, which is
            // deliberately the damage this card actually dealt, not what it theoretically rolled.
            // Creature.LoseHpInternal computes UnblockedDamage as (hpBefore - hpAfter) and parks the excess
            // in OverkillDamage, so TotalDamage is clamped to the target's remaining HP. The preview
            // predicts the rolled number, so without this every finisher that kills its target would report
            // a spurious mismatch.
            int finisherDamage = finisher.Results.SelectMany(r => r).Sum(dr => dr.TotalDamage + dr.OverkillDamage);

            // The whole point of the CalculatedDamageVar above is that "(Deals N damage)" is the number
            // the finisher actually hits for. Both run the same Hook.ModifyDamage, so they can only
            // disagree if the preview's raw base disagrees with the finisher's — which is exactly the
            // drift that silently broke Tuned's damage bonus once, when a game update unbound a
            // ModifyDamage override for one path but not the other.
            //
            // Only assert it on a genuine hand-play, where the on-screen preview is the authority. When
            // the card is auto-played from the Planned queue (Workshop/Da Capo → CardCmd.AutoPlay, which
            // sets CardPlay.IsAutoPlay), its cached preview is whatever the card last showed in hand —
            // computed against a DIFFERENT target/state than this resolution — so comparing it to the live
            // finisher is meaningless and logs a spurious mismatch. (An earlier version gated on
            // previewedFinisher > 0 as a proxy for "was displayed", but a card shown in hand then Planned
            // keeps a non-zero stale preview, so that proxy fired here wrongly.)
            //
            // Even on a hand-play, only assert when the preview's premise held: the preview extrapolates
            // Strikes x the per-strike number, while the finisher sums what the strikes actually dealt; a
            // target that dies partway, or a modifier that changes mid-sequence, makes those legitimately
            // differ. Block does not trip this (TotalDamage counts blocked damage), and neither does a
            // lethal strike — the running sum is clamped to the target's remaining HP, which makes this
            // premise false and skips the assert rather than reporting a mismatch.
            bool strikesLandedAsPreviewed = strikeDamage == (int)ExpectedStrikeTotal(Strikes, previewedPerStrike);
            if (!cardPlay.IsAutoPlay && previewedFinisher > 0 && strikesLandedAsPreviewed)
                Invariants.CheckEqual((int)previewedFinisher, finisherDamage,
                    nameof(FateKnocking) + "." + nameof(OnPlay),
                    "previewed finisher damage vs. the damage the finisher actually calculated");
        }
    }
}
