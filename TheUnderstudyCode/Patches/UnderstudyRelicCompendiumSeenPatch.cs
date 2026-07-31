using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using MegaCrit.Sts2.Core.Saves;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Marks the Understudy's relics "seen" so the compendium (NRelicCollection) renders them in full — with
// the character's gold LabOutlineColor shadow — like base-game character relics, instead of as dark
// "unknown" tiles. The colored-shadow branch in NRelicCollectionEntry only runs for Visible (seen) relics;
// an unseen relic shows a plain half-transparent-white outline no matter what the pool color is, which is
// why just setting LabOutlineColor did nothing on its own.
//
// BaseLib offers CustomRelicPoolModel.SeenByDefault for exactly this, but that marks EVERY relic in the
// pool — which would reveal the Book of Endings, a secret event relic that must stay hidden until it is
// obtained from The Golden Bedroom. So this pool leaves SeenByDefault false and we mark seen ourselves,
// skipping the Book of Endings. Marking is idempotent (a save-set add) and mirrors BaseLib's own
// CustomRelicPoolMarkAsSeenPatch prefix on the same method. (Epoch-gated relics still show Locked until
// their epoch is revealed; Locked takes precedence over seen.)
[HarmonyPatch(typeof(NRelicCollection), "LoadRelics")]
public static class UnderstudyRelicCompendiumSeenPatch
{
    [HarmonyPrefix]
    private static void MarkSeenExceptBookOfEndings()
    {
        foreach (RelicModel relic in ModelDb.RelicPool<TheUnderstudyRelicPool>().AllRelics)
            if (relic is not BookOfEndings)
                SaveManager.Instance.MarkRelicAsSeen(relic);
    }
}
