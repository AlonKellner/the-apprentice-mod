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
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class ShakenPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Shaken",
        "At the end of your turn, add [gold]Unplayable[/gold] to all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand. [gold]Invertible[/gold].",
        "At the end of your turn, add [gold]Unplayable[/gold] to all [gold]Attacks[/gold] and [gold]Skills[/gold] in your hand. [gold]Invertible[/gold].");

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != CombatSide.Player || Owner.Player == null) return;
        var hand = Owner.Player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList().Where(c =>
            (c.Type == CardType.Attack || c.Type == CardType.Skill) && !c.IsStable()))
        {
            // Muscle Memory immunity (Tuned cards) is enforced centrally in
            // UnplayableModifier.OnInitialApplication — the attach below simply doesn't stick for a
            // Tuned card under Muscle Memory. Shaken still locks any other eligible Attack/Skill.
            if (!card.TryGetModifier<UnplayableModifier>(out _))
                CardModifier.AddModifier<UnplayableModifier>(card);
        }
        if (!HeldNotePower.IsActive(Owner))
        {
            Invariants.Check(Amount > 0, nameof(ShakenPower) + "." + nameof(BeforeSideTurnEnd),
                "about to decrement a Counter power that is already at 0 or below");
            await PowerCmd.Decrement(this);
        }
    }
}
