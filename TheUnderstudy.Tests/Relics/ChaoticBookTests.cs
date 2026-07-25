using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

// Structural checks on the Chaotic Book / Book of Order quest pair — the SwordOfStone shape. Runtime
// behaviour (Study advancing the counter, the transform, the map effect) is verified live in-game.
public class ChaoticBookTests
{
    [Fact]
    public void ChaoticBook_IsEventRarity() =>
        Assert.Equal(RelicRarity.Event, new ChaoticBook().Rarity);

    [Fact]
    public void BookOfOrder_IsEventRarity() =>
        Assert.Equal(RelicRarity.Event, new BookOfOrder().Rarity);

    [Fact]
    public void ChaoticBook_ShowsACounter() =>
        Assert.True(new ChaoticBook().ShowCounter);

    // The counter shows studies remaining, driven by the pure StudyProgress helper.
    [Fact]
    public void ChaoticBook_DisplayAmount_StartsAtThreshold() =>
        Assert.Equal(ChaoticBook.StudiesRequired, new ChaoticBook().DisplayAmount);

    // StudyCount must persist across save/load, so it carries the base game's [SavedProperty].
    [Fact]
    public void ChaoticBook_StudyCount_IsSavedProperty()
    {
        var prop = typeof(ChaoticBook).GetProperty(nameof(ChaoticBook.StudyCount));
        Assert.NotNull(prop);
        Assert.Contains(prop!.GetCustomAttributes(inherit: true),
            a => a.GetType().Name == "SavedPropertyAttribute");
    }

    // Both forms are event-only: they carry [Pool] (so the STS004 analyzer is satisfied) but are listed
    // in the pool's EventOnlyRelics so GetUnlockedRelics removes them — they never drop as a reward.
    [Fact]
    public void QuestRelics_AreExcludedFromRewardsButKeepPoolAttribute()
    {
        foreach (var t in new[] { typeof(ChaoticBook), typeof(BookOfOrder) })
        {
            Assert.Contains(t.GetCustomAttributes(inherit: true), a => a.GetType().Name == "PoolAttribute");
            Assert.Contains(t, TheUnderstudy.TheUnderstudyCode.Character.TheUnderstudyRelicPool.EventOnlyRelics);
        }
    }
}
