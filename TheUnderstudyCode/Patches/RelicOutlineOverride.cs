using BaseLib.Patches.UI;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// The reward-pool relics ship no icon/outline art, so the base game's PackedIconOutlinePath resolves to a
// non-existent relic_outline_atlas entry — the compendium's colored-shadow branch (NRelicCollectionEntry)
// then has no Outline texture to tint, which is why the gold LabOutlineColor never showed (confirmed via
// the earlier UnderstudyRelicOutlineDiagnosticPatch: every precondition met, willColorOutline=True, but
// iconOutlineTextureNull was True). As a stopgap, register a generic rounded-square outline shape bundled
// with the mod (via BaseLib's RelicImageOverridePatch) as each relic's PackedIconOutlinePath; the
// compendium tints it to the character's gold, matching base-game character relics. Only the outline is
// overridden — icon/big stay at the default placeholder until real per-relic art exists.
//
// Scope: the reward-pool relics only (Common/Uncommon/Rare/Shop). Starter (False/True Mask) and Event
// (Book of Endings) are deliberately excluded.
public static class RelicOutlineOverride
{
    private const string GenericOutline = MainFile.ResPath + "/images/relics/generic_relic_outline.png";

    public static void Register()
    {
        var outlineOnly = new RelicIconData(BigIconPath: null, PackedIconPath: null, PackedIconOutlinePath: GenericOutline);
        RelicImageOverridePatch.AddOverride<GoldenCape>(outlineOnly);
        RelicImageOverridePatch.AddOverride<Lampshade>(outlineOnly);
        RelicImageOverridePatch.AddOverride<Greasepaint>(outlineOnly);
        RelicImageOverridePatch.AddOverride<FoldableStage>(outlineOnly);
        RelicImageOverridePatch.AddOverride<DraftingPaper>(outlineOnly);
        RelicImageOverridePatch.AddOverride<Rosin>(outlineOnly);
        RelicImageOverridePatch.AddOverride<Lozenge>(outlineOnly);
    }
}
