using BaseLib.Abstracts;
using BaseLib.Extensions;
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

    // The Muscle Memory immunity guard in OnInitialApplication must only block when the card is Tuned
    // AND its owner has Muscle Memory. With no owner/creature (bare card), IsActive(null) is false, so
    // the modifier must attach normally and raise Applied — i.e. the guard never over-blocks. The
    // positive immune path needs a live creature carrying Muscle Memory and is verified in-game.
    private static bool AttachUnplayableAndDidItFireApplied(CardModel card)
    {
        bool fired = false;
        void Handler(CardModel c) { if (c == card) fired = true; }
        UnplayableModifier.Applied += Handler;
        try { CardModifier.AddModifier(card, new UnplayableModifier()); }
        finally { UnplayableModifier.Applied -= Handler; }
        return fired;
    }

    [Fact]
    public void OnInitialApplication_NonTunedCard_Attaches_AndFiresApplied()
    {
        var card = new PlayableSkill();
        Assert.True(AttachUnplayableAndDidItFireApplied(card));
        Assert.True(card.TryGetModifier<UnplayableModifier>(out _));
    }

    [Fact]
    public void OnInitialApplication_TunedCard_NoMuscleMemoryCreature_StillAttaches_AndFiresApplied()
    {
        var card = new PlayableSkill();
        CardModifier.AddModifier(card, new TunedModifier()); // Tuned, but bare card has no creature/power
        Assert.True(AttachUnplayableAndDidItFireApplied(card));
        Assert.True(card.TryGetModifier<UnplayableModifier>(out _));
    }
}
