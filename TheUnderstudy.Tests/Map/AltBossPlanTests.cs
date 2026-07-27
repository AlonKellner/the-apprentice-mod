using System.Collections.Generic;
using System.Linq;
using TheUnderstudy.TheUnderstudyCode.Map;
using Xunit;

namespace TheUnderstudy.Tests.Map;

// The pure planning behind the Book of Order's Alternative Bosses. Determinism is the whole safety
// story here — the same seed must produce the same left/right assignment on fresh generation, on load,
// and on every co-op client — so these tests hammer determinism, coverage, and the no-self-pairing
// property. The game-side injection/rendering/travel is verified live in-game (no map test harness).
public class AltBossPlanTests
{
    private static readonly string[] ThreeBosses = { "BOSS_A", "BOSS_B", "BOSS_C" };

    [Theory]
    [InlineData(12345UL, 0)]
    [InlineData(12345UL, 1)]
    [InlineData(0UL, 2)]
    [InlineData(9999999999UL, 3)]
    public void SeedFor_IsDeterministic(ulong runSeed, int actIndex) =>
        Assert.Equal(AltBossPlan.SeedFor(runSeed, actIndex), AltBossPlan.SeedFor(runSeed, actIndex));

    [Fact]
    public void SeedFor_DiffersAcrossActs()
    {
        // Adjacent acts of the same run must not collide, or every act would flank identically.
        var seeds = Enumerable.Range(0, 4).Select(i => AltBossPlan.SeedFor(777UL, i)).ToList();
        Assert.Equal(seeds.Count, seeds.Distinct().Count());
    }

    [Fact]
    public void AssignFlanks_ThreeBosses_PlacesBothOthersOnePerFlank()
    {
        var seed = AltBossPlan.SeedFor(42UL, 0);
        var flanks = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", seed);

        Assert.Equal(2, flanks.Count);
        Assert.Contains(FlankSide.Left, flanks.Select(f => f.side));
        Assert.Contains(FlankSide.Right, flanks.Select(f => f.side));
        // The default boss is never placed on a flank (it keeps its own node); the two others are.
        Assert.DoesNotContain("BOSS_A", flanks.Select(f => f.encounterId));
        Assert.Equal(new[] { "BOSS_B", "BOSS_C" }, flanks.Select(f => f.encounterId).OrderBy(x => x));
    }

    [Fact]
    public void AssignFlanks_AllThreeBossesReachable()
    {
        // default node + the two flank encounters must together cover the whole boss pool.
        var seed = AltBossPlan.SeedFor(42UL, 0);
        var flanks = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", seed);
        var reachable = new HashSet<string>(flanks.Select(f => f.encounterId)) { "BOSS_A" };
        Assert.Equal(new HashSet<string>(ThreeBosses), reachable);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    [InlineData(2UL)]
    [InlineData(0xFFFFFFFFFFFFFFFFUL)]
    public void AssignFlanks_IsDeterministicAndBijectiveOnSides(ulong seed)
    {
        var first = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", seed);
        var second = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", seed);
        Assert.Equal(first, second);
        // Exactly one Left and one Right, exactly the two non-default bosses.
        Assert.Single(first, f => f.side == FlankSide.Left);
        Assert.Single(first, f => f.side == FlankSide.Right);
    }

    [Fact]
    public void AssignFlanks_SeedBitControlsSide()
    {
        // Two seeds differing only in the low bit swap the two bosses' sides.
        var evenSeed = 0UL;
        var oddSeed = 1UL;
        var even = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", evenSeed);
        var odd = AltBossPlan.AssignFlanks(ThreeBosses, "BOSS_A", oddSeed);
        var evenLeft = even.First(f => f.side == FlankSide.Left).encounterId;
        var oddLeft = odd.First(f => f.side == FlankSide.Left).encounterId;
        Assert.NotEqual(evenLeft, oddLeft);
    }

    [Fact]
    public void AssignFlanks_TwoBosses_PlacesSingleOtherOnOneFlank()
    {
        var flanks = AltBossPlan.AssignFlanks(new[] { "BOSS_A", "BOSS_B" }, "BOSS_A", 0UL);
        Assert.Single(flanks);
        Assert.Equal("BOSS_B", flanks[0].encounterId);
    }

    [Fact]
    public void AssignFlanks_OneBoss_PlacesNothing()
    {
        var flanks = AltBossPlan.AssignFlanks(new[] { "BOSS_A" }, "BOSS_A", 123UL);
        Assert.Empty(flanks);
    }

    [Fact]
    public void AssignFlanks_FourBosses_PicksTwoDeterministically()
    {
        var flanks = AltBossPlan.AssignFlanks(new[] { "A", "B", "C", "D" }, "A", 0UL);
        Assert.Equal(2, flanks.Count);
        // First two non-default in stable order.
        Assert.Equal(new[] { "B", "C" }, flanks.Select(f => f.encounterId).OrderBy(x => x));
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    public void Derange_HasNoFixedPoint(ulong seed)
    {
        var map = AltBossPlan.Derange(ThreeBosses, seed);
        Assert.Equal(3, map.Count);
        foreach (var kv in map)
            Assert.NotEqual(kv.Key, kv.Value); // no boss chains into itself
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    public void Derange_IsAPermutation(ulong seed)
    {
        var map = AltBossPlan.Derange(ThreeBosses, seed);
        // Every boss appears exactly once as a second boss.
        Assert.Equal(new HashSet<string>(ThreeBosses), new HashSet<string>(map.Values));
    }

    [Fact]
    public void Derange_IsDeterministic()
    {
        var seed = AltBossPlan.SeedFor(555UL, 2);
        Assert.Equal(AltBossPlan.Derange(ThreeBosses, seed), AltBossPlan.Derange(ThreeBosses, seed));
    }

    [Fact]
    public void Derange_SingleBoss_ReturnsEmpty() =>
        Assert.Empty(AltBossPlan.Derange(new[] { "ONLY" }, 0UL));

    // ── FlankSeconds: the act's boss layout must be settled before anything is revealed ──────────────
    // The Book of Endings reveals its alternative bosses one at a time, so the double-boss chaining
    // cannot be derived from "the bosses reachable so far" — that set grows as you Study, and every
    // pairing would shift under the player. FlankSeconds deranges over the FULL potential set instead,
    // so the answer is the same whether zero, one or two flanks are currently on the map.

    private const string DefaultBoss = "BOSS_A";
    private static readonly string[] BothFlanks = { "BOSS_B", "BOSS_C" };

    // The regression test for exactly that defect. Shrinking the input set changes the answer for a flank
    // that is already on the map — which is what the old "derange over the bosses reachable so far" code
    // did every time a second flank appeared. So the caller MUST pass every flank the act can ever have,
    // never the revealed subset; this test fails the moment someone "optimises" that back.
    [Fact]
    public void FlankSeconds_OverTheRevealedSubset_WouldRechainAnAlreadyShownFlank()
    {
        var seed = AltBossPlan.SeedFor(1234UL, 2) & ~1UL; // even: forward rotation, where the two differ

        var fullSet = AltBossPlan.FlankSeconds(BothFlanks, DefaultBoss, seed);
        var revealedSubsetOnly = AltBossPlan.FlankSeconds(new[] { BothFlanks[0] }, DefaultBoss, seed);

        Assert.NotEqual(fullSet[BothFlanks[0]], revealedSubsetOnly[BothFlanks[0]]);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    public void FlankSeconds_NeverChainsAFlankIntoItself(ulong seed)
    {
        foreach (var kv in AltBossPlan.FlankSeconds(BothFlanks, DefaultBoss, seed))
            Assert.NotEqual(kv.Key, kv.Value);
    }

    // The relic is purely additive: it assigns seconds to the flanks it injects and never reassigns the
    // act's own default boss, which the game already decided.
    [Theory]
    [InlineData(0UL)]
    [InlineData(1UL)]
    public void FlankSeconds_LeavesTheDefaultBossAlone(ulong seed)
    {
        var seconds = AltBossPlan.FlankSeconds(BothFlanks, DefaultBoss, seed);

        Assert.DoesNotContain(DefaultBoss, seconds.Keys);
        Assert.Equal(new HashSet<string>(BothFlanks), new HashSet<string>(seconds.Keys));
    }

    [Fact]
    public void FlankSeconds_IsDeterministic()
    {
        var seed = AltBossPlan.SeedFor(4242UL, 2);
        Assert.Equal(
            AltBossPlan.FlankSeconds(BothFlanks, DefaultBoss, seed),
            AltBossPlan.FlankSeconds(BothFlanks, DefaultBoss, seed));
    }

    // A 2-boss act has a single alternative; there is nothing to derange it against but the default.
    [Fact]
    public void FlankSeconds_SingleFlank_ChainsIntoTheDefault()
    {
        var seconds = AltBossPlan.FlankSeconds(new[] { "BOSS_B" }, DefaultBoss, 0UL);

        Assert.Equal(DefaultBoss, seconds["BOSS_B"]);
    }

    [Fact]
    public void FlankSeconds_NoFlanks_ReturnsEmpty() =>
        Assert.Empty(AltBossPlan.FlankSeconds(System.Array.Empty<string>(), DefaultBoss, 0UL));
}
