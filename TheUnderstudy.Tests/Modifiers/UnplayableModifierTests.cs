using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class UnplayableModifierTests
{
#pragma warning disable STS001 // test-only stubs — no localization entries needed
    private class UnplayableSkill : UnderstudyCard
    {
        public UnplayableSkill()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
        {
            WithKeyword(CardKeyword.Unplayable, ConstructedCardModel.UpgradeType.None);
        }
    }

    private class PlayableSkill : UnderstudyCard
    {
        public PlayableSkill()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.None) { }
    }
#pragma warning restore STS001

    [Fact]
    public void AnyIn_EmptyInput_ReturnsFalse()
    {
        Assert.False(UnplayableModifier.AnyIn(System.Array.Empty<CardModel>()));
    }

    [Fact]
    public void AnyIn_NoUnplayableCards_ReturnsFalse()
    {
        Assert.False(UnplayableModifier.AnyIn(new CardModel[] { new PlayableSkill(), new PlayableSkill() }));
    }

    [Fact]
    public void AnyIn_OneUnplayableCard_ReturnsTrue()
    {
        Assert.True(UnplayableModifier.AnyIn(new CardModel[] { new PlayableSkill(), new UnplayableSkill() }));
    }
}
