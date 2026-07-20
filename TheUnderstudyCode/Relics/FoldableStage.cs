using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Character;
using TheUnderstudy.TheUnderstudyCode.RestSite;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "You may Enchant a card with Tuned at Rest Sites." Adds a rest-site option that enchants a chosen
// deck card with the PreTuned enchantment (persists across combats), so it starts each combat Tuned.
[Pool(typeof(TheUnderstudyRelicPool))]
public class FoldableStage : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override List<(string, string)>? Localization => new RelicLoc(
        "Foldable Stage",
        "You may Enchant a card with [gold]Tuned[/gold] at Rest Sites.",
        "Rehearse anywhere; the boards travel with you.");

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Tuned) };

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner) return false;
        options.Add(new FoldableStageRestSiteOption(player));
        return true;
    }
}
