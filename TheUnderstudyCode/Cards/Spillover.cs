using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Spillover : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Spillover";

    public Spillover() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(6);
        WithTip(typeof(ShakenPower));
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        int amount = creature.GetPowerAmount<ShakenPower>() + 1;
        await PowerCmd.Apply<WeakPower>(context, CombatState!.HittableEnemies, amount, creature, cardPlay.Card, false);
    }
}
