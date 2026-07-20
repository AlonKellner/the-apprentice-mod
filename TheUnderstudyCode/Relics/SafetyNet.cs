using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "Start each combat with 1 Unvulnerable and 1 Unfrail." — combat-start one-shot idiom (Anchor /
// Vajra / OddlySmoothStone). Applied on entering a combat room via a ThrowingPlayerChoiceContext,
// exactly like base OddlySmoothStone/Vajra (BeforeCombatStart carries no PlayerChoiceContext, and a
// flat power grant needs no player choice).
[Pool(typeof(TheUnderstudyRelicPool))]
public class SafetyNet : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override List<(string, string)>? Localization => new RelicLoc(
        "Safety Net",
        "Start each combat with 1 [gold]Unvulnerable[/gold] and 1 [gold]Unfrail[/gold].",
        "No fall is too far when someone's rigged the catch.");

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.FromPower<UnvulnerablePower>(),
        HoverTipFactory.FromPower<UnfrailPower>(),
    };

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom) return;
        Flash();
        await PowerCmd.Apply<UnvulnerablePower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null, false);
        await PowerCmd.Apply<UnfrailPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1, Owner.Creature, null, false);
    }
}
