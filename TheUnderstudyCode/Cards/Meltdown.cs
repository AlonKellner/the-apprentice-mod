using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// AoE emotional spill: hit everyone, and leave yourself and every enemy exposed. (Vulnerable =
// Emotional theme.)
public class Meltdown : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Meltdown";

    public Meltdown() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(10);
        WithDebuffTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, this);
        var enemies = creature.CombatState!.HittableEnemies;
        await PowerCmd.Apply<VulnerablePower>(context, enemies, 1, creature, cardPlay.Card, false);
    }
}
