using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Encore : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Encore";

    public Encore() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithPowerNoTip<EncorePower>(1);
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<EncorePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
