using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Map;

// Forces a flank boss's map-node art into the asset cache. The game preloads only the chosen default and
// second boss's node art (ActModel.MapNodeAssetPaths → _rooms.Boss / _rooms.SecondBoss), so a flank
// boss — an encounter the game never expected on this map — has its art loaded only on demand during
// _Ready, which for the many placeholder-art bosses (BossNodeSpineResource => null, a "{id}_icon.png"
// placeholder) comes back "Asset not cached" and the node draws blank. Preload exactly the paths the node
// will request (MapNodeAssetPaths: the spine .tres for finished bosses, or the two placeholder PNGs for
// the rest) so they are cached and resolvable when _Ready reads them.
public static class AltBossArtPreload
{
    public static void Ensure(EncounterModel enc)
    {
        foreach (var path in enc.MapNodeAssetPaths)
        {
            bool exists = ResourceLoader.Exists(path);
            Resource? loaded = null;
            try { loaded = PreloadManager.Cache.GetAsset<Resource>(path); }
            catch { /* missing/failed asset: node keeps its (blank) placeholder */ }
            Log.Info($"[BookOfOrder] preload {enc.Id}: {path} exists={exists} loaded={loaded != null}");
        }
    }
}
