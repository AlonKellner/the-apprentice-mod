using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

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

        // ── DIAGNOSTIC (temporary) — captures the shown preview vs every actual number so we can pinpoint
        //    where the finisher preview drifts (esp. with Tuned/Planned now that this card isn't Stable). ──
        decimal perStrikePreview = card.DynamicVars.Damage.PreviewValue;
        decimal finisherBaseRaw = ComputeFinisherBase(prior, Strikes, perStrikePreview);
        decimal shownCalcDamage = card.DynamicVars.ContainsKey("CalculatedDamage")
            ? card.DynamicVars["CalculatedDamage"].PreviewValue : -1m;
        string tuned = card.TryGetModifier<TunedModifier>(out var t) ? $"stacks={t!.Stacks},bonus={t.Bonus}" : "none";
        Log.Info($"FateKnocking DIAG pre-play: prior={prior} perStrikePreview={perStrikePreview} " +
                 $"finisherBaseRaw={finisherBaseRaw} shownCalcDamage={shownCalcDamage} tuned=[{tuned}] upgraded={IsUpgraded}");

        // The base strikes — capture the ACTUAL damage they deal (after all modifiers and block).
        var strikes = await CommonActions.CardAttack(card, cardPlay, Strikes).Execute(context);
        var strikeHits = strikes.Results.SelectMany(r => r).Select(dr => dr.TotalDamage).ToList();
        int strikeDamage = strikeHits.Sum();

        int total = prior + strikeDamage;
        _damageDealt[this] = total;
        Log.Info($"FateKnocking DIAG strikes: hits=[{string.Join(",", strikeHits)}] sum={strikeDamage} total(prior+sum)={total}");

        // Finisher: deal that running sum as a single hit. Its own damage is not summed back in, so
        // it doesn't compound play-to-play.
        if (total > 0)
        {
            var fin = await CommonActions.CardAttack(card, cardPlay.Target, (decimal)total).Execute(context);
            int finisherDamage = fin.Results.SelectMany(r => r).Sum(dr => dr.TotalDamage);
            Log.Info($"FateKnocking DIAG finisher: requested(total)={total} actualDealt={finisherDamage} " +
                     $"| shownCalcDamage(preview) was {shownCalcDamage}");
        }
    }
}
