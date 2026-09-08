using BaseLib.Patches.UI;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Relic art registry. BaseLib's RelicImageOverridePatch is the only seam for mod relic images — a
// CustomRelicModel exposes no icon-path members of its own — so every relic's three pieces (packed
// icon, outline, big icon) are registered here rather than on the relic classes.
//
// Each piece resolves drop-in: images/relics/<slug>.png, images/relics/outline/<slug>.png and
// images/relics/big/<slug>.png, each null when not yet packed. RelicIconData treats a null member as
// "don't override", so an unmade piece leaves the base game's default in place. Dropping a correctly
// named file into the right folder is the whole adoption step; see art/assets/RELICS_POTIONS.md.
//
// The outline carries a stopgap the other two pieces don't. The reward-pool relics ship no outline art,
// so the base game's PackedIconOutlinePath resolves to a non-existent relic_outline_atlas entry — the
// compendium's colored-shadow branch (NRelicCollectionEntry) then has no Outline texture to tint, which
// is why the gold LabOutlineColor never showed (confirmed via the earlier
// UnderstudyRelicOutlineDiagnosticPatch: every precondition met, willColorOutline=True, but
// iconOutlineTextureNull was True). Those relics therefore fall back to a generic rounded-square outline
// bundled with the mod, which the compendium tints gold, matching base-game character relics.
//
// That fallback stays scoped to the reward pool (Common/Uncommon/Rare/Shop). Starter (False/True Mask)
// and Event (Book of Endings) are deliberately excluded from it — they are still registered here, so
// bespoke art picks them up, but they get no generic outline.
public static class RelicOutlineOverride
{
    private const string GenericOutline = MainFile.ResPath + "/images/generic_outline.png";

    public static void Register()
    {
        // Reward-pool relics: bespoke art if present, generic outline as the floor.
        AddRewardPool<GoldenCape>("golden_cape");
        AddRewardPool<Lampshade>("lampshade");
        AddRewardPool<Greasepaint>("greasepaint");
        AddRewardPool<FoldableStage>("foldable_stage");
        AddRewardPool<DraftingPaper>("drafting_paper");
        AddRewardPool<Rosin>("rosin");
        AddRewardPool<Lozenge>("lozenge");

        // Starter and event relics: bespoke art only, no generic-outline floor.
        AddBespokeOnly<FalseMask>("false_mask");
        AddBespokeOnly<TrueMask>("true_mask");
        AddBespokeOnly<BookOfEndings>("book_of_endings");
    }

    private static void AddRewardPool<TRelic>(string slug) where TRelic : RelicModel =>
        Add<TRelic>(slug, outlineFallback: GenericOutline);

    private static void AddBespokeOnly<TRelic>(string slug) where TRelic : RelicModel =>
        Add<TRelic>(slug, outlineFallback: null);

    private static void Add<TRelic>(string slug, string? outlineFallback) where TRelic : RelicModel
    {
        var file = slug + ".png";
        RelicImageOverridePatch.AddOverride<TRelic>(new RelicIconData(
            BigIconPath: file.BigRelicImagePath(),
            PackedIconPath: file.RelicImagePath(),
            PackedIconOutlinePath: file.RelicOutlineImagePath() ?? outlineFallback));
    }
}
