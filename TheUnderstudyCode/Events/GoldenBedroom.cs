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
using TheUnderstudy.TheUnderstudyCode.Extensions;
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
    // Gold: base "grab the loot" events sit ~60-100 (LostWisp 60, Field of Man-Sized Holes 75, several
    // at 100). 90 reads as the "hefty bag" the flavor promises without eclipsing the quest-relic choice.
    private const int GoldAmount = 90;

    // Without this, the event portrait resolves to the base game's images/events/<id>.png (which can't
    // hold a mod texture) and throws AssetLoadException. PLACEHOLDER: the character illustration until
    // dedicated Golden Bedroom art is drawn.
    public override string? CustomInitialPortraitPath => "charui/char_select_the_understudy.png".ImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        new DynamicVar[] { new GoldVar(GoldAmount) };

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
        // Sleeping is resting — so heal exactly as a campfire would (MimicRestSiteHeal), the base
        // game's own mechanic for rest-flavored event heals (Dense Vegetation). Scales with Max HP,
        // unlike a flat number, and matches "you wake up feeling refreshed".
        Log.Info("[GoldenBedroom] SleepInTheBed -> rest-site heal");
        await PlayerCmd.MimicRestSiteHeal(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SLEEP_IN_THE_BED.description"));
    }

    private async Task ChaseTheBook()
    {
        Log.Info("[GoldenBedroom] ChaseTheBook -> obtain Chaotic Book");
        await RelicCmd.Obtain<ChaoticBook>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CHASE_THE_BOOK.description"));
    }
}
