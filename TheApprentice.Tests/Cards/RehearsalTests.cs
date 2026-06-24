using TheApprentice.TheApprenticeCode.Cards;
using Xunit;

namespace TheApprentice.Tests.Cards;

public class RehearsalTests
{
    [Fact]
    public void Rehearsal_CardId_MatchesExpectedConstant() =>
        Assert.Equal("TheApprentice:Rehearsal", Rehearsal.CardId);
}
