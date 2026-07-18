using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Get the whole room singing: everyone gains Vigor. A symmetric payoff-enabler (feeds enemies too),
// so it wants Vigor-drain / Reverb backing. (Vigor = Sounds theme.)
public class SingAlong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SingAlong";

    public SingAlong() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        // "All creatures in combat" = every ally (all players + their pets/Osty in multiplayer) AND
        // every enemy. CombatState.Creatures is _allies.Concat(_enemies) — the full room, mirroring
        // how LegionOfBone reaches every player's creature in co-op.
        var all = creature.CombatState!.Creatures.Where(c => c?.IsAlive ?? false).ToList();
        await PowerCmd.Apply<VigorPower>(context, all, 6, creature, cardPlay.Card, false);
    }
}
