using TheUnderstudy.TheUnderstudyCode.Relics;
using Xunit;

namespace TheUnderstudy.Tests.Relics;

public class UnderstudyStarterRelicTests
{
    [Theory]
    [InlineData(5, 0, 5)]
    [InlineData(5, 1, 4)]
    [InlineData(5, 2, 3)]
    [InlineData(1, 2, 0)]
    public void ComputeModifiedHandDraw_SubtractsGrantedCount(decimal natural, int granted, decimal expected) =>
        Assert.Equal(expected, UnderstudyStarterRelic.ComputeModifiedHandDraw(natural, granted));
}
