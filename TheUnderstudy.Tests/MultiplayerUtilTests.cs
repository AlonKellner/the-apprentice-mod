using TheUnderstudy.TheUnderstudyCode.Extensions;
using Xunit;

namespace TheUnderstudy.Tests;

// Pure core of the local-player gate used to keep client-local UI side effects (Planned badge arming)
// from firing on remote players' plays. IsLocalPlayer itself calls the platform layer (unavailable in the
// bare test host), so only the id comparison is unit-tested here.
public class MultiplayerUtilTests
{
    [Fact]
    public void IsLocalPlayerId_MatchingIds_True() =>
        Assert.True(MultiplayerUtil.IsLocalPlayerId(ownerNetId: 76561198000000001, localPlayerId: 76561198000000001));

    [Fact]
    public void IsLocalPlayerId_DifferentIds_False() =>
        Assert.False(MultiplayerUtil.IsLocalPlayerId(ownerNetId: 76561198000000002, localPlayerId: 76561198000000001));

    [Fact]
    public void IsLocalPlayerId_ZeroIds_TreatedAsMatch() =>
        // Single-player / uninitialised platform can yield 0 for both; equal ids are "local".
        Assert.True(MultiplayerUtil.IsLocalPlayerId(ownerNetId: 0, localPlayerId: 0));
}
