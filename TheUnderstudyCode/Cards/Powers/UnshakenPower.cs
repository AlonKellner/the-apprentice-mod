using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class UnshakenPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Unshaken",
        "At the start of your turn, remove [gold]Unplayable[/gold] from all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.",
        "At the start of your turn, remove [gold]Unplayable[/gold] from all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList().Where(c => c.Type == CardType.Attack || c.Type == CardType.Skill))
        {
            if (card.TryGetModifier<UnplayableModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);
        }
        await PowerCmd.Decrement(this);
    }
}
