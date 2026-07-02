using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
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
}
