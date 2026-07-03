using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class ShakenPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Shaken",
        "At the end of your turn, add [gold]Unplayable[/gold] to all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.",
        "At the end of your turn, add [gold]Unplayable[/gold] to all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand.");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;
        var hand = Owner.Player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList().Where(c =>
            (c.Type == CardType.Attack || c.Type == CardType.Skill)
            && !c.Keywords.Contains(UnderstudyKeywords.Stable)))
        {
            if (!card.TryGetModifier<UnplayableModifier>(out _))
                CardModifier.AddModifier<UnplayableModifier>(card);
        }
        if (!HeldNotePower.IsActive(Owner))
            await PowerCmd.Decrement(this);
    }
}
