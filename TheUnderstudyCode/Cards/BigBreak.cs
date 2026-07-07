using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class BigBreak : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BigBreak";

    protected override bool HasEnergyCostX => true;

    public BigBreak() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int x = ResolveEnergyXValue();
        int perStack = IsUpgraded ? 6 : 4;
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, perStack * x, creature, this, false);
    }
}
