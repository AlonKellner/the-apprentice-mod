using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Crash : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Crash";

    public Crash() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(7);
        WithVars(new IntVar("Vigor", 3));
        WithTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Vigor"].UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var creature = cardPlay.Card.Owner.Creature;
        int vigor = (int)DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, this, false);
    }
}
