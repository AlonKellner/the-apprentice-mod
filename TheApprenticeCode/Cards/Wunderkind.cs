using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Wunderkind : ApprenticeCard
{
    public const string CardId = "TheApprentice:Wunderkind";

    public Wunderkind() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(new TooltipSource(_ => HoverTipFactory.FromCard<Dream>(upgrade: true)));
        WithTip(new TooltipSource(_ => HoverTipFactory.FromCard<Ambition>(upgrade: true)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<WunderkindPower>(context, creature, 1m, creature, cardPlay.Card, false);
    }
}
