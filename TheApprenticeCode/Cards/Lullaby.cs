using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Lullaby : ApprenticeCard
{
    public const string CardId = "TheApprentice:Lullaby";

    public Lullaby() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CreatureCmd.TriggerAnim(creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        decimal flag = IsUpgraded ? 2m : 1m;
        await PowerCmd.Apply<LullabyPower>(context, creature, flag, creature, cardPlay.Card, false);
    }
}
