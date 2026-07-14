using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Notifies subscribers (Take Notes) whenever a debuff on a creature is *removed* — the true
// "a debuff of yours cleared" signal. Driven by DebuffClearOnRemovePatch, a Harmony postfix on
// PowerCmd.Remove, which is the single async choke point every debuff clear funnels through:
// natural per-turn decay and this deck's own Invert conversions reach 0, and PowerCmd.ModifyAmount
// then calls Remove (ShouldRemoveDueToAmount removes any non-AllowNegative debuff the instant it
// hits <= 0); direct removals like base-game Tainted's own AfterSideTurnEnd call Remove straight.
//
// This replaces an earlier hidden tracker power that observed Hook.AfterPowerAmountChanged for a
// decrease landing on 0. That missed direct removals entirely — PowerCmd.Remove fires neither
// AfterPowerAmountChanged nor any other broadcast hook — so Tainted (Infested Prism) clearing
// granted no Vigor. Detecting at the removal point catches every clear exactly once, and correctly
// excludes the combat-end bulk wipe (RemoveAllPowersInternalExcept, which bypasses PowerCmd.Remove)
// so no Vigor is granted as a combat ends.
public static class DebuffClearNotifier
{
    // A Func rather than a true multicast event: it must be awaited, and only one subscriber
    // (Take Notes) is expected at a time in practice. Set/cleared by CryingOutLoudPower on apply /
    // combat end, so this is null — and NotifyDebuffRemoved a no-op — whenever Take Notes is absent.
    public static Func<PlayerChoiceContext, Creature, PowerModel, Task>? DebuffCleared;

    public static async Task NotifyDebuffRemoved(PowerModel? power)
    {
        if (DebuffCleared == null) return;
        if (power == null) return;
        // Crying Out Loud rewards a DEBUFF or a real player BUFF (the invert-economy Un-X powers,
        // Strength, Dexterity, Vigor) clearing. Deliberately NOT every PowerType.Buff: the deck's
        // internal bookkeeping powers (PlannedCounter, AutoTune, Schedule, InvertTracker, Venue, ...)
        // are all Type==Buff too and must never grant Vigor when they end.
        if (power.Type != PowerType.Debuff && !IsRewardableBuff(power)) return;
        // RemovePowerInternal removes the power from the creature's list but does not null its
        // back-reference, so Owner is still readable here (base-game PowerCmd.Remove itself reads
        // power.Owner for AfterRemoved after RemoveInternal). The handler filters by owner.
        Creature owner = power.Owner;
        if (owner == null) return;
        await DebuffCleared(new ThrowingPlayerChoiceContext(), owner, power);
    }

    // The buffs a player thinks of as "theirs" — the invert-economy pay-offs (Un-X), the stat buffs
    // (Strength/Dexterity, which the deck's Invert can create), and Vigor. Excludes the many internal
    // bookkeeping powers that are also PowerType.Buff.
    private static bool IsRewardableBuff(PowerModel power) =>
        power is UnweakPower or UnvulnerablePower or UnshakenPower or UnjadedPower
              or UnlimitedPower or UnfrailPower or StrengthPower or DexterityPower or VigorPower;
}
