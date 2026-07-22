using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

public class TunedModifierTests
{
    private static CardPlay MakePlay(CardModel card, int index, int count) => new()
    {
        Card = card,
        Player = null!,  // bare test: no combat player; none of the exercised guard paths read it
        Target = null,
        ResultPile = PileType.Discard,
        Resources = default,
        IsAutoPlay = false,
        PlayIndex = index,
        PlayCount = count,
    };

    [Fact]
    public void ModifierId_IsExpected()
    {
        Assert.Equal("TheUnderstudy:Tuned", TunedModifier.ModifierId);
    }

    [Fact]
    public void CanApplyTo_UnderstudyStrike_ReturnsTrue()
    {
        // UnderstudyStrike uses WithDamage → DamageVar with ValueProp.Move (powered).
        Assert.True(TunedModifier.CanApplyTo(new UnderstudyStrike()));
    }

    [Fact]
    public void CanApplyTo_UnderstudyDefend_ReturnsTrue()
    {
        // UnderstudyDefend uses WithBlock → BlockVar with ValueProp.Move (powered).
        Assert.True(TunedModifier.CanApplyTo(new UnderstudyDefend()));
    }

    [Fact]
    public void CanApplyTo_Performance_ReturnsFalse()
    {
        // Workshop has no Damage or Block DynamicVar — Tuned cannot benefit it.
        Assert.False(TunedModifier.CanApplyTo(new Workshop()));
    }

    [Fact]
    public void CanApplyTo_Buildup_ReturnsFalse()
    {
        // Practice has no Damage or Block DynamicVar — Tuned cannot benefit it.
        Assert.False(TunedModifier.CanApplyTo(new Practice()));
    }

    [Fact]
    public void CanApplyTo_EverythingIveGot_ReturnsTrueAfterGeneralization()
    {
        // OwnIt is a non-Stable Skill with no Damage/Block DynamicVar. Tuned now
        // applies to any Attack/Skill (matching PlannedModifier/UnplayableModifier's own
        // eligibility check), not just cards that print damage or block — a pure-utility Skill
        // still gets the "becomes Unplayable when played" behavior, just no numeric bonus.
        Assert.True(TunedModifier.CanApplyTo(new OwnIt()));
    }

    [Fact]
    public void CanApplyTo_AlreadyTunedCard_ReturnsTrue()
    {
        // Tuned stacks: re-applying to a card that already has Tuned adds another stack.
        var card = new UnderstudyStrike();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        Assert.True(TunedModifier.CanApplyTo(card));
    }

    [Fact]
    public void CanApplyTo_IsStaticMethod()
    {
        var method = typeof(TunedModifier).GetMethod(
            "CanApplyTo", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Apply_IsStaticMethod()
    {
        var method = typeof(TunedModifier).GetMethod(
            "Apply", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void TunedField_ExistsOnUnderstudyKeywords()
    {
        var field = typeof(UnderstudyKeywords).GetField("Tuned");
        Assert.NotNull(field);
    }

    [Fact]
    public void Stacks_DefaultsToZero()
    {
        var mod = new TunedModifier();
        Assert.Equal(0, mod.Stacks);
    }

    [Fact]
    public void IsFinalTunedPlay_NoTunedModifier_SinglePlay_ReturnsFalse()
    {
        var card = new UnderstudyStrike();
        Assert.False(TunedModifier.IsFinalTunedPlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalTunedPlay_HasTuned_SinglePlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        Assert.True(TunedModifier.IsFinalTunedPlay(MakePlay(card, 0, 1)));
    }

    [Fact]
    public void IsFinalTunedPlay_HasTuned_Replay1_FirstPlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        // Replay 1 -> PlayCount = 2; first play is PlayIndex 0.
        Assert.False(TunedModifier.IsFinalTunedPlay(MakePlay(card, 0, 2)));
    }

    [Fact]
    public void IsFinalTunedPlay_HasTuned_Replay1_SecondPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        Assert.True(TunedModifier.IsFinalTunedPlay(MakePlay(card, 1, 2)));
    }

    [Fact]
    public void IsFinalTunedPlay_HasTuned_Replay2_MiddlePlay_ReturnsFalse()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        // Replay 2 -> PlayCount = 3; middle play is PlayIndex 1.
        Assert.False(TunedModifier.IsFinalTunedPlay(MakePlay(card, 1, 3)));
    }

    [Fact]
    public void IsFinalTunedPlay_HasTuned_Replay2_FinalPlay_ReturnsTrue()
    {
        var card = new UnderstudyDefend();
        CardModifier.AddModifier(card, new TunedModifier());
        Assert.True(TunedModifier.IsFinalTunedPlay(MakePlay(card, 2, 3)));
    }

    [Fact]
    public void IsFinalTunedPlay_GrantedAfterOwnCheckThisSamePlay_ReturnsFalse()
    {
        // Da Capo's case: Tuned was granted after this exact play's own attack, so it
        // shouldn't lock the card up for THIS play.
        var card = new UnderstudyStrike();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        var play = MakePlay(card, 0, 1);
        typeof(TunedModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, play);
        Assert.False(TunedModifier.IsFinalTunedPlay(play));
    }

    [Fact]
    public void IsFinalTunedPlay_GrantedAfterOwnCheckOnAnEarlierPlay_ReturnsTrue()
    {
        // The next time the card is played, Tuned was already active before this new play's
        // own check ran, so the normal locking rule applies again.
        var card = new UnderstudyStrike();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        var firstPlay = MakePlay(card, 0, 1);
        typeof(TunedModifier)
            .GetField("_grantedAfterOwnCheckDuringPlay", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(mod, firstPlay);
        var secondPlay = MakePlay(card, 0, 1);
        Assert.True(TunedModifier.IsFinalTunedPlay(secondPlay));
    }

    // ── Damage/block delivery via BaseLib's card-modifier ModifyBase* contract ───────────────
    // Tuned must deliver its bonus through CardModifier.ModifyBaseDamageAdditive /
    // ModifyBaseBlockAdditive (invoked directly on cardSource.GetModifiers() by a Harmony patch),
    // NOT the game's 5-arg AbstractModel.ModifyDamageAdditive/ModifyBlockAdditive hooks — those only
    // reach card modifiers through the version-fragile run-state hook-listener chain, which is why
    // damage silently stopped applying on some players' installs while block kept working.

    private static void SetStacks(TunedModifier mod, int stacks) =>
        typeof(TunedModifier).GetProperty(nameof(TunedModifier.Stacks))!.SetValue(mod, stacks);

    // TunedCardCount reads the owning player's live combat piles, which the bare test host cannot
    // stand up. This stand-in supplies the count directly. Subclassing is safe: BaseLib's
    // GetModifier<T> matches with OfType<T>, so this is still found as a TunedModifier.
    private sealed class TunedWithCardCount : TunedModifier
    {
        private readonly int _count;
        public TunedWithCardCount(int count) => _count = count;
        protected override int TunedCardCount() => _count;
    }

    [Fact]
    public void ModifyBaseDamageAdditive_IsOverride()
    {
        var method = typeof(TunedModifier).GetMethod(nameof(TunedModifier.ModifyBaseDamageAdditive));
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(TunedModifier));
    }

    [Fact]
    public void ModifyBaseBlockAdditive_IsOverride()
    {
        var method = typeof(TunedModifier).GetMethod(nameof(TunedModifier.ModifyBaseBlockAdditive));
        Assert.NotNull(method);
        Assert.True(method!.DeclaringType == typeof(TunedModifier));
    }

    [Fact]
    public void Bonus_IsStacksTimesTunedCardCount()
    {
        var mod = new TunedWithCardCount(3);
        SetStacks(mod, 2);
        Assert.Equal(6, mod.Bonus);
    }

    [Fact]
    public void Bonus_UnattachedModifier_IsZero()
    {
        // No owning card means no piles to count, so a modifier that isn't on a card in combat
        // contributes nothing rather than throwing (CardModel.Owner asserts mutability).
        var mod = new TunedModifier();
        SetStacks(mod, 2);
        Assert.Equal(0, mod.Bonus);
    }

    [Fact]
    public void ModifyBaseDamageAdditive_PoweredAttack_ReturnsStacksScaledBonus()
    {
        var mod = new TunedWithCardCount(3);
        SetStacks(mod, 2); // bonus = Stacks * Tuned card count = 6
        // ValueProp.Move (no Unpowered) is a powered attack.
        Assert.Equal(6m, mod.ModifyBaseDamageAdditive(10m, ValueProp.Move));
    }

    [Fact]
    public void ModifyBaseDamageAdditive_UnpoweredProps_ReturnsZero()
    {
        var mod = new TunedWithCardCount(3);
        SetStacks(mod, 2);
        // Move but Unpowered (e.g. relic/power/potion damage) gets no Tuned bonus.
        Assert.Equal(0m, mod.ModifyBaseDamageAdditive(10m, ValueProp.Move | ValueProp.Unpowered));
    }

    [Fact]
    public void ModifyBaseBlockAdditive_ReturnsStacksScaledBonus()
    {
        var mod = new TunedWithCardCount(3);
        SetStacks(mod, 2); // bonus = 6
        Assert.Equal(6m, mod.ModifyBaseBlockAdditive(5m));
    }

    [Fact]
    public void DoubleStacks_StacksOne_BecomesTwo()
    {
        var card = new UnderstudyStrike();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        typeof(TunedModifier).GetProperty(nameof(TunedModifier.Stacks))!.SetValue(mod, 1);
        TunedModifier.DoubleStacks(card);
        Assert.Equal(2, mod.Stacks);
    }

    [Fact]
    public void DoubleStacks_StacksThree_BecomesSix()
    {
        var card = new UnderstudyStrike();
        var mod = new TunedModifier();
        CardModifier.AddModifier(card, mod);
        typeof(TunedModifier).GetProperty(nameof(TunedModifier.Stacks))!.SetValue(mod, 3);
        TunedModifier.DoubleStacks(card);
        Assert.Equal(6, mod.Stacks);
    }

    [Fact]
    public void DoubleStacks_NoTunedModifier_NoOp()
    {
        var card = new UnderstudyStrike();
        TunedModifier.DoubleStacks(card); // must not throw
        Assert.False(card.TryGetModifier<TunedModifier>(out _));
    }

    [Fact]
    public void ModifyDescription_ShowsStackCount_BeforeDescription()
    {
        var mod = new TunedModifier();
        typeof(TunedModifier).GetProperty(nameof(TunedModifier.Stacks))!.SetValue(mod, 2);
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.StartsWith("[gold]Tuned 2[/gold].", description);
        Assert.Contains("Deal 6 damage.", description);
    }

    [Fact]
    public void ModifyDescription_NoStacks_DoesNotModify()
    {
        var mod = new TunedModifier();
        string description = "Deal 6 damage.";
        mod.ModifyDescription(null, ref description);
        Assert.Equal("Deal 6 damage.", description);
    }
}
