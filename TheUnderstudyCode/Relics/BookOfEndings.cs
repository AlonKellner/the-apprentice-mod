using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Map;
using TheUnderstudy.TheUnderstudyCode.RestSite;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// The Architect's book, found in "The Golden Bedroom". Study it at a Rest Site to reveal one alternative
// boss on the current act's map, up to 2 per act — the Alternative Bosses mechanic, bought a piece at a
// time instead of unlocked wholesale. It replaces the old Chaotic Book / Book of Order pair: one relic,
// no transform, and it does something the first time you use it.
//
// Studies are a BANK, not a per-act counter (see AltBossReveal): each act draws from the bank up to its
// own capacity and leaving the act consumes only what it absorbed, so a study an act had no room for
// carries into the next one. That is a safety net for the co-op race where two players both Study before
// the map redraws — the primary control is the rest-site button disabling itself once the act is full.
//
// Event-obtained; TheUnderstudyRelicPool excludes it as event-only, so it never drops as a reward.
[Pool(typeof(TheUnderstudyRelicPool))]
public class BookOfEndings : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;

    // ── Saved bank state ─────────────────────────────────────────────────────────────────────────────
    // The bank is the one part of this mechanic that cannot be re-derived from the run seed (everything
    // in AltBossPlan can), so it has to persist and to travel between co-op clients — hence
    // [SavedProperty], the same shape the base game's GoldenCompass uses for its per-act bookkeeping.
    //
    // StudyCount is PER HOLDER and additive: a player's Studies land on their own relic, and the party
    // total is the sum. Consumed/LastActIndex are PARTY-WIDE values mirrored onto every holder by the
    // settle step, and are read from the primary (lowest-index) holder so a stale mirror can't be read.

    private int _studyCount;
    private int _consumed;
    private int _lastActIndex = -1;

    /// Studies performed by THIS holder. Party total = sum across holders.
    [SavedProperty]
    public int StudyCount
    {
        get => _studyCount;
        set { AssertMutable(); _studyCount = value; }
    }

    /// Party-wide: alternative bosses revealed by acts already left behind.
    [SavedProperty]
    public int Consumed
    {
        get => _consumed;
        set { AssertMutable(); _consumed = value; }
    }

    /// Party-wide: the act the bank was last settled for. -1 so act 0 is never taken as already settled.
    [SavedProperty]
    public int LastActIndex
    {
        get => _lastActIndex;
        set { AssertMutable(); _lastActIndex = value; }
    }

    // ── Party aggregation ────────────────────────────────────────────────────────────────────────────

    // Every player holding a book, in player order. The first is the "primary" whose Consumed /
    // LastActIndex mirror is authoritative. Normally there is exactly one: The Golden Bedroom grants to
    // whoever chose the option.
    internal static IReadOnlyList<BookOfEndings> Holders(IRunState state) =>
        state.Players.Select(p => p.GetRelic<BookOfEndings>())
                     .Where(r => r != null)
                     .Select(r => r!)
                     .ToList();

    // The counter ring: the party's unspent studies. Deliberately the BANK and not the number of bosses
    // revealed — the two agree in normal play (Study disables at the cap), so a 3 showing while the act
    // has only 2 bosses is the visible signature that a co-op race banked a surplus.
    public override int DisplayAmount
    {
        get
        {
            // RelicModel.Owner asserts mutability, so a canonical relic — which is what the compendium
            // and relic library render — must never be asked for it.
            if (!IsMutable) return 0;
            var state = Owner?.RunState;
            if (state == null) return 0;

            var holders = Holders(state);
            if (holders.Count == 0) return 0;

            return AltBossReveal.Bank(
                AltBossReveal.PartyStudies(holders.Select(h => h.StudyCount)),
                holders[0].Consumed);
        }
    }

    // How many alternative bosses act `actIndex` can ever offer: 2 on the usual 3-boss act, 1 on a
    // 2-boss act, 0 when there is nothing to choose between. Pure in the run seed, so it can be asked
    // about any act — which is what lets the settle step below price an act the player has already left.
    private static int CapFor(IRunState state, int actIndex) =>
        AltBossPlan.AssignFlanks(
            state.Acts[actIndex].AllBossEncounters.Select(e => e.Id.ToString()).ToList(),
            state.Acts[actIndex].BossEncounter.Id.ToString(),
            AltBossPlan.SeedFor(state.Rng.Seed, actIndex)).Count;

    // Charge the act the player just left for what it actually showed, and carry the rest. Mirrored onto
    // every holder so the party-wide figures stay identical; each client runs this with the same inputs.
    // Idempotent by the act stamp: a mid-act regeneration (every Study triggers one) settles nothing.
    private static void SettleIfActChanged(IRunState state, int actIndex)
    {
        var holders = Holders(state);
        if (holders.Count == 0) return;

        var primary = holders[0];
        if (primary.LastActIndex == actIndex) return;

        int consumed = primary.Consumed;
        if (primary.LastActIndex >= 0)
        {
            int studies = AltBossReveal.PartyStudies(holders.Select(h => h.StudyCount));
            consumed = AltBossReveal.Settle(studies, consumed, CapFor(state, primary.LastActIndex));
        }

        foreach (var h in holders)
        {
            h.Consumed = consumed;
            h.LastActIndex = actIndex;
        }
    }

    // Whether the Rest Site's Study option is usable right now. The option is always shown; this is what
    // greys it out once this act's alternative bosses are all revealed, the same way Smith greys out when
    // every card is upgraded.
    public bool CanStudyNow()
    {
        if (!IsMutable) return false;
        var state = Owner?.RunState;
        if (state == null) return false;

        var holders = Holders(state);
        if (holders.Count == 0) return false;

        return AltBossReveal.CanStudy(
            AltBossReveal.PartyStudies(holders.Select(h => h.StudyCount)),
            holders[0].Consumed,
            CapFor(state, state.CurrentActIndex));
    }

    // Called by StudyRestSiteOption when the player spends a rest action studying. The study lands on
    // THIS holder's count; the reveal itself comes from regenerating the map, which is deterministic
    // (seeded) so it returns the same map plus the newly revealed flank, redraws immediately via
    // NMapScreen.SetMap, and keeps the player's position — exactly how Golden Compass and the old Book of
    // Order made their map changes appear mid-run.
    public async Task Study()
    {
        StudyCount++;

        // Refresh every holder's ring, not just this one: the counter shows the PARTY bank, so a co-op
        // partner's relic would otherwise keep displaying the pre-Study number.
        var state = Owner?.RunState;
        foreach (var h in state != null ? Holders(state) : new[] { this })
            h.InvokeDisplayAmountChanged();
        Flash();

        int partyStudies = state != null
            ? AltBossReveal.PartyStudies(Holders(state).Select(h => h.StudyCount))
            : StudyCount;
        Log.Info($"[BookOfEndings] Study: this holder {StudyCount}, party {partyStudies}");

        if (RunManager.Instance != null)
            await RunManager.Instance.GenerateMap();
    }

    // Inject the revealed flank boss nodes. Runs in ModifyGeneratedMapLate (not ModifyGeneratedMap)
    // because that hook fires on BOTH paths: fresh generation (Hook.ModifyGeneratedMap ends by calling
    // it) AND load (GenerateMap's saved-map branch calls only it). The base save format has no slot for
    // alt bosses, so instead of serializing them they are re-derived here from AltBossPlan — a pure
    // function of the run seed + act index — so the same nodes, with the same left/right assignment,
    // reappear identically on load and on every co-op client. Idempotent via the store check, so it
    // can't double-inject; a Study reveals by regenerating the map, which builds a fresh ActMap and
    // therefore an empty store.
    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        if (AltBossStore.For(map).Count > 0) return map; // already injected on this map instance

        SettleIfActChanged(runState, actIndex);

        var act = runState.Acts[actIndex];
        var allBossIds = act.AllBossEncounters.Select(e => e.Id.ToString()).ToList();
        var defaultId = act.BossEncounter.Id.ToString();
        ulong seed = AltBossPlan.SeedFor(runState.Rng.Seed, actIndex);
        var flanks = AltBossPlan.AssignFlanks(allBossIds, defaultId, seed);
        if (flanks.Count == 0)
        {
            Log.Info($"[BookOfEndings] act {actIndex}: only one boss in pool, no alt bosses to inject");
            return map;
        }

        var holders = Holders(runState);
        int studies = AltBossReveal.PartyStudies(holders.Select(h => h.StudyCount));
        int consumed = holders.Count > 0 ? holders[0].Consumed : 0;
        int revealed = AltBossReveal.RevealedInAct(studies, consumed, flanks.Count);
        if (revealed == 0)
        {
            Log.Info($"[BookOfEndings] act {actIndex}: studies={studies} consumed={consumed} " +
                     $"bank={AltBossReveal.Bank(studies, consumed)} cap={flanks.Count}; nothing revealed yet");
            return map;
        }

        var preBossRow = map.GetPointsInRow(map.GetRowCount() - 1).ToList();
        if (preBossRow.Count == 0)
        {
            Log.Warn($"[BookOfEndings] act {actIndex}: no pre-boss row, cannot inject alt bosses");
            return map;
        }

        int bossRow = map.BossMapPoint.coord.row;
        int cols = map.GetColumnCount();
        int defaultCol = map.BossMapPoint.coord.col;
        var farLeftRest = preBossRow.OrderBy(p => p.coord.col).First();
        var farRightRest = preBossRow.OrderByDescending(p => p.coord.col).First();

        var injected = new List<string>();
        var flankNodes = new List<(FlankSide side, string enc, MapPoint node)>();
        foreach (var (side, encounterId) in flanks.Take(revealed))
        {
            // Left node hugs col 0, right node hugs the last column, so they render on either side of the
            // centre default boss; each wires as a child of the pre-boss rest on its own side.
            int col = side == FlankSide.Left ? 0 : cols - 1;
            var rest = side == FlankSide.Left ? farLeftRest : farRightRest;

            if (col == defaultCol)
            {
                Invariants.Check(false, "BookOfEndings",
                    $"alt boss col {col} collides with default boss col on a {cols}-wide map; skipping {side}");
                continue;
            }

            var altBoss = new MapPoint(col, bossRow) { PointType = MapPointType.Boss };
            rest.AddChildPoint(altBoss);
            AltBossStore.Register(map, new AltBossNode(altBoss, side, encounterId, rest.coord, IsSecond: false));
            flankNodes.Add((side, encounterId, altBoss));
            injected.Add($"{side}=({col},{bossRow})->{encounterId} from rest ({rest.coord.col},{rest.coord.row})");
        }

        // Double boss (Ascension 10 final act): chain a second boss above each revealed flank. The
        // pairing comes from FlankSeconds, which deranges over EVERY flank the act can have — not the
        // revealed subset — so a flank already on the map never re-chains when the other one is later
        // revealed. The act's own default boss and its second are left exactly as the game generated
        // them: this relic only ever adds.
        if (act.HasSecondBoss)
        {
            var seconds = AltBossPlan.FlankSeconds(flanks.Select(f => f.encounterId).ToList(), defaultId, seed);

            foreach (var f in flankNodes)
            {
                if (!seconds.TryGetValue(f.enc, out var secondEnc)) continue;
                var second = new MapPoint(f.node.coord.col, bossRow + 1) { PointType = MapPointType.Boss };
                f.node.AddChildPoint(second);
                AltBossStore.Register(map, new AltBossNode(second, f.side, secondEnc, f.node.coord, IsSecond: true));
                injected.Add($"{f.side}#2=({second.coord.col},{second.coord.row})->{secondEnc} chained from {f.enc}");
            }

            Invariants.Check(flankNodes.All(f => !seconds.TryGetValue(f.enc, out var s) || s != f.enc),
                "BookOfEndings", "a flank second boss self-pairs (derangement failed)");
        }

        Log.Info($"[BookOfEndings] act {actIndex}: seed {runState.Rng.Seed}->{seed}, default={defaultId} " +
                 $"at col {defaultCol}; studies={studies} consumed={consumed} " +
                 $"bank={AltBossReveal.Bank(studies, consumed)} cap={flanks.Count} revealed={revealed}; " +
                 $"doubleBoss={act.HasSecondBoss}; injected [{string.Join(", ", injected)}]; " +
                 $"GetAllMapPoints now sees {map.GetAllMapPoints().Count()} points");
        return map;
    }

    // While the book is held, offer the one-word "Study" action at every Rest Site. Always added, even
    // when it cannot be used — StudyRestSiteOption.IsEnabled greys it out instead of hiding it, so the
    // player can still hover it and read what the book does.
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner) return false;
        options.Add(new StudyRestSiteOption(player, this));
        return true;
    }

    // A book obtained mid-run must not start with a zeroed party mirror, or the next settle would
    // re-charge acts that were already paid for. Copy the authoritative figures from an existing holder.
    public override async Task AfterObtained()
    {
        await base.AfterObtained();

        var state = Owner?.RunState;
        if (state == null) return;

        var other = Holders(state).FirstOrDefault(h => h != this);
        if (other != null)
        {
            Consumed = other.Consumed;
            LastActIndex = other.LastActIndex;
        }
        // No map regeneration here: the bank is 0 on obtain, so there is nothing to reveal yet.
    }
}
