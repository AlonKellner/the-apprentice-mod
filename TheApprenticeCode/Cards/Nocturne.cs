using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Nocturne : ApprenticeCard
{
    public const string CardId = "TheApprentice:Nocturne";

    public Nocturne() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithTip(typeof(Dream));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 3, IsUpgraded);
    }
}
