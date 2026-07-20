using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// The single source of truth for every invertible buff/debuff pair. Everything (Invert, The Second Lesson
// pool, InvertTrackerPower, the tooltip patch) derives from InvertiblePairs.All, so adding a pair here flows
// everywhere. These bare tests pin the registry's structure (membership, kind, Weak/Vulnerable flag); the
// combat-facing methods (Categorize/Apply/Invert over a live Creature) are verified in-game.
public class InvertiblePairsTests
{
    [Fact]
    public void All_ContainsEveryInvertiblePair()
    {
        // 9 same-shape X/Un-X pairs + 3 sign-flip powers = 12.
        Assert.Equal(12, InvertiblePairs.All.Count);
    }

    [Fact]
    public void For_SameShapePairs_MatchBothSides()
    {
        AssertSameShape(new WeakPower(), new UnweakPower());
        AssertSameShape(new VulnerablePower(), new UnvulnerablePower());
        AssertSameShape(new ShakenPower(), new UnshakenPower());
        AssertSameShape(new LimitedPower(), new UnlimitedPower());
        AssertSameShape(new JadedPower(), new UnjadedPower());
        AssertSameShape(new FrailPower(), new UnfrailPower());
        AssertSameShape(new TaintedPower(), new UntaintedPower());
        AssertSameShape(new TensionPower(), new UntensionPower());
        AssertSameShape(new DoomPower(), new UndoomPower());
    }

    [Fact]
    public void For_SignFlipPowers_AreSignFlip()
    {
        foreach (var p in new PowerModel[] { new StrengthPower(), new DexterityPower(), new VigorPower() })
        {
            var pair = InvertiblePairs.For(p);
            Assert.NotNull(pair);
            Assert.False(pair!.IsSameShape);
            Assert.True(pair.Contains(p));
            Assert.False(pair.IsWeakOrVulnerable);
        }
    }

    [Fact]
    public void For_NonInvertiblePower_IsNull()
    {
        Assert.Null(InvertiblePairs.For(new PoisonPower()));
    }

    [Fact]
    public void OnlyWeakAndVulnerablePairs_ReportIsWeakOrVulnerable()
    {
        Assert.True(InvertiblePairs.For(new WeakPower())!.IsWeakOrVulnerable);
        Assert.True(InvertiblePairs.For(new VulnerablePower())!.IsWeakOrVulnerable);
        Assert.False(InvertiblePairs.For(new ShakenPower())!.IsWeakOrVulnerable);
        Assert.False(InvertiblePairs.For(new DoomPower())!.IsWeakOrVulnerable);
    }

    // Both sides resolve to the SAME pair object, that pair is same-shape, and it isn't accidentally sign-flip.
    private static void AssertSameShape(PowerModel debuff, PowerModel buff)
    {
        var fromDebuff = InvertiblePairs.For(debuff);
        var fromBuff = InvertiblePairs.For(buff);
        Assert.NotNull(fromDebuff);
        Assert.Same(fromDebuff, fromBuff);
        Assert.True(fromDebuff!.IsSameShape);
        Assert.True(fromDebuff.Contains(debuff));
        Assert.True(fromDebuff.Contains(buff));
    }
}
