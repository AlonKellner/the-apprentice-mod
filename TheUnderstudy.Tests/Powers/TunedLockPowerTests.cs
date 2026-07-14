using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using Xunit;

namespace TheUnderstudy.Tests.Powers;

// Covers the pure decision seam TunedLockPower.ShouldLock — the part of the Tuned->Unplayable lock
// that used to live (Understudy-only) in UnderstudyCard.AfterCardPlayed and now runs for EVERY played
// card (colorless included) via the hidden TunedLockPower. The attach itself (AddModifier<T>) needs a
// populated ModelDb and the in-combat hook (AfterCardPlayedLate) needs combat state, so both are
// verified in-game per the repo's no-combat-harness convention; here we drive the pure predicate on
// bare cards, exactly like TunedModifierTests / UnplayableModifierTests do.
public class TunedLockPowerTests
{
#pragma warning disable STS001 // test-only stub — no localization entry needed
    // A plain Skill with no card-specific logic. Stands in for any card that can receive Tuned,
    // Understudy or not — LockIfFinalTunedPlay must not care which concrete class it is.
    private class PlainSkill : UnderstudyCard
    {
        public PlainSkill()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.None) { }
    }
#pragma warning restore STS001

    private static CardPlay MakePlay(CardModel card, int index, int count) => new()
    {
        Card = card,
        Target = null,
        ResultPile = PileType.Discard,
        Resources = default,
        IsAutoPlay = false,
        PlayIndex = index,
        PlayCount = count,
    };

    [Fact]
    public void ShouldLock_LocksAnyTunedCardOnFinalPlay()
    {
        var card = new PlainSkill();
        CardModifier.AddModifier(card, new TunedModifier());
        var play = MakePlay(card, index: 0, count: 1); // single, final play

        // The card type is irrelevant — this is what makes colorless Tuned cards lock too.
        Assert.True(TunedLockPower.ShouldLock(card, play));
    }

    [Fact]
    public void ShouldLock_FalseMidReplaySeries()
    {
        var card = new PlainSkill();
        CardModifier.AddModifier(card, new TunedModifier());
        var play = MakePlay(card, index: 0, count: 2); // first of two plays — not final

        Assert.False(TunedLockPower.ShouldLock(card, play));
    }

    [Fact]
    public void ShouldLock_FalseForUntunedCard()
    {
        var card = new PlainSkill(); // no TunedModifier
        var play = MakePlay(card, index: 0, count: 1);

        Assert.False(TunedLockPower.ShouldLock(card, play));
    }

    [Fact]
    public void ShouldLock_FalseWhenAlreadyUnplayable()
    {
        var card = new PlainSkill();
        CardModifier.AddModifier(card, new TunedModifier());
        CardModifier.AddModifier(card, new UnplayableModifier()); // already locked

        // The guard suppresses a redundant re-lock (and the Applied event it would fire).
        Assert.False(TunedLockPower.ShouldLock(card, MakePlay(card, index: 0, count: 1)));
    }

    [Fact]
    public void TunedLockPower_OverridesAfterCardPlayedLate()
    {
        Assert.NotNull(typeof(TunedLockPower).GetMethod(
            "AfterCardPlayedLate",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
    }
}
