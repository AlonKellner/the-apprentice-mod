using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "At the start of your turn, gain 2 Vigor." — the small-passive-per-turn idiom (Brimstone /
// MercuryHourglass; Akabeko grants 8 Vigor as a one-shot for comparison).
[Pool(typeof(TheUnderstudyRelicPool))]
public class Lozenge : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override List<(string, string)>? Localization => new RelicLoc(
        "Lozenge",
        "At the start of your turn, gain 2 [gold]Vigor[/gold].",
        "For the voice that must go on.");

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<VigorPower>() };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        Flash();
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, 2, Owner.Creature, null, false);
    }
}
