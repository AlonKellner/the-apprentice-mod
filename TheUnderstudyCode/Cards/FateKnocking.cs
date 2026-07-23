using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
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
        // ExtraDamage(1) * (priorSum + Strikes * modified-strike); CalculatedDamageVar then runs the SAME
        // Hook.ModifyDamage the real finisher runs, so "(Deals N damage)" equals the finisher's actual
        // damage — including the double-scale (Strength/Vigor re-apply to the total), Vulnerable, and the
        // Intangible cap. Registered AFTER WithDamage so the strike Damage var's PreviewValue (the modified
        // per-strike number) is already computed when this multiplier reads it.
        WithVars(
            new CalculationBaseVar(0m),
            new ExtraDamageVar(1m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
                ComputeFinisherBase(PriorSumThisCombat(card), Strikes, card.DynamicVars.Damage.PreviewValue)));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    // Pure: the finisher's raw (pre-ModifyDamage) base = the accumulated prior sum plus the 3 upcoming
    // strikes at their current modified per-hit damage. CalculatedDamageVar applies ModifyDamage on top.
    public static decimal ComputeFinisherBase(int priorSum, int strikes, decimal perStrikeDamage) =>
        priorSum + strikes * perStrikeDamage;

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
            int finisherDamage = finisher.Results.SelectMany(r => r).Sum(dr => dr.TotalDamage);

            // The whole point of the CalculatedDamageVar above is that "(Deals N damage)" is the number
            // the finisher actually hits for. Both run the same Hook.ModifyDamage, so they can only
            // disagree if the preview's raw base disagrees with the finisher's — which is exactly the
            // drift that silently broke Tuned's damage bonus once, when a game update unbound a
            // ModifyDamage override for one path but not the other.
            //
            // Only assert it when the preview's premise held. The preview extrapolates Strikes x the
            // per-strike number, while the finisher sums what the strikes actually dealt; a target that
            // dies partway, or a modifier that changes mid-sequence, makes those legitimately differ.
            // TotalDamage is pre-block (BlockedDamage + UnblockedDamage), so a blocking target does not
            // trip this. A previewed finisher of 0 means the card was never displayed (auto-played from
            // a Planned queue), so there is no preview to hold it to.
            bool strikesLandedAsPreviewed = strikeDamage == (int)(Strikes * previewedPerStrike);
            if (previewedFinisher > 0 && strikesLandedAsPreviewed)
                Invariants.CheckEqual((int)previewedFinisher, finisherDamage,
                    nameof(FateKnocking) + "." + nameof(OnPlay),
                    "previewed finisher damage vs. the damage the finisher actually calculated");
        }
    }
}
