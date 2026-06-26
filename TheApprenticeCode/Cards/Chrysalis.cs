using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Chrysalis : ApprenticeCard
{
    public const string CardId = "TheApprentice:Chrysalis";

    public Chrysalis() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Potential>(upgrade: card.IsUpgraded)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.Where(p => p.Type == PileType.Hand).SelectMany(p => p.Cards);
        var toExhaust = hand.Where(c => c is Dream or Ambition).ToList();

        foreach (var card in toExhaust)
            await CardCmd.Exhaust(context, card, false, false);

        await DreamsAndAmbitions.AddPotentials(player, CombatState!, toExhaust.Count, IsUpgraded);
    }
}
