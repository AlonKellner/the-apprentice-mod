using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Lucidity : ApprenticeCard
{
    public const string CardId = "TheApprentice:Lucidity";

    public Lucidity() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(Dream));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int toAdd = IsUpgraded ? 2 : 1;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, toAdd, upgraded: true);

        var hand = player.Piles.Where(p => p.Type == PileType.Hand).SelectMany(p => p.Cards);
        foreach (var card in hand.Where(c => c is Dream && !c.IsUpgraded).ToList())
            CardCmd.Upgrade(card, CardPreviewStyle.None);
    }
}
