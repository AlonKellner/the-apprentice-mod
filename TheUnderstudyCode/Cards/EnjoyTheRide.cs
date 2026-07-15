using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class EnjoyTheRide : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:EnjoyTheRide";

    public EnjoyTheRide() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(9);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        // Arm a one-shot reactive Invert: the next time an invertible debuff is modified, Invert 2 of it.
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<EnjoyTheRidePower>(context, creature, 2, creature, this, false);
    }
}
