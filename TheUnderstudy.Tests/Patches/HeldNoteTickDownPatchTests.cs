using System.Linq;
using System.Reflection;
using TheUnderstudy.TheUnderstudyCode.Patches;
using Xunit;

namespace TheUnderstudy.Tests.Patches;

// Held Note — "Turn-based buffs and debuffs no longer decrease by 1 each turn."
//
// Held Note's freeze is implemented as a Harmony prefix on PowerCmd.TickDownDuration, the single
// shared per-turn decrement gate that EVERY base-game duration power routes through (Weak, Vulnerable,
// Frail, Blur, Intangible, Temporary Strength/Dexterity/Focus, Dampen, Shrink, Constrict, Poison
// count, NoDraw/NoBlock/NoEnergy, ...). Skipping that call when the power's owner has Held Note freezes
// all of them at once, without enumerating them.
//
// The actual across-a-turn freeze (a debuff still present next turn; a permanent power like Strength
// unaffected; Poison still deals its damage) is turn-tick timing and is verified in-game — the bare
// xUnit host has no ModelDb/combat. These reflection guards instead lock in the wiring so the freeze
// can never silently detach (e.g. a rename of TickDownDuration, or the prefix losing its bool return).
public class HeldNoteTickDownPatchTests
{
    private static object PatchAttr() =>
        typeof(HeldNoteTickDownPatch).GetCustomAttributes(false)
            .First(a => a.GetType().Name == "HarmonyPatch");

    [Fact]
    public void Patch_IsAHarmonyPatch() =>
        Assert.Contains(typeof(HeldNoteTickDownPatch).GetCustomAttributes(false),
            a => a.GetType().Name == "HarmonyPatch");

    [Fact]
    public void Patch_TargetsPowerCmd_TickDownDuration()
    {
        var attr = PatchAttr();
        var info = attr.GetType().GetField("info")?.GetValue(attr);
        var methodName = info?.GetType().GetField("methodName")?.GetValue(info) as string;
        var declaringType = info?.GetType().GetField("declaringType")?.GetValue(info) as System.Type;
        Assert.Equal("TickDownDuration", methodName);
        Assert.Equal("PowerCmd", declaringType?.Name);
    }

    [Fact]
    public void Patch_PrefixReturnsBool_SoItCanSkipTheDecrement()
    {
        var prefix = typeof(HeldNoteTickDownPatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(prefix);
        Assert.Equal(typeof(bool), prefix!.ReturnType);
        Assert.Contains(prefix.GetCustomAttributes(false), a => a.GetType().Name == "HarmonyPrefix");
    }
}
