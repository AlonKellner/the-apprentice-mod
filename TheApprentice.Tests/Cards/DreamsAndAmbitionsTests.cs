using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class DreamsAndAmbitionsTests
{
    [Fact] public void CountDreams_Empty_ReturnsZero() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([]));

    [Fact] public void CountDreams_OneDream_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountDreams([new Dream()]));

    [Fact] public void CountDreams_IgnoresAmbitions() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([new Ambition()]));

    [Fact] public void CountDreams_IgnoresPotentials() =>
        Assert.Equal(0, DreamsAndAmbitions.CountDreams([new Potential()]));

    [Fact] public void CountAmbitions_Empty_ReturnsZero() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAmbitions([]));

    [Fact] public void CountAmbitions_OneAmbition_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountAmbitions([new Ambition()]));

    [Fact] public void CountAmbitions_IgnoresDreams() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAmbitions([new Dream()]));

    [Fact] public void CountAll_MixedDreamsAndAmbitions_ReturnsCombinedCount() =>
        Assert.Equal(3, DreamsAndAmbitions.CountAll([new Dream(), new Ambition(), new Dream()]));

    [Fact] public void CountAll_IgnoresPotentials() =>
        Assert.Equal(0, DreamsAndAmbitions.CountAll([new Potential()]));

    [Fact] public void CountPotentials_OnePotential_ReturnsOne() =>
        Assert.Equal(1, DreamsAndAmbitions.CountPotentials([new Potential()]));

    [Fact] public void CountPotentials_IgnoresDreams() =>
        Assert.Equal(0, DreamsAndAmbitions.CountPotentials([new Dream()]));
}
