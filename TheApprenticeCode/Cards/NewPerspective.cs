using System.Linq;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class NewPerspective : ApprenticeCard
{
    public const string CardId = "TheApprentice:NewPerspective";

    public NewPerspective() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var handPile = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (handPile == null) return;

        var unplayable = handPile.Cards
            .Where(c => c.IsUnplayable())
            .ToList();

        await CardCmd.DiscardAndDraw(context, unplayable, unplayable.Count);
    }
}
