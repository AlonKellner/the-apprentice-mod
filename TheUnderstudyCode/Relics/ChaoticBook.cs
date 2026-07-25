using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using BaseLib.Utils;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.RestSite;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Quest form of the Architect's book, found in "The Golden Bedroom". Study it at Rest Sites three
// times — the deliberate sacrifice of three rest actions — and it transforms into the Book of Order.
// This is the base game's SwordOfStone quest-relic pattern: an Event-rarity relic with a persistent
// (SavedProperty) counter that RelicCmd.Replace-s itself at the threshold. Carries [Pool] to satisfy
// the analyzer, but TheUnderstudyRelicPool excludes it as event-only, so it never drops as a reward.
[Pool(typeof(TheUnderstudyRelicPool))]
public class ChaoticBook : CustomRelicModel
{
    public const int StudiesRequired = 3;

    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;
    public override int DisplayAmount => StudyProgress.Remaining(StudyCount, StudiesRequired);

    // Persists across combats/rooms and through save/load (SerializableRelic.Props), unlike the
    // per-combat UnderstudyCounterRelic — the quest spans the whole run.
    [SavedProperty]
    public int StudyCount { get; set; }

    // While the unstudied book is held, offer the one-word "Study" action at Rest Sites. Passes this
    // instance so the option acts on the live relic rather than re-looking it up.
    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner) return false;
        options.Add(new StudyRestSiteOption(player, this));
        return true;
    }

    // Called by StudyRestSiteOption when the player spends a rest action studying.
    public async Task Study()
    {
        StudyCount++;
        InvokeDisplayAmountChanged();
        Flash();
        Log.Info($"[ChaoticBook] StudyCount {StudyCount}/{StudiesRequired}");
        if (StudyProgress.IsComplete(StudyCount, StudiesRequired))
        {
            Log.Info("[ChaoticBook] threshold reached -> transforming to Book of Order");
            await RelicCmd.Replace(this, ModelDb.Relic<BookOfOrder>().ToMutable());
        }
    }
}
