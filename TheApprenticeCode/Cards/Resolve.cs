using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Resolve : ApprenticeCard
{
    public const string CardId = "TheApprentice:Resolve";

    public Resolve() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int toAdd = IsUpgraded ? 2 : 1;
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, toAdd, upgraded: true);

        var hand = player.Piles.Where(p => p.Type == PileType.Hand).SelectMany(p => p.Cards);
        foreach (var card in hand.Where(c => c is Ambition && !c.IsUpgraded).ToList())
            CardCmd.Upgrade(card, CardPreviewStyle.None);
    }
}
