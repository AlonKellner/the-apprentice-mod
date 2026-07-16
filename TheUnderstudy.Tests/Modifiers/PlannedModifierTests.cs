using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Xunit;

namespace TheUnderstudy.Tests.Modifiers;

// Tests requiring CardModifier.AddModifier<T>() (which queries ModelDb) cannot run
// without full game initialization and are omitted. The behaviors they cover
// (PlannedModifier.Apply sequence index logic, TryModifyKeywordsInCombat) are
// verified in-game.
public class PlannedModifierTests
{
    [Fact]
    public void ModifierIdConstant_IsExpected()
    {
        Assert.Equal("TheUnderstudy:Planned", PlannedModifier.ModifierId);
    }

    [Fact]
    public void SequenceIndices_DefaultsToEmpty()
    {
        var mod = new PlannedModifier();
        Assert.Empty(mod.SequenceIndices);
    }

    [Fact]
    public void SequenceIndices_CanAddEntry()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(2);
        Assert.Equal(2, mod.SequenceIndices[0]);
    }

    [Fact]
    public void ModifyDescriptionPost_UsesGoldFormatting()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        var args = new object?[] { null, "base" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("[gold]Planned #", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsOneBased_Index()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(2);
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#3", (string)args[1]!);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsBothSlots_WhenCardIsMultiPlanned()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(3);
        mod.SequenceIndices.Add(7);
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        var desc = (string)args[1]!;
        Assert.Contains("#4", desc);
        Assert.Contains("#8", desc);
    }

    [Fact]
    public void TryModifyKeywordsInCombat_PlannedField_Exists()
    {
        var field = typeof(TheUnderstudy.TheUnderstudyCode.Cards.UnderstudyKeywords).GetField("Planned");
        Assert.NotNull(field);
    }

    [Fact]
    public void CanApplyTo_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "CanApplyTo", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void CanApplyTo_AttackCard_ReturnsTrue()
    {
        Assert.True(PlannedModifier.CanApplyTo(new UnderstudyStrike()));
    }

    [Fact]
    public void CanApplyTo_SkillCard_ReturnsTrue()
    {
        Assert.True(PlannedModifier.CanApplyTo(new UnderstudyDefend()));
    }

    [Fact]
    public void CanApplyTo_RuntimeStableCard_ReturnsFalse()
    {
        // A card made Stable at runtime (via StableModifier, not just the printed keyword) must
        // be just as ineligible as a printed-Stable card like Practice.
        var card = new UnderstudyStrike();
        CardModifier.AddModifier(card, new StableModifier());
        Assert.False(PlannedModifier.CanApplyTo(card));
    }

    [Fact]
    public void CanApplyTo_AlreadyPlannedAttack_ReturnsTrue()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);
        Assert.True(PlannedModifier.CanApplyTo(card));
    }

    [Fact]
    public void GetSorted_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "GetSorted", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void GetSorted_EmptyInput_ReturnsEmpty()
    {
        Assert.Empty(PlannedModifier.GetSorted(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void GetSorted_CardWithTwoSlots_ReturnsTwoEntries()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);
        var sorted = PlannedModifier.GetSorted(new[] { card });
        Assert.Equal(2, sorted.Count);
        Assert.Equal(0, sorted[0].slotSeqIdx);
        Assert.Equal(3, sorted[1].slotSeqIdx);
    }

    [Fact]
    public void CountIn_EmptyInput_ReturnsZero()
    {
        Assert.Equal(0, PlannedModifier.CountIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void CountIn_CountsDistinctCards_NotSlots()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);
        Assert.Equal(1, PlannedModifier.CountIn(new[] { card }));
    }

    [Fact]
    public void TotalSlotCount_EmptyInput_ReturnsZero()
    {
        Assert.Equal(0, PlannedModifier.TotalSlotCount(Enumerable.Empty<CardModel>()));
    }

    // Regression case for the bug this was added to fix: a plan of length 3 where one card holds
    // 2 of those slots and another card holds the 3rd. PlannedCounterPower.DisplayAmount must show
    // 3 (matching the 3 lines CardList lists), not 2 (the distinct-card count CountIn would give).
    [Fact]
    public void TotalSlotCount_OneCardWithTwoSlotsPlusAnotherWithOne_CountsAllSlots()
    {
        var multiSlotCard = new UnderstudyStrike();
        var multiSlotMod = new PlannedModifier();
        multiSlotMod.SequenceIndices.Add(0);
        multiSlotMod.SequenceIndices.Add(2);
        CardModifier.DirectModifiers(multiSlotCard).Add(multiSlotMod);

        var singleSlotCard = new UnderstudyDefend();
        var singleSlotMod = new PlannedModifier();
        singleSlotMod.SequenceIndices.Add(1);
        CardModifier.DirectModifiers(singleSlotCard).Add(singleSlotMod);

        var cards = new CardModel[] { multiSlotCard, singleSlotCard };
        Assert.Equal(3, PlannedModifier.TotalSlotCount(cards));
        Assert.Equal(2, PlannedModifier.CountIn(cards));
        Assert.Equal(3, PlannedModifier.GetSorted(cards).Count);
    }

    [Fact]
    public void AnyIn_EmptyInput_ReturnsFalse()
    {
        Assert.False(PlannedModifier.AnyIn(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void VisualBySeq_DefaultsToEmpty()
    {
        Assert.Empty(new PlannedModifier().VisualBySeq);
    }

    [Fact]
    public void ModifyDescriptionPost_ShowsVisualIndexWhenSet()
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(5);
        mod.VisualBySeq[5] = 2;
        var args = new object?[] { null, "" };
        typeof(PlannedModifier).GetMethod("ModifyDescriptionPost")!.Invoke(mod, args);
        Assert.Contains("#2", (string)args[1]!);
    }

    [Fact]
    public void Remove_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "Remove", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void RemoveSlot_IsStaticMethod()
    {
        var method = typeof(PlannedModifier).GetMethod(
            "RemoveSlot", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
    }

    [Fact]
    public void Remove_ClearsModifierFromCard()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());

        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void RemoveSlot_RemovesSpecificSlot_KeepsOther()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        mod.SequenceIndices.Add(3);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.RemoveSlot(card, 0, Enumerable.Empty<CardModel>());

        Assert.True(card.TryGetModifier<PlannedModifier>(out var remaining));
        Assert.Equal(new[] { 3 }, remaining!.SequenceIndices);
    }

    [Fact]
    public void RemoveSlot_RemovesModifier_WhenLastSlotGone()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);

        PlannedModifier.RemoveSlot(card, 0, Enumerable.Empty<CardModel>());

        Assert.False(card.TryGetModifier<PlannedModifier>(out _));
    }

    // Stable = frozen: a queue resolver (Workshop/Showtime/...) calling RemoveSlot on a Stable
    // card must NOT strip its Planned — the card is still auto-played, but keeps its slot and re-queues.
    // (Regression for: Planned + Stable on Showstopper lost Planned after Workshop.)
    [Fact]
    public void RemoveSlot_StableCard_KeepsSlotAndModifier()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);
        CardModifier.AddModifier(card, new StableModifier());
        Assert.True(card.IsStable());

        PlannedModifier.RemoveSlot(card, 0, Enumerable.Empty<CardModel>());

        Assert.True(card.TryGetModifier<PlannedModifier>(out var remaining));
        Assert.Equal(new[] { 0 }, remaining!.SequenceIndices);
    }

    // "Remove all Planned" effects (Improvise, TabulaRasa) must also refuse to touch a Stable card.
    [Fact]
    public void Remove_StableCard_KeepsPlanned()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);
        CardModifier.AddModifier(card, new StableModifier());

        PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());

        Assert.True(card.TryGetModifier<PlannedModifier>(out _));
    }

    [Fact]
    public void Remove_FiresChangedEvent()
    {
        var card = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);

        bool fired = false;
        PlannedModifier.Changed += () => fired = true;
        try
        {
            PlannedModifier.Remove(card, Enumerable.Empty<CardModel>());
            Assert.True(fired);
        }
        finally
        {
            PlannedModifier.Changed -= () => fired = true;
        }
    }

    [Fact]
    public void RelevantCards_NullPlayer_ReturnsEmpty()
    {
        Assert.Empty(PlannedModifier.RelevantCards(null));
    }

    // QueueNeedsEnemyTarget — whether the Planned queue contains a card that needs the player to
    // pick a single enemy, used by Showtime/Workshop/DaCapo to skip the targeting prompt
    // when nothing queued needs it (empty plan, or only AoE/self/no-target cards).

    [Fact]
    public void QueueNeedsEnemyTarget_EmptyInput_ReturnsFalse()
    {
        Assert.False(PlannedModifier.QueueNeedsEnemyTarget(Enumerable.Empty<CardModel>()));
    }

    [Fact]
    public void QueueNeedsEnemyTarget_OnlyPlannedDefend_ReturnsFalse()
    {
        var defend = new UnderstudyDefend();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(defend).Add(mod);

        Assert.False(PlannedModifier.QueueNeedsEnemyTarget(new[] { defend }));
    }

    [Fact]
    public void QueueNeedsEnemyTarget_PlannedStrike_ReturnsTrue()
    {
        var strike = new UnderstudyStrike();
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(strike).Add(mod);

        Assert.True(PlannedModifier.QueueNeedsEnemyTarget(new[] { strike }));
    }

    [Fact]
    public void QueueNeedsEnemyTarget_MixedPlannedDefendAndStrike_ReturnsTrue()
    {
        var defend = new UnderstudyDefend();
        var defendMod = new PlannedModifier();
        defendMod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(defend).Add(defendMod);

        var strike = new UnderstudyStrike();
        var strikeMod = new PlannedModifier();
        strikeMod.SequenceIndices.Add(1);
        CardModifier.DirectModifiers(strike).Add(strikeMod);

        Assert.True(PlannedModifier.QueueNeedsEnemyTarget(new CardModel[] { defend, strike }));
    }

    [Fact]
    public void QueueNeedsEnemyTarget_UnplannedStrike_ReturnsFalse()
    {
        // A card with TargetType.AnyEnemy that isn't actually in the Planned queue must not count.
        var strike = new UnderstudyStrike();
        Assert.False(PlannedModifier.QueueNeedsEnemyTarget(new[] { strike }));
    }

    // Regression for the StackOverflow crash: a Planned-queue resolver (Showtime/DaCapo, both
    // non-Stable) can itself be Planned, and its TargetType queries the very queue it sits in. Without
    // a re-entrancy guard, QueueNeedsEnemyTarget reads that card's TargetType, which calls back into
    // QueueNeedsEnemyTarget over the same queue — infinite recursion, fatal StackOverflow. This double
    // reproduces that self-referential TargetType (a bare Showtime can't, since Owner-backed
    // RelevantCards needs a live combat).
#pragma warning disable STS001 // test-only card double, no shipped localization
    private sealed class RecursiveResolverCard : UnderstudyCard
    {
        public RecursiveResolverCard() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

        public override TargetType TargetType =>
            PlannedModifier.QueueNeedsEnemyTarget(new CardModel[] { this }) ? TargetType.AnyEnemy : TargetType.None;
    }
#pragma warning restore STS001

    private static T Planned<T>(T card) where T : CardModel
    {
        var mod = new PlannedModifier();
        mod.SequenceIndices.Add(0);
        CardModifier.DirectModifiers(card).Add(mod);
        return card;
    }

    [Fact]
    public void QueueNeedsEnemyTarget_PlannedResolverInQueue_DoesNotRecurse()
    {
        // Would StackOverflow (crashing the process) without the re-entrancy guard.
        var resolver = Planned(new RecursiveResolverCard());
        Assert.False(PlannedModifier.QueueNeedsEnemyTarget(new CardModel[] { resolver }));
    }

    [Fact]
    public void QueueNeedsEnemyTarget_PlannedResolverPlusPlannedStrike_ReturnsTrue()
    {
        // The resolver contributes nothing, but the real AnyEnemy leaf card still requires a target.
        var resolver = Planned(new RecursiveResolverCard());
        var strike = Planned(new UnderstudyStrike());
        Assert.True(PlannedModifier.QueueNeedsEnemyTarget(new CardModel[] { resolver, strike }));
    }
}
