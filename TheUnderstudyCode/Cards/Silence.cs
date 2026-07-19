using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Cut the music: AoE hit that drains loudness from the whole room, yourself included. (Vigor =
// Sounds theme.)
public class Silence : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Silence";

    public Silence() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(6);
        WithMarkedTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, -6, creature, this, false);
        var enemies = creature.CombatState!.HittableEnemies;
        await PowerCmd.Apply<VigorPower>(context, enemies, -6, creature, cardPlay.Card, false);
    }
}
