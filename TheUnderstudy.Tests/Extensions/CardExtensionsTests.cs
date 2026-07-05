using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Xunit;

namespace TheUnderstudy.Tests.Extensions;

public class CardExtensionsTests
{
#pragma warning disable STS001 // test-only stubs — no localization entries needed
    private class NativelyUnplayableCard : UnderstudyCard
    {
        public NativelyUnplayableCard()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
        {
            WithKeyword(CardKeyword.Unplayable, ConstructedCardModel.UpgradeType.None);
        }
    }

    private class PlayableCard : UnderstudyCard
    {
        public PlayableCard()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.None) { }
    }
#pragma warning restore STS001

    [Fact]
    public void IsUnplayable_NativeUnplayableKeyword_ReturnsTrue()
    {
        Assert.True(new NativelyUnplayableCard().IsUnplayable());
    }

    [Fact]
    public void IsUnplayable_CardWithoutUnplayable_ReturnsFalse()
    {
        Assert.False(new PlayableCard().IsUnplayable());
    }

    // IsFunctionallyUnplayableReason — pure bitmask predicate, no Card/CombatState needed.

    [Fact]
    public void IsFunctionallyUnplayableReason_None_ReturnsFalse() =>
        Assert.False(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.None));

    [Fact]
    public void IsFunctionallyUnplayableReason_EnergyCostTooHigh_ReturnsFalse() =>
        Assert.False(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.EnergyCostTooHigh));

    [Fact]
    public void IsFunctionallyUnplayableReason_StarCostTooHigh_ReturnsFalse() =>
        Assert.False(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.StarCostTooHigh));

    [Fact]
    public void IsFunctionallyUnplayableReason_BothCostReasons_ReturnsFalse() =>
        Assert.False(CardExtensions.IsFunctionallyUnplayableReason(
            UnplayableReason.EnergyCostTooHigh | UnplayableReason.StarCostTooHigh));

    [Fact]
    public void IsFunctionallyUnplayableReason_HasUnplayableKeyword_ReturnsTrue() =>
        Assert.True(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.HasUnplayableKeyword));

    [Fact]
    public void IsFunctionallyUnplayableReason_BlockedByHook_ReturnsTrue() =>
        Assert.True(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.BlockedByHook));

    [Fact]
    public void IsFunctionallyUnplayableReason_BlockedByCardLogic_ReturnsTrue() =>
        Assert.True(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.BlockedByCardLogic));

    [Fact]
    public void IsFunctionallyUnplayableReason_NoLivingAllies_ReturnsTrue() =>
        Assert.True(CardExtensions.IsFunctionallyUnplayableReason(UnplayableReason.NoLivingAllies));

    [Fact]
    public void IsFunctionallyUnplayableReason_CostReasonPlusHookReason_ReturnsTrue() =>
        Assert.True(CardExtensions.IsFunctionallyUnplayableReason(
            UnplayableReason.EnergyCostTooHigh | UnplayableReason.BlockedByHook));

    // AnyUnplayable — broader than UnplayableModifier.AnyIn (any card type, matching
    // TakeYourBow's existing damage-counting scope).

    [Fact]
    public void AnyUnplayable_EmptyInput_ReturnsFalse() =>
        Assert.False(CardExtensions.AnyUnplayable(System.Array.Empty<CardModel>()));

    [Fact]
    public void AnyUnplayable_NoUnplayableCards_ReturnsFalse() =>
        Assert.False(CardExtensions.AnyUnplayable(new CardModel[] { new PlayableCard(), new PlayableCard() }));

    [Fact]
    public void AnyUnplayable_OneUnplayableCard_ReturnsTrue() =>
        Assert.True(CardExtensions.AnyUnplayable(new CardModel[] { new PlayableCard(), new NativelyUnplayableCard() }));
}
