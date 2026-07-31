using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

// Structural checks on the Book of Endings — the single relic that replaced the Chaotic Book / Book of
// Order quest pair. Bare instantiation only: the interesting behaviour (Study revealing a boss, the map
// injection, the rest-site button greying out) needs a live run and ModelDb, so it is verified in-game.
// The bank arithmetic those parts are built on is covered by AltBossRevealTests instead.
public class BookOfEndingsTests
{
    [Fact]
    public void IsEventRarity() =>
        Assert.Equal(RelicRarity.Event, new BookOfEndings().Rarity);

    // The counter ring shows the study bank, so it needs to be on.
    [Fact]
    public void ShowsACounter() =>
        Assert.True(new BookOfEndings().ShowCounter);

    // Outside a run — the compendium and the relic library both read this — there is no RunManager to
    // ask, so the counter must fall back to 0 rather than throwing.
    [Fact]
    public void DisplayAmount_OutsideARun_IsZero() =>
        Assert.Equal(0, new BookOfEndings().DisplayAmount);

    // All three must survive save/load: the bank is the one part of this mechanic that cannot be
    // re-derived from the run seed, unlike everything in AltBossPlan.
    [Theory]
    [InlineData(nameof(BookOfEndings.StudyCount))]
    [InlineData(nameof(BookOfEndings.Consumed))]
    [InlineData(nameof(BookOfEndings.LastActIndex))]
    public void BankState_IsSavedProperty(string propertyName)
    {
        var prop = typeof(BookOfEndings).GetProperty(propertyName);
        Assert.NotNull(prop);
        Assert.Contains(prop!.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == "SavedPropertyAttribute");
    }

    // -1 rather than 0, so act 0 is never mistaken for "already settled" on a fresh relic.
    [Fact]
    public void LastActIndex_StartsBeforeAnyAct() =>
        Assert.Equal(-1, new BookOfEndings().LastActIndex);

    // Never a random reward — but for the right reason: Event rarity is what excludes it (every reward
    // path rarity-filters to Common/Uncommon/Rare/Shop), NOT a pool exclusion. It keeps [Pool] (so the
    // STS004 analyzer is satisfied) and stays in the pool's unlocked set, so the compendium can show it as
    // a hidden "unknown" tile that becomes visible once The Golden Bedroom grants it.
    [Fact]
    public void IsExcludedFromRewardsByRarityButKeepsPoolAttribute()
    {
        Assert.Contains(typeof(BookOfEndings).GetCustomAttributes(inherit: true),
            a => a.GetType().Name == "PoolAttribute");
        Assert.Equal(RelicRarity.Event, new BookOfEndings().Rarity);
    }
}
