using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// DIAGNOSTIC (temporary): the Understudy's reward-pool relics aren't getting the gold compendium outline
// that base-game character relics have. The colored-outline branch in NRelicCollectionEntry._Ready runs
// only when BOTH (1) the tile is Visible (unlocked AND seen) and (2) a pool in ModelDb.AllCharacterRelicPools
// has AllRelicIds.Contains(relic.Id) — it reads that pool's LabOutlineColor. NRelicCollectionEntry.Create
// receives the already-computed ModelVisibility, so this postfix logs, per Understudy relic, exactly which
// of those preconditions is failing (plus a one-time dump of every character relic pool + its color, to
// see whether the Understudy pool is even in that list and how it compares to the base-game pools).
//
// Interpreting the log:
//   visibility=Locked   -> not in the unlocked set (epoch-gated: Greasepaint/Rosin/Lozenge until ep1). Expected.
//   visibility=NotSeen  -> unlocked but not marked seen -> UnderstudyRelicCompendiumSeenPatch isn't taking.
//   visibility=Visible  -> good; then owningCharacterPool==NONE or understudyPoolInList==false is the culprit
//                          (the color loop can't find a character pool for the relic), else the color IS
//                          being set and the problem is elsewhere (alpha / render / a different tile).
[HarmonyPatch(typeof(NRelicCollectionEntry), nameof(NRelicCollectionEntry.Create))]
public static class UnderstudyRelicOutlineDiagnosticPatch
{
    private static bool _loggedPools;

    // Base-game character relics that DO get the colored outline — logged as a control so we can compare a
    // working relic's icon/outline resolution against the Understudy's broken ones.
    private static readonly string[] ControlRelics = { "RED_SKULL", "SNECKO_SKULL", "FENCING_MANUAL" };

    [HarmonyPostfix]
    public static void Postfix(RelicModel relic, ModelVisibility visibility)
    {
        var pool = ModelDb.RelicPool<TheUnderstudyRelicPool>();
        bool isUnderstudy = pool.AllRelicIds.Contains(relic.Id);
        string idStr = relic.Id.ToString();
        bool isControl = ControlRelics.Any(n => idStr.Contains(n));
        if (!isUnderstudy && !isControl) return; // Understudy relics + a few base-game controls
        string owner = isUnderstudy ? "UNDERSTUDY" : "BASEGAME";

        var charPools = ModelDb.AllCharacterRelicPools.ToList();

        if (!_loggedPools)
        {
            _loggedPools = true;
            Log.Info("[UnderstudyRelicOutline] AllCharacterRelicPools = [" +
                     string.Join(", ", charPools.Select(p => $"{p.GetType().Name}(color={p.LabOutlineColor}, relics={p.AllRelicIds.Count})")) + "]");
        }

        bool understudyPoolInList = charPools.Any(p => p is TheUnderstudyRelicPool);
        var owningCharacterPool = charPools.FirstOrDefault(p => p.AllRelicIds.Contains(relic.Id));
        bool willColorOutline = visibility == ModelVisibility.Visible && owningCharacterPool != null;

        Log.Info($"[UnderstudyRelicOutline] [{owner}] {relic.Id} rarity={relic.Rarity} visibility={visibility} " +
                 $"understudyPoolInList={understudyPoolInList} " +
                 $"owningCharacterPool={owningCharacterPool?.GetType().Name ?? "NONE"} " +
                 $"labOutlineColor={(owningCharacterPool != null ? owningCharacterPool.LabOutlineColor.ToString() : "n/a")} " +
                 $"willColorOutline={willColorOutline}");

        // The tint is set on the Outline TextureRect, whose TEXTURE is Model.IconOutline. If that texture
        // is null/missing (custom relics have no entry in the base-game relic_outline_atlas and register no
        // override), SelfModulate has nothing to color. Log whether the icon/outline resources exist.
        string? outlinePath = typeof(RelicModel)
            .GetProperty("PackedIconOutlinePath", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?.GetValue(relic) as string;
        bool iconExists = ResourceLoader.Exists(relic.PackedIconPath);
        bool outlineExists = outlinePath != null && ResourceLoader.Exists(outlinePath);
        Log.Info($"[UnderstudyRelicOutline]   [{owner}] assets: iconPath={relic.PackedIconPath} iconExists={iconExists} " +
                 $"outlinePath={outlinePath ?? "null"} outlineExists={outlineExists} iconOutlineTextureNull={relic.IconOutline == null}");
    }
}
