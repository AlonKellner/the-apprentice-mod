using TheApprentice.TheApprenticeCode.Cards.Powers;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Attunement : ApprenticeCard
{
    public const string CardId = "TheApprentice:Attunement";

    public Attunement() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        var handCards = hand?.Cards ?? System.Array.Empty<MegaCrit.Sts2.Core.Models.CardModel>();
        int tokens = DreamsAndAmbitions.CountAll(handCards.Where(c => c != cardPlay.Card));
        int tensionPerToken = IsUpgraded ? 3 : 2;
        if (tokens > 0)
            await TensionHelper.AddTension(context, player.Creature, tokens * tensionPerToken, cardPlay.Card);
        await CommonActions.Draw(cardPlay.Card, context);
    }
}
