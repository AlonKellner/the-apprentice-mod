using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Reverb: "Retain your Vigor after using it this turn." This patch covers the attack path — VigorPower
// is a sealed base-game power that, in its AfterAttack hook, consumes ALL of its amount on the first
// powered attack (PowerCmd.ModifyAmount by -amountWhenAttackStarted). It exposes no skip flag, so —
// mirroring DebuffClearOnRemovePatch's approach to sealed base choke points — we prefix AfterAttack and,
// while the attacker has ReverbPower, cancel that consumption for the turn. (DeceptiveCadence's own
// Vigor use is retained separately, by checking ReverbPower.IsActive before it spends Vigor.)
//
// We also null out VigorPower's private Data.commandToModify. Two reasons:
//   * VigorPower.ModifyDamageAdditive returns 0 for a DIFFERENT card's attack while commandToModify is
//     still latched to the first attack's command — so without clearing it, only same-card multi-hits
//     would keep the Vigor bonus. Clearing it lets each subsequent attack this turn re-latch and benefit.
//   * It leaves VigorPower in its pristine "unlatched" state, so once Reverb expires next turn the very
//     first powered attack consumes Vigor normally again.
[HarmonyPatch(typeof(VigorPower), nameof(VigorPower.AfterAttack))]
public static class ReverbVigorRetentionPatch
{
    // PowerModel._internalData holds the boxed per-instance Data object (see PowerModel.GetInternalData).
    private static readonly FieldInfo InternalDataField =
        typeof(PowerModel).GetField("_internalData", BindingFlags.Instance | BindingFlags.NonPublic)!;

    // VigorPower.Data is a private nested type with an AttackCommand? commandToModify field.
    private static readonly FieldInfo CommandToModifyField =
        typeof(VigorPower).GetNestedType("Data", BindingFlags.NonPublic)!
            .GetField("commandToModify", BindingFlags.Instance | BindingFlags.Public)!;

    [HarmonyPrefix]
    public static bool Prefix(VigorPower __instance, AttackCommand command, ref Task __result)
    {
        if (!ReverbPower.IsActive(command.Attacker))
            return true; // no Reverb — let Vigor consume itself normally

        var data = InternalDataField.GetValue(__instance);
        // Only intervene on the attack VigorPower actually latched onto (the one it would consume for).
        if (data == null || !ReferenceEquals(CommandToModifyField.GetValue(data), command))
            return true;

        // Retain Vigor: unlatch and skip the consuming ModifyAmount entirely.
        CommandToModifyField.SetValue(data, null);
        __result = Task.CompletedTask;
        return false;
    }
}
