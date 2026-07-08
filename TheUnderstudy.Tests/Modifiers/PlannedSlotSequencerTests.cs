using System.Collections.Generic;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class PlannedSlotSequencerTests
{
    [Fact]
    public void Next_FirstCallForToken_UsesResyncStart()
    {
        var sequencer = new PlannedSlotSequencer();
        var token = new object();

        int result = sequencer.Next(token, () => 5);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Next_SubsequentCallsSameToken_NeverCallsResyncAgain_IncrementsByOne()
    {
        var sequencer = new PlannedSlotSequencer();
        var token = new object();
        int resyncCalls = 0;
        int Resync()
        {
            resyncCalls++;
            return 0;
        }

        int first = sequencer.Next(token, Resync);
        int second = sequencer.Next(token, Resync);
        int third = sequencer.Next(token, Resync);

        Assert.Equal(1, resyncCalls);
        Assert.Equal(new[] { 0, 1, 2 }, new[] { first, second, third });
    }

    [Fact]
    public void Next_NewToken_ResyncsFromNewStartingPoint()
    {
        var sequencer = new PlannedSlotSequencer();
        var tokenA = new object();
        var tokenB = new object();

        int a1 = sequencer.Next(tokenA, () => 0);
        int a2 = sequencer.Next(tokenA, () => 0);
        int b1 = sequencer.Next(tokenB, () => 10);

        Assert.Equal(0, a1);
        Assert.Equal(1, a2);
        Assert.Equal(10, b1);
    }

    [Fact]
    public void Next_ManyCallsSameToken_NeverRepeats()
    {
        var sequencer = new PlannedSlotSequencer();
        var token = new object();
        var seen = new HashSet<int>();

        for (int i = 0; i < 1000; i++)
            Assert.True(seen.Add(sequencer.Next(token, () => 0)));
    }

    [Fact]
    public void Next_ResyncReturnsExistingMax_ContinuesFromThatPointNotZero()
    {
        // Simulates a mid-combat reload: existing cards already hold slots up to 5 (max), so the
        // very next Apply after the token changes must start at 6, not 0.
        var sequencer = new PlannedSlotSequencer();
        var token = new object();

        int result = sequencer.Next(token, () => 6);

        Assert.Equal(6, result);
    }
}
