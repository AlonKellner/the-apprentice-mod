using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheApprentice.TheApprenticeCode.Relics;
using Xunit;

namespace TheApprentice.Tests.Relics;

// Each relic type may only be instantiated once (CustomContentDictionary rejects duplicates).
public class RelicTests
{
    // ── Type hierarchy (no instantiation needed) ─────────────────────────────

    [Fact]
    public void ConstantStruggle_InheritsFromCustomRelicModel() =>
        Assert.True(typeof(ConstantStruggle).IsSubclassOf(typeof(CustomRelicModel)));

    [Fact]
    public void PoeticStruggle_InheritsFromCustomRelicModel() =>
        Assert.True(typeof(PoeticStruggle).IsSubclassOf(typeof(CustomRelicModel)));

    // ── Rarity (one instantiation each) ──────────────────────────────────────

    [Fact]
    public void ConstantStruggle_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new ConstantStruggle().Rarity);

    [Fact]
    public void PoeticStruggle_IsStarterRarity() =>
        Assert.Equal(RelicRarity.Starter, new PoeticStruggle().Rarity);

    // ── Upgrade link ──────────────────────────────────────────────────────────

    [Fact]
    public void ConstantStruggle_GetUpgradeReplacement_IsOverridden()
    {
        var method = typeof(ConstantStruggle).GetMethod(nameof(CustomRelicModel.GetUpgradeReplacement));
        Assert.NotNull(method);
        Assert.Equal(typeof(ConstantStruggle), method!.DeclaringType);
    }

    [Fact]
    public void PoeticStruggle_GetUpgradeReplacement_IsNotOverridden()
    {
        // PoeticStruggle is the terminal upgrade — it must NOT chain further
        var method = typeof(PoeticStruggle).GetMethod(nameof(CustomRelicModel.GetUpgradeReplacement));
        Assert.NotEqual(typeof(PoeticStruggle), method?.DeclaringType);
    }

    // ── Hand draw count ───────────────────────────────────────────────────────

    [Fact]
    public void ConstantStruggle_ModifyHandDraw_IsOverridden()
    {
        // Must declare ModifyHandDraw to subtract 1 draw (compensates for BeforeHandDraw card)
        var method = typeof(ConstantStruggle).GetMethod(
            "ModifyHandDraw",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }

    [Fact]
    public void PoeticStruggle_ModifyHandDraw_IsOverridden()
    {
        var method = typeof(PoeticStruggle).GetMethod(
            "ModifyHandDraw",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
        Assert.NotNull(method);
    }
}
