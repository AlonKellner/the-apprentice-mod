using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Covers only the guard branches of UnderstudyCard.AfterCardPlayed that never reach
// CardModifier.AddModifier<UnplayableModifier>(this) — that generic call requires ModelDb,
// which isn't available in this bare test harness (see AssemblyInfo.cs). The "should attach"
// branch is exercised via TunedModifier.IsFinalTunedPlay in TunedModifierTests.cs, and
// verified end-to-end in-game.
public class UnderstudyCardTests
{
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
    public void AfterCardPlayed_NoTunedModifier_DoesNotThrow_NoUnplayableAdded()
    {
        var card = new UnderstudyStrike();
        card.AfterCardPlayed(null!, MakePlay(card, 0, 1));
        Assert.False(card.TryGetModifier<UnplayableModifier>(out _));
    }

    [Fact]
    public void AfterCardPlayed_TunedNotFinalPlay_DoesNotAddUnplayable()
    {
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new TunedModifier());
        // Replay 1, first of two plays.
        card.AfterCardPlayed(null!, MakePlay(card, 0, 2));
        Assert.False(card.TryGetModifier<UnplayableModifier>(out _));
    }

    [Fact]
    public void AfterCardPlayed_AlreadyUnplayable_DoesNotReAddOrThrow()
    {
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new TunedModifier());
        CardModifier.AddModifier(card, new UnplayableModifier());
        card.AfterCardPlayed(null!, MakePlay(card, 0, 1));
        Assert.Single(CardModifier.DirectModifiers(card), m => m is UnplayableModifier);
    }

    [Fact]
    public void AfterCardPlayed_DifferentCardPlayed_DoesNotAffectThisCard()
    {
        var card = new UnderstudyStrike();
        var otherCard = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        card.AfterCardPlayed(null!, MakePlay(otherCard, 0, 1));
        Assert.False(card.TryGetModifier<UnplayableModifier>(out _));
    }
}
