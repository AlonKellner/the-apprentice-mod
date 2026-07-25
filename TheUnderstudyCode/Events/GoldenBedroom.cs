using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Understudy = TheUnderstudy.TheUnderstudyCode.Character.TheUnderstudy;

namespace TheUnderstudy.TheUnderstudyCode.Events;

// "The Golden Bedroom" — through strange secret corridors the Understudy reaches a child's bedroom
// made of gold: the room of The Child, the perfect mirror the Architect made to replace himself, the
// one consumed by the Blight. The bed is dreamless, like its creator. A golden book — the Architect's
// story of endings — darts away, and you can chase it.
//
// Three options: take the untouched golden toys (gold), sleep in the dreamless bed (heal), or chase
// the book (the Chaotic Book quest relic). Understudy-only (IsAllowed); shared across all acts (empty
// Acts). Loc in events.json under THEUNDERSTUDY-GOLDEN_BEDROOM. Modelled on the base game's LostWisp.
public class GoldenBedroom : CustomEventModel
{
    private const int GoldAmount = 75;
    private const int HealAmount = 20;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new GoldVar(GoldAmount),
        new HealVar(HealAmount),
    };

    // The Golden Bedroom is the Understudy's story; it must not appear in other characters' runs.
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.Any(p => p.Character is Understudy);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => new[]
    {
        Option(TakeGoldenToys),
        Option(SleepInTheBed),
        Option(ChaseTheBook, HoverTipFactory.FromRelic<ChaoticBook>()),
    };

    private async Task TakeGoldenToys()
    {
        Log.Info($"[GoldenBedroom] TakeGoldenToys -> +{DynamicVars["Gold"].IntValue} gold");
        await PlayerCmd.GainGold(DynamicVars["Gold"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TAKE_GOLDEN_TOYS.description"));
    }

    private async Task SleepInTheBed()
    {
        Log.Info($"[GoldenBedroom] SleepInTheBed -> heal {DynamicVars["Heal"].IntValue}");
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars["Heal"].IntValue);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SLEEP_IN_THE_BED.description"));
    }

    private async Task ChaseTheBook()
    {
        Log.Info("[GoldenBedroom] ChaseTheBook -> obtain Chaotic Book");
        await RelicCmd.Obtain<ChaoticBook>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CHASE_THE_BOOK.description"));
    }
}
