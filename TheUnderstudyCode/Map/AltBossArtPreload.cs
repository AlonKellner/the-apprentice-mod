using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// Forces a flank boss's map-node art into the asset cache. The game preloads only the chosen default
// and second boss's node art (ActModel.MapNodeAssetPaths adds _rooms.Boss / _rooms.SecondBoss), so a
// flank boss — an encounter the game never expected on this map — has no loaded art, and
// EncounterModel.BossNodeSpineResource's ResourceLoader.Exists gate returns null → the node draws blank.
// Loading the skeleton .tres into PreloadManager.Cache before the node's _Ready runs makes the resource
// resolvable so the real animated boss art renders. Also logs before/after existence so we can tell a
// "not cached" case (fixable here) from a "resource genuinely absent" case (needs a different source).
public static class AltBossArtPreload
{
    public static void Ensure(EncounterModel enc)
    {
        var path = enc.BossNodePath;
        bool existsBefore = ResourceLoader.Exists(path);

        Resource? loaded = null;
        try { loaded = PreloadManager.Cache.GetAsset<Resource>(path); }
        catch { /* missing/failed asset: node keeps its placeholder */ }

        Log.Info($"[BookOfOrder] preload {enc.Id}: {path} existsBefore={existsBefore} " +
                 $"loaded={(loaded != null)} existsAfter={ResourceLoader.Exists(path)}");
    }
}
