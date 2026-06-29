using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Buildup : ApprenticeCard
{
    public const string CardId = "TheApprentice:Buildup";

    public Buildup() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int current = TensionHelper.GetTension(creature);
        if (current > 0)
            await PowerCmd.Apply<TensionPower>(context, creature, current, creature, cardPlay.Card, false);
        if (IsUpgraded)
            await PlayerCmd.GainEnergy(1, cardPlay.Card.Owner);
    }
}
