using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Conviction : ApprenticeCard
{
    public const string CardId = "TheApprentice:Conviction";

    public Conviction() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
        WithAmbitionKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal flag = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<ConvictionPower>(context, creature, flag, creature, cardPlay.Card, false);
    }
}
