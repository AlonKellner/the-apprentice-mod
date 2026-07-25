using System.Linq;
using System.Reflection;
using TheUnderstudy.TheUnderstudyCode.Events;
using Xunit;

namespace TheUnderstudy.Tests.Events;

// Structural checks on the Golden Bedroom event. Its option effects (gold / heal / grant relic) run
// game commands and are verified live in-game via `event THEUNDERSTUDY-GOLDEN_BEDROOM`; the option
// texts are covered by LocFileTests parsing events.json. This guards the three-choice shape: each
// option's loc key and outcome page are derived from its handler method name, so the names matter.
public class GoldenBedroomTests
{
    [Theory]
    [InlineData("TakeGoldenToys")]
    [InlineData("SleepInTheBed")]
    [InlineData("ChaseTheBook")]
    public void HasOptionHandler(string method) =>
        Assert.NotNull(typeof(GoldenBedroom).GetMethod(method,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

    // Exactly the three choices — no stray private async Task option handlers crept in or dropped out.
    [Fact]
    public void HasExactlyThreeOptionHandlers()
    {
        var handlers = typeof(GoldenBedroom)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(System.Threading.Tasks.Task) && !m.IsSpecialName)
            .ToList();
        Assert.Equal(3, handlers.Count);
    }
}
